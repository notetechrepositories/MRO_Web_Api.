using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MRO_Api.Context;
using MRO_Api.IRepository;
using MRO_Api.Model;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text.Json.Nodes;
using static MRO_Api.Model.CommonModel;

namespace MRO_Api.Controllers
{
    [Route("api/common")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IMasterRepository _masterRepository;

        private readonly DapperContext dapperContext;

        public MasterController(IMasterRepository masterRepository, DapperContext dapper)
        {
            _masterRepository = masterRepository;
            dapperContext = dapper;
        }



        [HttpPost("get")]
        public async Task<IActionResult> commonGet(CreateModel createModel)
        {

            var result = await _masterRepository.commonGet(createModel);
            return Ok(result);
        }




        [HttpDelete("delete")]
        public async Task<IActionResult> commonDelete([FromBody] DeleteModel deleteModel)
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
        public async Task<IActionResult> otpVerification(Dictionary<string, string> data)
        {
            var result = await _masterRepository.otpVerification(data);
            return Ok(result);
        }



        [HttpPost("Insert-user")]
        public async Task<IActionResult> InsertUser(string data, [FromForm] IFormFile? formFile)
        {
            var result = await _masterRepository.InsertUser(data, formFile);
            return Ok(result);

        }




        [HttpPut("Update-user")]
        public async Task<IActionResult> UpdateUser(string data,[FromForm] IFormFile? formFile )
        {
            var result = await _masterRepository.UpdateUser(data, formFile);
            return Ok(result);

        }



    }
}
