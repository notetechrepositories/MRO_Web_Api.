﻿using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MRO_Api.Context;
using MRO_Api.IRepository;
using MRO_Api.Libraries.Decrypt;
using MRO_Api.Libraries.Encrypt;
using MRO_Api.Model;
using MRO_Api.Utilities;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;


namespace MRO_Api.Repositories
{
    public class MasterRepository : IMasterRepository
    {
        private readonly DapperContext _context;
        private readonly IEmailRepository _emailRepository;
        private readonly CommunicationUtilities _communicationUtilities;
       
        public MasterRepository(DapperContext context,IEmailRepository emailRepository, CommunicationUtilities communicationUtilities)

        {
            _context = context;
            _emailRepository = emailRepository;
            _communicationUtilities = communicationUtilities;
        }




        /*public async Task<ApiResponseModel<dynamic>> commonGet(CommonModel commonModel)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    // Serialize the request object to JSON
                    var jsonData = JsonConvert.SerializeObject(commonModel);

                    // Call the stored procedure
                    var result = await connection.QueryAsync(
                        "api_crud_sp", // Stored procedure name
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );

                    var resultDataJson = result.FirstOrDefault()?.data;
                    var resultDataObject = JsonConvert.DeserializeObject<ResultDataObject>(resultDataJson);

                    // Access the message directly from the result variable
                    var message = result.FirstOrDefault()?.message;

                    var status = result.FirstOrDefault()?.status;

                    var dataDeserialize = resultDataObject?.ResultData;



                    return new ApiResponseModel<dynamic>
                    {
                        Data = dataDeserialize,
                        Message = message,
                        Status = status
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<dynamic>
                {
                    Data = 0,
                    Message = ex.Message,
                    Status = 400
                };
            }
        }*/





     /*   public class ResultDataObject
        {
            [JsonProperty("ResultData")]
            public List<Dictionary<string, object>> ResultData { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }*/






        public async Task<ApiResponseModel<dynamic>> commonGet(CommonModel commonModel)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    // Serialize the request object to JSON
                    var jsonData = JsonConvert.SerializeObject(commonModel);

                    // Call the stored procedure
                    var result = await connection.QueryAsync(
                        "api_crud_sp", // Stored procedure name
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );

                    var resultDataJson = result.FirstOrDefault()?.data;
                    var resultDataList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(resultDataJson);

                    // Access the message directly from the result variable
                    var message = result.FirstOrDefault()?.message;

                    var status = result.FirstOrDefault()?.status;

                    return new ApiResponseModel<dynamic>
                    {
                        Data = resultDataList,
                        Message = message,
                        Status = status
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<dynamic>
                {
                    Data = 0,
                    Message = ex.Message,
                    Status = 400
                };
            }
        }







        /*   var getMenuJson = result.First().data;


           var getMenuList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(getMenuJson);

           // Iterate through each dictionary in the 'getMenu' list
           foreach (var menuDict in getMenuList)
           {
               if (menuDict.ContainsKey("t15_file_path"))
               {

                   var t15FilePathValue = menuDict["t15_file_path"];
               }
           }*/




        public async Task<ApiResponseModel<dynamic>> commonApiForEmail(CommonModel commonModel)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    // Serialize the request object to JSON
                    var jsonData = JsonConvert.SerializeObject(commonModel);

                    // Call the stored procedure
                    var result = await connection.QueryAsync(
                        "api_crud_sp", // Stored procedure name
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );

                    var emailDTOModel = new EmailDtoModel();
                   
                    var row = (IDictionary<string, object>)result.First();

                    emailDTOModel.t16_email_subject = row["t16_email_subject"]?.ToString();
                    emailDTOModel.t16_email_cc = row["t16_email_cc"]?.ToString();
                    emailDTOModel.t16_email_bcc = row["t16_email_bcc"]?.ToString();
                    emailDTOModel.t16_email_html_body = row["t16_email_html_body"]?.ToString();
                    emailDTOModel.from_email_password = row["from_email_password"]?.ToString();
                    emailDTOModel.from_email = row["from_email"]?.ToString();
                    emailDTOModel.to_email = row["to_email"]?.ToString();
                    emailDTOModel.signature_content = row["signature_content"]?.ToString();

                    var emailStatus = await _communicationUtilities.SendMail(emailDTOModel);
                    
                    if (emailStatus)
                    {
                        var currentTime = DateTime.Now.ToString();
                        var otp = row["otp"]?.ToString();

                        var otpTimeDictionary = new Dictionary<string, string>
                        {
                            ["otp"] = otp,
                            ["time"] = currentTime
                        };

                        // Encrypt the otpTimeDictionary
                        string encryptedData = Encryption.encrypt(JsonConvert.SerializeObject(otpTimeDictionary));

                        

                        return new ApiResponseModel<dynamic>
                        {
                            Data = new Dictionary<string, string>() { {"encryptedData", encryptedData } ,{ "time" , currentTime } },
                            Message = "Successfully retrieved data",
                            Status = 200
                        };
                    }
                    return new ApiResponseModel<dynamic>
                    {
                        Data = null,
                        Message = "Email not sent",
                        Status = 400
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponseModel<dynamic>
                {
                    Data = 0,
                    Message = ex.Message,
                    Status = 400
                };
            }
        }




       
        public async Task<string> otpVerification(Dictionary<string,string> data)
        {
            try
            {
                string status = "";

                // Decrypt the encrypted data
                var otpAndTimeDecrypted = Decryption.Decrypt(data["encryptedData"]);

                // Log or print the decrypted data for debugging
                 status="Decrypted Data: " + otpAndTimeDecrypted;


                // Deserialize the decrypted data to a Dictionary
                var otpAndTimeDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(otpAndTimeDecrypted);

                // Extract OTP and Time from the dictionary
                var storedOTP = otpAndTimeDictionary["otp"];
                DateTime storedTime = DateTime.Parse(otpAndTimeDictionary["time"]);
                DateTime currentTime = DateTime.Now;


                // Calculate the time difference
                TimeSpan timeDifference = currentTime - storedTime;


                // Check if the OTP matches and is within the 60-second window
                if (timeDifference.TotalSeconds < 60)
                {
                    if (storedOTP == data["otp"].Trim())
                    {
                        status = "Valid OTP";
                    }
                    else
                    {
                        status = "OTP not match";
                    }
                }
                else
                {
                    status = "OTP Expired";
                }
                return status;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }







    }
}
