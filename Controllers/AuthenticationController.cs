using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MRO_Api.Hubs;
using MRO_Api.IRepository;
using MRO_Api.Repositories;
using static MRO_Api.Model.CommonModel;

namespace MRO_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private IHubContext<AuthenticationSignalR,IAuthenticationSignalR> _signalHub;
        private readonly IMasterRepository _masterRepository;
        public AuthenticationController(IAuthenticationRepository authenticationRepository,IMasterRepository masterRepository ,IHubContext<AuthenticationSignalR,IAuthenticationSignalR> signalHub)
        {
            _authenticationRepository = authenticationRepository;
            _masterRepository = masterRepository;
            _signalHub =  signalHub ;
        }



        [HttpPost("Terminate_Session")]
        public async Task<IActionResult>TerminateSession(List<string> connectionList)
        {
            try
            {
                var result =  await _masterRepository.TerminateSession(connectionList);
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }



        [HttpPost("logout")]
        public async Task<IActionResult> logout(CreateModel createModel)
        {
            try
            {
                var result = await _masterRepository.LogoutSignalR(createModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

    }
}
