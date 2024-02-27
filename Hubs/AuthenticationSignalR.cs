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




       


        public async Task AlertNewLogin(string message, string connectionDict)
        {
/*            await Clients.Client(connectionDict).AlertNewLogin(message, connectionDict);
*/            await Clients.All.AlertNewLogin(message, connectionDict);
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

                    if (result != null && result.connectionPhoneId != null)
                    {
                       /* await Clients.Client(result.connectionPhoneId).AlertNewLogin(result.connectionPhoneId, result.connectionPhoneId);*/
                        await Clients.All.AlertNewLogin(result.connectionPhoneId, result.connectionPhoneId);
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






        public string GenerateConnectionIdForQR() => Context.ConnectionId;




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

                if (result.connectionPhoneId != null)
                {
                    await Clients.Client(result.connectionPhoneId).AlterNewLogin("New User Added", result.connectionPhoneId);
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
