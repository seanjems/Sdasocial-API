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

namespace BillTrickOnline.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    [Authorize]
    public class CartController : Controller
    {
        private readonly sdakccapiDbContext context;
        public CartController(sdakccapiDbContext context)
        {
            this.context = context;
        }

        //POST paypal details after payment
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AfterPayPalPayment(OffertoryCartDto offertoryCartDto)
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
                                //no need to save changes immediately
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
