using Dapper;
using Microsoft.IdentityModel.Tokens;
using MRO_Api.Context;
using MRO_Api.IRepository;
using MRO_Api.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace MRO_Api.Repositories
{
    public class LoginRepository: ILoginRepository
    {
        private readonly DapperContext _context;

        private readonly IConfiguration _iconfiguration;

        public LoginRepository(DapperContext context,IConfiguration configuration)
        {
            _context = context;
            _iconfiguration = configuration;
        }



        public async Task<ApiResponseModel<dynamic>> ValidatePin(Dictionary<string, string> jsonData)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    jsonData["connectionId"] = "afdsfa";
                    var Serialize_jsonData = JsonConvert.SerializeObject(jsonData);

                 

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                         "get_validation_pin_sp",
                         new { jsonData = Serialize_jsonData },
                         commandType: CommandType.StoredProcedure
                     );


                    // Deserialize the deviceList prvoperty from a string to a list of dictionaries
                    if (result != null && result.roleList is string roleListString)
                    {
                        result.roleList = JsonConvert.DeserializeObject<List<string>>(result.roleList);
                    }
                    var accessTokenDictionary = new Dictionary<string, dynamic>()
                    {
                        {"access_token",GetToken(result) },
                        {"roleList",result.roleList }
                    };
                    
                    return new ApiResponseModel<dynamic>
                    {
                        Data = accessTokenDictionary,
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

            
             /* var accessTokenDictionary = new Dictionary<string, string>()
                {
                    {"access_token",new JwtSecurityTokenHandler().WriteToken(token) }
                };*/


            return new JwtSecurityTokenHandler().WriteToken(token);

        }









    }
}
