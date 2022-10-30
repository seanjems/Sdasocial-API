using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using sdakccapi.Dtos.EmailDto;
using sdakccapi.Services;

namespace sdakccapi.Controllers.SmtpClient
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmtpSenderController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private IConfiguration _configuration;

        public SmtpSenderController(IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _configuration = configuration;
        }
        [HttpPost]
        public IActionResult SubmitForm(string content)
        {
            var allowedDomains = _configuration.GetSection("AllowedOrigins").Get<string[]>();
            
            var requestUrl = HttpContext.Request.HttpContext.Request.Headers.Origin;
            if (allowedDomains.Where(x => x.Contains(requestUrl.ToString().Trim(), StringComparison.OrdinalIgnoreCase)).Count() == 0)
            {
                //not authorized domain
                return Unauthorized("Not authorised");
            }

            //base 64 decode info

            var clientJson = Base64Decode(content);
            if (string.IsNullOrEmpty(clientJson)) return BadRequest("Invalid input data");


            //send email

            var emailDto = JsonConvert.DeserializeObject<EmailDto>(clientJson);

            var emailSenderCredentials = _configuration.GetSection($"SmtpCredentials:{requestUrl}");
            emailDto.emailSenderAccount = emailSenderCredentials?["emailSenderAccount"].ToString();
            emailDto.emailSenderSecret = emailSenderCredentials?["emailSenderSecret"].ToString();
            emailDto.CompanyName = emailSenderCredentials?["CompanyName"].ToString();
            emailDto.ReplyToEmail = emailSenderCredentials?["ReplyToEmail"].ToString();
            emailDto.websiteLink = requestUrl.ToString();
            emailDto.ToEmail = emailSenderCredentials?["ToEmail"].ToString();

            //this should be updated last 
            emailDto.Body = EmailTemplates.EmailTemplates.WebsiteForm(emailDto);

            SendMail(emailDto);
            return Ok();
        }
        [NonAction]
        public void SendMail(EmailDto emailDto)
        {
            try
            {
                emailDto.emailSenderSecret = emailDto.emailSenderSecret ?? "no-reply@kampalacentraladventist.com";
                emailDto.emailSenderAccount = emailDto.emailSenderAccount ?? "123qwe*";
                _emailService.SendEmail(emailDto);
            }
            catch (Exception ex)
            {
               //log error
            }
           

        }
        private static string Base64Decode(string base64EncodedData)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }
    }
}
