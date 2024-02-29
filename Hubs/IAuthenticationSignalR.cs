using Microsoft.AspNetCore.SignalR;
using MRO_Api.Model;
using static MRO_Api.Model.CommonModel;

namespace MRO_Api.Hubs
{
    public interface IAuthenticationSignalR
    {
 
        Task GetLoggedConnectionDetails(IEnumerable<dynamic> connectionDetails);
        Task AlertNewLogin( dynamic connectionDict);
        Task AlertTerminateSession(List<string>connectionList,string message);
        Task SentLoggedUserDetails(ApiResponseModel<dynamic> data);

        Task AlertLogout(dynamic connectionDict);
    }
}
