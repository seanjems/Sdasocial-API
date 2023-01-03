namespace sdakccapi.Dtos.MtnMomoDto
{
    public class RequestToPayDto
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ExternalId { get; set; }
        public  Payer Payer { get; set; }
        public string PayeeNote { get; set; }
        public string PayerMessage { get; set; }
        public string CallBackUrl { get; set; }
        public string Status { get; set; }
    }
}
