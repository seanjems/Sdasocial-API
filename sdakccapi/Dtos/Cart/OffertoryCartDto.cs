namespace sdakccapi.Dtos.Cart
{
    public class OffertoryCartDto
    {

        public List<CartDetail> items { get; set; }
        public string orderCurrency { get; set; }
        public string paypalString { get; set; }

    }
}
