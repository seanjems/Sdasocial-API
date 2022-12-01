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

        public CartController(sdakccapiDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AuthorizationController authorizationController)
        {
            this.context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _authorizationController = authorizationController;
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
                                FullName = (payPalObj?["payer"]?["name"]?.ToString())
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
                                    //send pass word generation email
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
                                    productSlug = item.title.ToLower().Replace(" ","-"),
                                    priceInc = item.price,
                                    qty = item.quantity,
                                    productID = item.id,
                                    totalPriceInc = item.quantity * item.price,
                                    sortOrder = sortOrder,
                                };
                                sortOrder++;
                                context.orderDetails.Add(orderDetail);
                                
                                //create user role for claims for the lugogo ambasodors show

                                if(orderDetail.productSlug=="lugogo" || orderDetail.productSlug == "imperial")
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

    }
}
