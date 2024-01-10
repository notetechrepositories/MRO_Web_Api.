using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MRO_Api.IRepository;
using MRO_Api.Model;

namespace MRO_Api.Controllers
{
    [Route("api/Mro")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        public LoginController(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult>ValidatePin(Dictionary<string,string> jsonData)
        {
            var result = await _loginRepository.ValidatePin(jsonData);
            return Ok(result);
        }
    }
}
