using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MRO_Api.IRepository;
using MRO_Api.Model;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace MRO_Api.Controllers
{
    [Route("api/common")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IMasterRepository _masterRepository;
        public MasterController(IMasterRepository masterRepository)
        {
            _masterRepository = masterRepository;
        }


      
        [HttpPost("get")]
        public  async Task<IActionResult>commonGet([FromBody]CommonModel data)
        {
           
            var result = await _masterRepository.commonGet(data);
            return Ok(result);
        }


        [HttpPost("email")]
        public async Task<IActionResult> commonApiForEmail(CommonModel commonModel)
        {
            var result = await _masterRepository.commonApiForEmail(commonModel);
            return Ok(result);
        }


        [HttpPost("otpVerification")]
        public async Task<IActionResult>  otpVerification(Dictionary<string, string> data)
        {
            var result = await _masterRepository.otpVerification(data);
            return Ok(result);
        }

    }
}
