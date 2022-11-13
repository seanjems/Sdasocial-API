using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sdakccapi.StaticDetails
{
    public static class statics
    {
        #region PayPalResponseDetails
        public static string buyerEmail { get; set; }

        #endregion
        public enum PaymentModes
        {
            Cash=1,
            Account=2,
            Card=3,
            Bankdeposit=4,
            Cheque=5,
            Paypal=6
        }
       
        public enum SaleStatus
        {
            opened = 4,
            OnHold = 1,
            ActiveIDonHold = 2,
            Quotation = 3,
            SaleCompletedMark = 5,
            Order = 6,
            Refunded = 7,

            ClosedInstantly = 0 + 10,
            ClosedFromHold = 1 + 10,
            ClosedFromQuote = 3 + 10,

        }
    }
}
