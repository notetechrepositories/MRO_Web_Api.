using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MRO_Api.Hubs;

namespace MRO_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignalHubController : ControllerBase
    {
          
        /*    private IHubContext<SignalHub, ISignalHubClient> messageHub;
            public SignalHubController(IHubContext<SignalHub, ISignalHubClient> _messageHub)
            {
                messageHub = _messageHub;
            }

            [HttpPost]
            [Route("productoffers")]
            public string Get()
            {
                List<string> offers = new List<string>();
                offers.Add("20% Off on IPhone 12");
                offers.Add("15% Off on HP Pavillion");
                offers.Add("25% Off on Samsung Smart TV");
*//*                messageHub.Clients.SendMessage();*//*
                return "Offers sent successfully to all users!";
            }*/
        }


    
}
