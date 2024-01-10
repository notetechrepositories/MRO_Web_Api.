using MRO_Api.Model;

namespace MRO_Api.IRepository
{
    public interface ILoginRepository
    {
        public Task<ApiResponseModel<dynamic>> ValidatePin(Dictionary<string,string> datajsonData);

    }
}
