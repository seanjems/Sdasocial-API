using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Transactions;
using Newtonsoft.Json;
using sdakccapi.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using sdakccapi.Models.Entities;
using sdakccapi.Dtos.Cart;
using sdakccapi.StaticDetails;
using Microsoft.AspNetCore.Identity;
using sdakccapi.Controllers;
using sdakccapi.Dtos.Users;
using sdakccapi.Controllers.SmtpClient;
using sdakccapi.Dtos.EmailDto;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using sdakccapi.Controllers.MtnMomo;
using sdakccapi.Dtos.MtnMomoDto;

namespace BillTrickOnline.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    [Authorize]
    public class CartController : Controller
    {
        private readonly sdakccapiDbContext context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly AuthorizationController _authorizationController;
        private readonly SmtpSenderController _smtpSenderController;
        private readonly MtnmomoController _mtnmomoController;

        public CartController(sdakccapiDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AuthorizationController authorizationController, SmtpSenderController smtpSenderController, MtnmomoController mtnmomoController)
        {
            this.context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _authorizationController = authorizationController;
            _smtpSenderController = smtpSenderController;
            _mtnmomoController = mtnmomoController;
        }

        //POST paypal details after payment
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AfterPayPalPayment(OffertoryCartDto offertoryCartDto)
        {


            if (!string.IsNullOrEmpty(offertoryCartDto.paypalString))
            {
                var payPalObj = JsonConvert.DeserializeObject<JToken>(offertoryCartDto.paypalString);
                var PaymentStatus = payPalObj["status"];
                decimal paidAmount = ((decimal)payPalObj["purchase_units"][0]["amount"]["value"]);


                //ToDo: call paypalapi and verify payment
                try
                {

                    if (PaymentStatus.ToString().ToLower() == "completed")
                    {

                        using (var scope = context.Database.BeginTransaction())
                        {

                            //create customer

                            Customer customer = new Customer()
                            {
                                CountryCode = (payPalObj?["payer"]?["address"]?["country_code"]?.ToString()),
                                Email = (payPalObj?["payer"]?["email_address"]?.ToString()),
                                FullName = (payPalObj?["payer"]?["name"]?["given_name"]?.ToString()+" "+ payPalObj?["payer"]?["name"]?["surname"]?.ToString())
                            };
                            var objFromDB = context.customers.FirstOrDefault(c => c.Email == customer.Email);
                            if (objFromDB == null)
                            {
                                context.customers.Add(customer);
                                context.SaveChanges();

                            }
                            else
                            {
                                customer = objFromDB;
                            }

                            //create customer as user
                            AppUser systemUser = await _userManager.FindByEmailAsync(customer.Email);

                            if (systemUser == null)
                            {
                                systemUser = new AppUser()
                                {
                                    Email = customer.Email,
                                    FirstName = (bool)customer?.FullName.Contains(" ") ? customer?.FullName?.Substring(0, customer.FullName.IndexOf(" ") + 1) : customer.FullName,
                                    Lastname = (bool)customer?.FullName.Contains(" ") ? customer?.FullName?.Substring(customer.FullName.IndexOf(" ")) : "",
                                    UserName = customer.FullName.ToLower().Replace(" ", "")

                                };

                                var result = await _userManager.CreateAsync(systemUser);
                                if (result.Succeeded)
                                {
                                    var requestor = Url.Action(new UrlActionContext
                                    {
                                        Protocol = Request.Scheme,
                                        Host = Request.Host.Value,
                                        Action = "Action"
                                    });
                                    //send pass word generation email
                                    _smtpSenderController.sendPasswordResetEmail(systemUser, requestor);
                                }
                                else if (result.Errors.FirstOrDefault()?.Code == "DuplicateUserName")
                                {
                                    // username exists auto generate new              
                                    systemUser.UserName = await _authorizationController.GenerateUserName(systemUser.UserName);

                                    //try again creating once
                                    result = await _userManager.CreateAsync(systemUser);
                                    if (result.Succeeded)
                                    {
                                        //send password generation email
                                        var requestor = Url.Action(new UrlActionContext
                                        {
                                            Protocol = Request.Scheme,
                                            Host = Request.Host.Value,
                                            Action = "Action"
                                        });
                                        _smtpSenderController.sendPasswordResetEmail(systemUser, requestor);
                                    }

                                }
                            }


                            //create order
                            List<CartDetail> cart = offertoryCartDto.items ?? new List<CartDetail>();
                            decimal GrandTotal = Math.Round(cart.Sum(x => x.quantity * x.price), 2);

                            Orders orders = new Orders()
                            {
                                change = GrandTotal - paidAmount,
                                CreatedDate = DateTime.UtcNow,
                                customerId = customer.CustomerID,
                                saleTotal = GrandTotal,
                                transactionStatus = (int)statics.SaleStatus.ClosedInstantly,
                                transactionComment = "PayPal Offertory"
                            };
                            context.orders.Add(orders);
                            context.SaveChanges();

                            //create orderdetail
                            int sortOrder = 1;

                            foreach (var item in offertoryCartDto.items)
                            {
                                OrderDetail orderDetail = new OrderDetail()
                                {
                                    orderId = orders.Id,
                                    productSlug = item.title.ToLower().Replace(" ", "-"),
                                    priceInc = item.price,
                                    qty = item.quantity,
                                    productID = item.id,
                                    totalPriceInc = item.quantity * item.price,
                                    sortOrder = sortOrder,
                                };
                                sortOrder++;
                                context.orderDetails.Add(orderDetail);

                                //create user role for claims for the lugogo ambasodors show

                                if (orderDetail.productSlug == "lugogo" || orderDetail.productSlug == "imperial")
                                {
                                    var check = await _roleManager.FindByNameAsync(orderDetail.productSlug);
                                    var role = await _userManager.AddToRoleAsync(systemUser, orderDetail.productSlug);
                                }


                            }

                            //create payment
                            Payments payments = new Payments()
                            {
                                Amount = paidAmount,
                                BankID = -1,
                                BankingDate = default(DateTime),
                                CardRef = "",
                                ChequeNo = "",
                                CustomerDepositID = -1,
                                CustomerID = (int)customer.CustomerID,
                                ExpenseID = -1,
                                NameOnCheque = "",
                                PaymentDetailJson = offertoryCartDto.paypalString,
                                PaymentEmail = (payPalObj?["payer"]?["email_address"]?.ToString()),
                                PaymentModeID = (int)statics.PaymentModes.Paypal,
                                SaleID = orders.Id, //to be editted
                                SupplierID = -1,
                                SupplierPaymentID = -1
                            };


                            context.payments.Add(payments);
                            context.SaveChanges();
                            scope.CommitAsync();
                            //clear session

                            var emailDto = new EmailDto(true);
                            emailDto.Subject = "Payment Received";
                            emailDto.ToEmail = systemUser.Email;
                            emailDto.Body = $"Hello {systemUser.FirstName}<br/><br/>" +
                                $"Your payment of {offertoryCartDto.orderCurrency} {orders.saleTotal} has been received" +
                                $"  <br/><br/>Thank you for being part of SDA Kampala central church.";

                            Task.Run(()=>_smtpSenderController.SendMail(emailDto));
                            return Ok("Payment received");

                        }

                    }
                    return BadRequest("Complete Payment first");

                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

            }
            return BadRequest();
        }

        //POST paypal details after payment
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> BeforeMomoPayment(CreateUserDto offertoryCartDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var baseLink = Request != null ? $"{Request?.Scheme}://{Request?.Host.Value}/" : null;
            decimal paidAmount = offertoryCartDto.items.Sum(x => x.price * x.quantity);

            //ToDo: call paypalapi and verify payment
            try
            {

                using (var scope = context.Database.BeginTransaction())
                {

                    //create customer

                    Customer customer = new Customer()
                    {

                        Email = offertoryCartDto.EmailAddress,
                        FullName = $"{offertoryCartDto.Name} {offertoryCartDto}"
                    };
                    var objFromDB = context.customers.FirstOrDefault(c => c.Email == customer.Email);
                    if (objFromDB == null)
                    {
                        context.customers.Add(customer);
                        context.SaveChanges();

                    }
                    else
                    {
                        customer = objFromDB;
                    }

                    //create customer as user
                    AppUser systemUser = await _userManager.FindByEmailAsync(customer.Email);

                    if (systemUser == null)
                    {
                        systemUser = new AppUser()
                        {
                            Email = customer.Email,
                            FirstName = (bool)customer?.FullName.Contains(" ") ? customer?.FullName?.Substring(0, customer.FullName.IndexOf(" ") + 1) : customer.FullName,
                            Lastname = (bool)customer?.FullName.Contains(" ") ? customer?.FullName?.Substring(customer.FullName.IndexOf(" ")) : "",
                            UserName = customer.FullName.ToLower().Replace(" ", "")

                        };

                        var result = await _userManager.CreateAsync(systemUser,offertoryCartDto.Password);
                        if (result.Succeeded)
                        {
                            //send pass word generation email
                            var requestor = Url.Action(new UrlActionContext
                            {
                                Protocol = Request.Scheme,
                                Host = Request.Host.Value,
                                Action = "Action"
                            });
                            _smtpSenderController.sendPasswordResetEmail(systemUser, requestor);

                        }
                        else if (result.Errors.FirstOrDefault()?.Code == "DuplicateUserName")
                        {
                            // username exists auto generate new              
                            systemUser.UserName = await _authorizationController.GenerateUserName(systemUser.UserName);

                            //try again creating once
                            result = await _userManager.CreateAsync(systemUser, offertoryCartDto.Password);
                            if (result.Succeeded)
                            {
                                //send password generation email
                                var requestor = Url.Action(new UrlActionContext
                                {
                                    Protocol = Request.Scheme,
                                    Host = Request.Host.Value,
                                    Action = "Action"
                                });
                                _smtpSenderController.sendPasswordResetEmail(systemUser, requestor);
                            }

                        }
                    }


                    //create order
                    List<CartDetail> cart = offertoryCartDto.items ?? new List<CartDetail>();
                    decimal GrandTotal = Math.Round(cart.Sum(x => x.quantity * x.price), 2);

                    Orders orders = new Orders()
                    {
                        change = GrandTotal - paidAmount,
                        CreatedDate = DateTime.UtcNow,
                        customerId = customer.CustomerID,
                        saleTotal = GrandTotal,
                        transactionStatus = (int)statics.SaleStatus.opened,
                        transactionComment = "Mobile Money Offertory"
                    };
                    context.orders.Add(orders);
                    context.SaveChanges();

                    //create orderdetail
                    int sortOrder = 1;

                    foreach (var item in offertoryCartDto.items)
                    {
                        OrderDetail orderDetail = new OrderDetail()
                        {
                            orderId = orders.Id,
                            productSlug = item.title.ToLower().Replace(" ", "-"),
                            priceInc = item.price,
                            qty = item.quantity,
                            productID = item.id,
                            totalPriceInc = item.quantity * item.price,
                            sortOrder = sortOrder,
                        };
                        sortOrder++;
                        context.orderDetails.Add(orderDetail);

                        //create user role for claims for the lugogo ambasodors show

                        //if (orderDetail.productSlug == "lugogo" || orderDetail.productSlug == "imperial")
                        //{
                        //    var check = await _roleManager.FindByNameAsync(orderDetail.productSlug);
                        //    var role = await _userManager.AddToRoleAsync(systemUser, orderDetail.productSlug);
                        //}


                    }

                    //create payment
                    Payments payments = new Payments()
                    {
                        Amount = paidAmount,
                        BankID = -1,
                        BankingDate = default(DateTime),
                        CardRef = "",
                        ChequeNo = "",
                        CustomerDepositID = -1,
                        CustomerID = (int)customer.CustomerID,
                        ExpenseID = -1,
                        NameOnCheque = "",
                        PaymentDetailJson = "",
                        PaymentEmail = offertoryCartDto.EmailAddress,
                        PaymentModeID = (int)statics.PaymentModes.MobileMoney,
                        SaleID = orders.Id, //to be editted
                        SupplierID = -1,
                        SupplierPaymentID = -1
                    };


                    context.payments.Add(payments);
                    context.SaveChanges();
                    scope.CommitAsync();

                    //Initiate Transaction request
                    var mtnMomo = new RequestToPayDto()
                    {
                        Amount = paidAmount.ToString(),
                        CallBackUrl = baseLink + "cart/AfterMomoPayment",
                        Currency = "EUR",
                        ExternalId = orders.Id.ToString(),
                        PayeeNote = "SDA Kcc",
                        PayerMessage = "SDAKampala Central Church",
                        Payer = new Payer()
                        {
                            partyId = "256754565256",
                            partyIdType = "MSISDN"
                        }
                    };
                    await _mtnmomoController.RequestToPay(mtnMomo);
                    //clear session

                    var emailDto = new EmailDto(true);
                    emailDto.Subject = "Order created";
                    emailDto.ToEmail = systemUser.Email;
                    emailDto.Body = $"Hello {systemUser.FirstName}<br/><br/>" +
                        $"Your order of {offertoryCartDto.orderCurrency} {orders.saleTotal} has been Created. And is awaiting payment. You can pay for the order by following this link <br/>" +
                        $"https://m.beyonic.com/sl/DoyernConcepts<br/>.If you have any queries regarding this order, you can reply directly to this email." +
                        $"  <br/><br/>Thank you for being part of SDA Kampala central church.";

                    Task.Run(() => _smtpSenderController.SendMail(emailDto));
                    return Ok("Payment Pending");

                }


                return BadRequest("Complete Payment first");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


            return BadRequest();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AfterMomoPayment(long transactionID, decimal? transactionAmount)
        {
            if (transactionID<=0) return BadRequest("Invalid Transaction ID");

            var transaction = context.orders.FirstOrDefault(x=>x.Id==transactionID);

            if (transaction == null) return NotFound("Transaction not found");

            decimal paidAmount = transactionAmount??(decimal)(transaction.saleTotal??0);

            //ToDo: call paypalapi and verify payment
            try
            {

                using (var scope = context.Database.BeginTransaction())
                {

                    //get user
                    var customerEmail = context.customers.FirstOrDefault(x => x.CustomerID == transaction.customerId)?.Email;
                    AppUser systemUser = await _userManager.FindByEmailAsync(customerEmail);

                    if (systemUser == null) return NotFound("User not found");
                    //update order
                    transaction.transactionStatus = (int)statics.SaleStatus.ClosedFromHold;

                   
                    context.orders.Update(transaction);
                    context.SaveChanges();

                    //update orderdetail
                   

                    var orderDetails = context.orderDetails.Where(x => x.orderId == transaction.Id);

                    foreach (var orderDetail in orderDetails)
                    {


                        // create user role for claims for the lugogo ambasodors show

                        if (orderDetail.productSlug == "lugogo" || orderDetail.productSlug == "imperial")
                        {
                            var check = await _roleManager.FindByNameAsync(orderDetail.productSlug);
                            var role = await _userManager.AddToRoleAsync(systemUser, orderDetail.productSlug);
                        }


                    }

                  
                    scope.CommitAsync();
                    //clear session

                    var emailDto = new EmailDto(true);
                    emailDto.Subject = "Payment Received";
                    emailDto.ToEmail = systemUser.Email;
                    emailDto.Body = $"Hello {systemUser.FirstName}<br/><br/>" +
                        $"Your payment of {transaction.saleTotal} for orderID {transaction.Id} made throgh {"Mobile Money"} has been received" +
                        $"  <br/><br/>Thank you for being part of SDA Kampala central church.";

                    Task.Run(() => _smtpSenderController.SendMail(emailDto));
                    return Ok("Payment Completed");

                }


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

        [HttpGet]
        [Authorize(AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetPendingTransactionsForUser(string fullName)
        {
            if (string.IsNullOrEmpty(fullName) || fullName.Length<2) return BadRequest("Full name cannot be null or less than 3 letters");

            var usersMatched = context.Users.Where(x => x.UserName.ToLower().Contains(fullName.ToLower().Replace(" ", ""))).ToList();

            var orders = new List<Orders>();
            foreach(var user in usersMatched)
            {
                var customerId = context.customers.FirstOrDefault(x=>x.Email==user.Email)?.CustomerID;
                var transactions = context.orders.Where(x => x.transactionStatus == (int)statics.SaleStatus.opened)
                    .Where(x => x.customerId == customerId);

                orders.AddRange(transactions);
            }

            return Ok(orders);
            

        }

    }
}
