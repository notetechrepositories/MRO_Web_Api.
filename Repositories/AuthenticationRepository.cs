﻿using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using MRO_Api.Context;
using MRO_Api.Hubs;
using MRO_Api.IRepository;
using MRO_Api.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static MRO_Api.Model.CommonModel;

namespace MRO_Api.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly DapperContext _dapperContext;


        private readonly IConfiguration _iconfiguration;

        public AuthenticationRepository(DapperContext dapperContext, IConfiguration configuration)
        {
            _dapperContext = dapperContext;
            _iconfiguration = configuration;
        }
      


       public async Task<ApiResponseModel<dynamic>> Login(Dictionary<string, string> datajsonData)
       {
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    /*Serialize the request object to JSON*/
                    /* jsonData["t18_connection_id"] = "1234";*/
                    var Serialize_jsonData = JsonConvert.SerializeObject(datajsonData);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                         "get_validation_pin_sp",
                         new { jsonData = Serialize_jsonData },
                         commandType: CommandType.StoredProcedure
                     );

                    return new ApiResponseModel<dynamic>
                    {
                        Data = GetToken(result),
                        Message = "Successfully login ",
                        Status = 200
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<dynamic>
                {
                    Data = null,
                    Message = ex.Message,
                    Status = 400
                };

            }
        
       }



        public async Task<ApiResponseModel<dynamic>> logout(CreateModel createModel)
        {
            try
            {
                dynamic finalResult = new List<Dictionary<string, object>>();
                using (var connection = _dapperContext.CreateConnection())
                {
                    var jsonData = JsonConvert.SerializeObject(createModel);

                    var result = await connection.QueryAsync(
                        "api_crud_sp",
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );                  
                    if (result != null)
                    {                                            
                        return new ApiResponseModel<dynamic>
                        {
                            Data = finalResult,
                            Message = "message"?.ToString(),
                            Status = Convert.ToInt32("status")
                        };
                    }
                    else
                    {
                        return new ApiResponseModel<dynamic>
                        {
                            Data = result,
                            Message = "No data returned",
                            Status = 204 // No Content
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                var errorDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);

                return new ApiResponseModel<dynamic>
                {
                    Data = null,
                    Message = errorDict?["Message"]?.ToString() ?? "An error occurred",
                    Status = errorDict?["Status"],
                };
            }
        }



        private object GetToken(dynamic userObj)
        {
            var claims = new[]
            {
                        new Claim(JwtRegisteredClaimNames.Sub,_iconfiguration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),
                        new Claim("UserId",userObj.id_t5_m_users.ToString()),
                         new Claim("first_name",userObj.t5_first_name),
                        new Claim("last_name",userObj.t5_last_name),
                        new Claim("Email",userObj.t5_email)

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iconfiguration["Jwt:Key"]));

            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _iconfiguration["Jwt:Issuer"],
                _iconfiguration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(40),
                signingCredentials: signIn
                );

            
            var accessTokenDictionary = new Dictionary<string, string>()
            {
                {"access_token",new JwtSecurityTokenHandler().WriteToken(token) }
            };
            return accessTokenDictionary ;
        }






    }
}
