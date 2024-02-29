using MRO_Api.Model;

namespace MRO_Api.IRepository
{
    public interface IAuthenticationRepository
    {

        public Task<ApiResponseModel<dynamic>> Login(Dictionary<string, string> datajsonData);

    }
}
