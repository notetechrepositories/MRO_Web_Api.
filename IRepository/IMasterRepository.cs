using MRO_Api.Model;
using System.Text.Json.Nodes;
using static MRO_Api.Model.CommonModel;

namespace MRO_Api.IRepository
{
    public interface IMasterRepository
    {
        /* public Task<object> commonGet(CommonModel jsonData);*/
        public Task<ApiResponseModel<dynamic>> commonGet(CreateModel createModel);

        public Task<ApiResponseModel<dynamic>> commonDelete(DeleteModel deleteModel);

        public Task<ApiResponseModel<dynamic>> commonApiForEmail(CreateModel createModel);
        public Task<string> otpVerification(Dictionary<string, string> data);

        public Task<ApiResponseModel<dynamic>> InsertUser(string data, IFormFile formFile);

        public Task<ApiResponseModel<dynamic>> UpdateUser(string data, IFormFile? formFile);


        public  Task<ApiResponseModel<dynamic>> commonGetwithImage(CreateModel createModel);

        public object GetToken(dynamic userObj);
        public  Task<ApiResponseModel<dynamic>> TerminateSession(List<string> connectionList);

        public  Task<ApiResponseModel<dynamic>> LogoutSignalR(CreateModel createModel);
    }
}
