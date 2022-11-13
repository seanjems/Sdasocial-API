using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Orders
    {
        [Key]
        public long Id { get; set; }
        public Guid? orderCode { get; set; }
        public DateTime? CreatedDate { get; set; }  
        public decimal? saleTotal { get; set; }
        public decimal? change { get; set; }
        public int? shiftId { get; set; }
        public long? customerId { get; set; }
        public int? transactionStatus { get; set; }
        public int? saleAgentId { get; set; }
        public int? quotationID { get; set; }
        public int? orderStatus { get; set; }
        public string? transactionComment { get; set; }

    }
}
