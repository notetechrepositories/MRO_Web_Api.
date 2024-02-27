using Microsoft.AspNetCore.SignalR;
using MRO_Api.Model;
using static MRO_Api.Model.CommonModel;

namespace MRO_Api.Hubs
{
    public interface IAuthenticationSignalR
    {
 
        Task GetLoggedConnectionDetails(IEnumerable<dynamic> connectionDetails);

        Task AlertNewLogin(string message, string connectionDict);
    }
}
