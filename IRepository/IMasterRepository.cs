using MRO_Api.Model;
using System.Text.Json.Nodes;

namespace MRO_Api.IRepository
{
    public interface IMasterRepository
    {
        /* public Task<object> commonGet(CommonModel jsonData);*/
        public Task<ApiResponseModel<dynamic>> commonGet(CommonModel commonModel);

        public Task<ApiResponseModel<dynamic>> commonApiForEmail(CommonModel commonModel);
        public Task<string> otpVerification(Dictionary<string, string> data);
    }
}
