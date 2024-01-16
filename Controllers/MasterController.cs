using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MRO_Api.IRepository;
using MRO_Api.Model;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using static MRO_Api.Model.CommonModel;

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
        public  async Task<IActionResult>commonGet([FromBody] CreateModel createModel)
        {
           
            var result = await _masterRepository.commonGet(createModel);
            return Ok(result);
        }


        [HttpDelete("delete")]
        public  async Task<IActionResult>commonDelete([FromBody]DeleteModel deleteModel)
        {        
            var result = await _masterRepository.commonDelete(deleteModel);
            return Ok(result);
        }



        [HttpPost("email")]
        public async Task<IActionResult> commonApiForEmail(CreateModel createModel)
        {
            var result = await _masterRepository.commonApiForEmail(createModel);
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
