using Microsoft.AspNetCore.Mvc;
using sdakccapi.Dtos.EmailDto;

namespace sdakccapi.Services
{
    public interface IEmailService
    {
        void SendEmail(EmailDto emailDto);
    }
}
