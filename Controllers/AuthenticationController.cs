using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MRO_Api.IRepository;

namespace MRO_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationRepository _authenticationRepository;

        public AuthenticationController(IAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
        }



       
    }
}
