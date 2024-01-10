using MRO_Api.Model;

namespace MRO_Api.IRepository
{
    public interface IEmailRepository
    {
        public  Task<int> SendMail(EmailDtoModel emailDtoModel);
    }
}
