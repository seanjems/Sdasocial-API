using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class OrderDetail
    {
        [Key]
        public long detailID { get; set; }
        public string? productSlug { get; set; }
        public long? orderId { get; set; }
        public long? productID { get; set; }
        public decimal? qty { get; set; }
        public decimal? costExc { get; set; }
        public decimal? costInc { get; set; }
        public decimal? priceInc { get; set; }
        public decimal? priceExc { get; set; }
        public int? taxID { get; set; }
        public decimal? taxPercent { get; set; }
        public int? discountID { get; set; }
        public decimal? discountPercent { get; set; }        
        public decimal? totalPriceExc { get; set; }
        public decimal? totalPriceInc { get; set; }
        public int? sortOrder { get; set; }
        public bool? specialPricingUsed { get; set; }
    }
}
