namespace sdakccapi.Dtos.EmailDto
{
    public class EmailDto
    {
        public string? FromEmail { get; set; }
        public string? ReplyToEmail { get; set; }
        public string? ToEmail { get; set; }
        public string? Subject { get;set; }
        public string? Body { get; set; }
        public string? SenderName { get; set; }
        public string? SenderPhonenumber { get; set; }
        public string? Category { get; set; }
        public string? websiteLink { get; set; }
        public string? CompanyName { get; set; }
        public string? emailSenderAccount { get; set; }
        public string? emailSenderSecret { get; set; }
    }
}
