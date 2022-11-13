using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Product code")]
        public string productCode { get; set; }

        [DisplayName("Barcode")]
        [MinLength(2, ErrorMessage = "Minimum length is 2")]
        public string barCode { get; set; }

        [DisplayName("slug")]
        public string Slug { get; set; }
        
        public string Description { get; set; }

        [Required]
        [DisplayName("ProductName")]
        public string productName { get; set; }

        [DisplayName("CostExc")]
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal costExclusive { get; set; }
        
        [DisplayName("CostInc")]
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal costInclusive { get; set; }

        [DisplayName("CostIncStatus")]
        public bool costIncStatus { get; set; }

        [DisplayName("InStock")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? inStock { get; set; }

        [DisplayName("PriceExc"), Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal priceExclusive { get; set; }

        [DisplayName("PriceInc"), Column(TypeName ="decimal(18,2)")]
        public decimal priceInclusive { get; set; }

        [DisplayName("Category")]       
        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int categoryId { get; set; }

        
        [ForeignKey("categoryId")]
        public virtual Category Category { get; set; }

        [DisplayName("Location")]
        public string location { get; set; }
        [DisplayName("SegmentID")]
        public int segmentId { get; set; }
        [DisplayName("SupplierID")]
        public int supplierId { get; set; }
        [DisplayName("ProductImage")]
        public string productImage { get; set; }
        
        public DateTime createdDate { get; set; }
        [DisplayName("CreatedBy")]
        public int createdBy { get; set; }
        [DisplayName("isDeleted")]
        public bool deleted { get; set; }
        public bool trackInventory { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal reOrderLevel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal reOrderQty { get; set; }
        public bool favourite { get; set; }
        public bool hasSubProduct { get; set; }
        public bool isAsubProduct { get; set; }
        public int compoundCostPricing { get; set; }
        [DisplayName("TaxID")]
        public int tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal priceInclusive2 { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceExclusive2 { get; set; }
    }
}
