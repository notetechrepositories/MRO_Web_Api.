using Dapper;
using Microsoft.AspNetCore.SignalR;
using MRO_Api.Context;
using MRO_Api.IRepository;
using MRO_Api.Model;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System.Data;
using static MRO_Api.Model.CommonModel;

namespace MRO_Api.Hubs
{
    public class AuthenticationSignalR : Hub<IAuthenticationSignalR>
    {
        private readonly DapperContext _dapperContext;
        private readonly IMasterRepository _masterRepository;

        public AuthenticationSignalR(DapperContext dapperContext,IMasterRepository masterRepository)
        {
            _dapperContext = dapperContext;
           _masterRepository = masterRepository;
        }




       


        public async Task AlertNewLogin( dynamic connectionDict)
        {
            await Clients.Client(connectionDict).AlertNewLogin( connectionDict);
/*            await Clients.All.AlertNewLogin(message, connectionDict);
*/      }    
        public async Task AlertLogout( dynamic connectionDict)
        {
            await Clients.Client(connectionDict).AlertLogout( connectionDict);
/*            await Clients.All.AlertNewLogin(message, connectionDict);
*/      }
        

        public async Task AlertTerminateSession(List<string> connectionDict,string message)
        {
            await Clients.Clients(connectionDict).AlertTerminateSession(connectionDict,message);
            /*            await Clients.All.AlertNewLogin(message, connectionDict);
            */
        }






        public async Task<ApiResponseModel<dynamic>> LoginSignalR(Dictionary<string, string> jsonData)
        {
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    jsonData["connectionId"] = Context.ConnectionId;

                    var serializedJsonData = JsonConvert.SerializeObject(jsonData);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "get_validation_pin_sp",
                        new { jsonData = serializedJsonData },
                        commandType: CommandType.StoredProcedure
                    );
                  

                    // Deserialize the deviceList property from a string to a list of dictionaries
                    if (result != null && result.deviceList is string deviceListString && result.connectionPhoneId != null)
                    {
                        result.deviceList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(result.deviceList);
                        await Clients.Client(result.connectionPhoneId).AlertNewLogin(result.deviceList);
                    }
                   
                        
                       
                    return new ApiResponseModel<dynamic>
                    {
                        Data = result != null ? _masterRepository.GetToken(result) : null,
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
                    Message = $"Error during login: {ex.Message}",
                    Status = 400
                };
            }
            
        }






        public async Task<ApiResponseModel<dynamic>> LogoutSignalR(CreateModel createModel)
        {
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var jsonData = JsonConvert.SerializeObject(createModel);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "api_crud_sp",
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );

                    // Deserialize the deviceList property from a string to a list of dictionaries
                    if (result != null)
                    {

                        if ( result.connectionPhoneId != null)
                        {
                            result.deviceList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(result.deviceList);
                        
                            await Clients.Client(result.connectionPhoneId).AlertLogout(result.deviceList);
                        }

                        return new ApiResponseModel<dynamic>
                        {
                            Data = result.data,
                            Message = result.message,
                            Status = Convert.ToInt32(result.status)
                        };
                    }
                    else
                    {
                        return new ApiResponseModel<dynamic>
                        {
                            Data = null,
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
                    Status = Convert.ToInt32(errorDict?["Status"] ?? 500),  // Provide a default status code if not available
                };
            }
        }





        public string GenerateConnectionIdForQR() => Context.ConnectionId;




        public async Task SentLoggedUserDetails(ApiResponseModel<dynamic> data, string connectionId)
        {
            await Clients.Client(connectionId).SentLoggedUserDetails(data);
        }



        public async Task<ApiResponseModel<dynamic>> QRLoginSignalR(CreateModel createModel)
        {
            using (var connection = _dapperContext.CreateConnection())
            {
                var jsonData = JsonConvert.SerializeObject(createModel);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "api_crud_sp",
                    new { jsonData },
                    commandType: CommandType.StoredProcedure
                );

                if (result != null)
                {
                    await Clients.Client(createModel.data["connectionId"]).SentLoggedUserDetails(
                        new ApiResponseModel<dynamic>()
                        {
                            Data= _masterRepository.GetToken(result),
                            Message = "User logged  succesfully",
                            Status = 200
                        }
                        );
                    /* await Clients.Client(result.connectionPhoneId).AlertNewLogin(result.connectionPhoneId, result.connectionPhoneId);*/
                    await Clients.All.AlertNewLogin(result.deviceList);
                }

                return new ApiResponseModel<dynamic>
                {
                    Data = result,
                    Message = "Successfully login ",
                    Status = 200
                };
            }
        }






        public async Task GetLoggedConnectionDetails(CreateModel createModel)
        {
            using (var connection = _dapperContext.CreateConnection())
            {
                var jsonData = JsonConvert.SerializeObject(createModel);

                var result = await connection.QueryAsync(
                    "api_crud_sp",
                    new { jsonData },
                    commandType: CommandType.StoredProcedure
                );
                var firstResult = result.FirstOrDefault() as IDictionary<string, object>;
                if (firstResult != null)
                {
                    await Clients.Client(firstResult["message"]?.ToString()).GetLoggedConnectionDetails(result);
                }                   
            }
        }






        public async Task GetLoggedConnectionDetailsSignalR(CreateModel createModel)
        {
            using (var connection = _dapperContext.CreateConnection())
            {
                var jsonData = JsonConvert.SerializeObject(createModel);

                var result = await connection.QueryAsync(
                    "api_crud_sp",
                    new { jsonData },
                    commandType: CommandType.StoredProcedure
                );
                var firstResult = result.FirstOrDefault() as IDictionary<string, object>;
                if (firstResult != null)
                {
                    await Clients.Client(firstResult["message"]?.ToString()).GetLoggedConnectionDetails(result);
                }
            }
        }





    }
}
