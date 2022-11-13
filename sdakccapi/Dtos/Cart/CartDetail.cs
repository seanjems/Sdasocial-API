namespace sdakccapi.Dtos.Cart
{
    public class CartDetail
    {
       public string desc {get; set;}
       public long id {get; set;}
       public decimal itemTotal {get; set;}
       public decimal price {get; set;}
       public decimal quantity {get; set;}
       public string title { get; set; }
    }
}
