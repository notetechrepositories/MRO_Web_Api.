﻿using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using MRO_Api.Context;
using MRO_Api.Hubs;
using MRO_Api.IRepository;
using MRO_Api.Libraries.Decrypt;
using MRO_Api.Libraries.Encrypt;
using MRO_Api.Model;
using MRO_Api.Utilities;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using static MRO_Api.Model.CommonModel;
using static System.Net.WebRequestMethods;


namespace MRO_Api.Repositories
{
    public class MasterRepository : IMasterRepository
    {
        private readonly DapperContext _context;
     
        private readonly CommunicationUtilities _communicationUtilities;
        private readonly IConfiguration _iconfiguration;
        private IHubContext<AuthenticationSignalR, IAuthenticationSignalR> _signalHub;


        public MasterRepository(DapperContext context, CommunicationUtilities communicationUtilities, IConfiguration iconfiguration, IHubContext<AuthenticationSignalR, IAuthenticationSignalR> signalHub)

        {
            _context = context;
            _communicationUtilities = communicationUtilities;
            _iconfiguration = iconfiguration;
            _signalHub = signalHub;
        }






        /*    public async Task<ApiResponseModel<dynamic>> commonGet(CreateModel createModel)
            {
                try
                {
                    dynamic finalResult = new List<Dictionary<string, object>>();
                    using (var connection = _context.CreateConnection())
                    {
                        var jsonData = JsonConvert.SerializeObject(createModel);


                        var result = await connection.QueryAsync(
                            "api_crud_sp",
                            new { jsonData },
                            commandType: CommandType.StoredProcedure
                        );

                        var firstResult = result.FirstOrDefault();
                        if (firstResult != null)
                        {
                            var testData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(firstResult);



                            foreach (var item in testData)
                            {
                                var accumulatedPairs = new Dictionary<string, object>();
                                foreach (var keyValuePair in item)
                                {
                                    if (keyValuePair.Value is JArray)
                                    {
                                        var jsonArray = keyValuePair.Value.ToObject<List<Dictionary<string, object>>>();
                                        accumulatedPairs[keyValuePair.Key] = jsonArray;
                                    }
                                    else if (keyValuePair.Value is string)
                                    {
                                        accumulatedPairs[keyValuePair.Key] = keyValuePair.Value.ToString();
                                    }
                                    else
                                    {

                                        accumulatedPairs[keyValuePair.Key] = keyValuePair.Value;
                                    }
                                }
                                finalResult.Add(accumulatedPairs);
                            }

                            var message = firstResult.message;
                            var status = firstResult.status;

                            return new ApiResponseModel<dynamic>
                            {
                                Data = finalResult,
                                Message = message,
                                Status = Convert.ToInt32(status)
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
                    var t = ex.Message;
                    var errorDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);

                    return new ApiResponseModel<dynamic>
                    {
                        Data = null,
                        Message = errorDict["Message"],
                        Status = Convert.ToInt32(errorDict["Status"])
                    };
                }
            }*/



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






        public async Task<ApiResponseModel<dynamic>> TerminateSession(List<string> jsonData)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                       "mro_dev_login_and_rights.web_terminate_by_app_sp",
                       new { jsonData = JsonConvert.SerializeObject(jsonData) },
                       commandType: CommandType.StoredProcedure
                   );
                    // Deserialize the deviceList property from a string to a list of dictionaries
                    if (result != null && result.connectionList is string deviceListString )
                    {
                        result.connectionList = JsonConvert.DeserializeObject<List<string>>(result.connectionList);
                        _signalHub.Clients.Clients(result.connectionList).AlertTerminateSession(result.connectionList,"User Terminate your session");
                    }
                    return new ApiResponseModel<dynamic>()
                    {
                        Data = result.connectionList,
                        Message = "Successfully Terminated",
                        Status = 200
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }






        public object GetToken(dynamic userObj)
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
            return accessTokenDictionary;
        }



        public async Task<ApiResponseModel<dynamic>> LogoutSignalR(CreateModel createModel)
        {
            try
            {

                using (var connection = _context.CreateConnection())
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
                        if (result.deviceList is string deviceListString && result.connectionPhoneId != null)
                        {
                            result.deviceList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(result.deviceList);
                           
                        }
                        return new ApiResponseModel<dynamic>
                        {
                            Data = result,
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
                    Status = errorDict?["Status"],
                };
            }
        }







        public async Task<ApiResponseModel<dynamic>> commonGet(CreateModel createModel)
        {
            try
            {
                dynamic finalResult = new List<Dictionary<string, object>>();
                using (var connection = _context.CreateConnection())
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
                        foreach (var kvp in firstResult)
                        {
                            var accumulatedPairs = new Dictionary<string, object>();
                            var propertyName = kvp.Key;
                            var propertyValue = kvp.Value;

                            if (propertyName != "message" && propertyName != "status")
                            {
                                if (propertyValue is string stringValue)
                                {
                                    try
                                    {
                                        // Try to parse the string as JSON array
                                        var jsonArray = JArray.Parse(stringValue);
                                        accumulatedPairs[propertyName] = jsonArray.ToObject<List<Dictionary<string, object>>>();
                                    }
                                    catch (JsonException)
                                    {
                                        // Handle as a plain string if parsing as JSON array fails
                                        accumulatedPairs[propertyName] = stringValue;
                                    }
                                }
                                else
                                {
                                    accumulatedPairs[propertyName] = propertyValue;
                                }

                                finalResult.Add(accumulatedPairs);
                            }

                        }
                        return new ApiResponseModel<dynamic>
                        {
                            Data = finalResult,
                            Message = firstResult["message"]?.ToString(),
                            Status = Convert.ToInt32(firstResult["status"])
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
                    Status = 500 // Internal Server Error
                };
            }
        }







        public async Task<ApiResponseModel<dynamic>> UpdateUser(string data, IFormFile? formFile)
        {
            try
            {
                var jsonData = "";
                dynamic finalResult = new List<Dictionary<string, object>>();

                using (var connection = _context.CreateConnection())
                {
                    if (formFile != null)
                    {
                        jsonData = await Insertimage(data, formFile);

                    }

                    var result = await connection.QueryAsync(
                        "api_crud_sp",
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );
                      

                    var firstResult = result.FirstOrDefault() as IDictionary<string, object>;
                    if (firstResult != null)
                    {

                        return new ApiResponseModel<dynamic>
                        {
                            Data = finalResult,
                            Message = firstResult["message"]?.ToString(),
                            Status = Convert.ToInt32(firstResult["status"])
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
                    Status = 500 // Internal Server Error
                };
            }
        }






        public async Task<ApiResponseModel<dynamic>> commonDelete(DeleteModel deleteModel)
        {
            try
            {
                dynamic finalResult = new List<Dictionary<string, object>>();
                using (var connection = _context.CreateConnection())
                {
                    var jsonData = JsonConvert.SerializeObject(deleteModel);

                    var result = await connection.QueryAsync(
                        "api_crud_sp",
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );
                    var firstResult = result.FirstOrDefault();
                    if (firstResult != null)
                    {
                        var testData = firstResult.data;
                        var message = firstResult.message;
                        var status = firstResult.status;

                       

                        return new ApiResponseModel<dynamic>
                        {
                            Data = finalResult,
                            Message = message,
                            Status = Convert.ToInt32(status)
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
                    Message = errorDict["Message"],
                    Status = Convert.ToInt32(errorDict["Status"])
                };
            }
        }


        public async Task<ApiResponseModel<dynamic>>Terminate(DeleteModel deleteModel)
        {
            try
            {
                dynamic finalResult = new List<Dictionary<string, object>>();
                using (var connection = _context.CreateConnection())
                {
                    var jsonData = JsonConvert.SerializeObject(deleteModel);

                    var result = await connection.QueryAsync(
                        "api_crud_sp",
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );
                    var firstResult = result.FirstOrDefault();
                    if (firstResult != null)
                    {
                        var testData = firstResult.data;
                        var message = firstResult.message;
                        var status = firstResult.status;



                        return new ApiResponseModel<dynamic>
                        {
                            Data = finalResult,
                            Message = message,
                            Status = Convert.ToInt32(status)
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
                    Message = errorDict["Message"],
                    Status = Convert.ToInt32(errorDict["Status"])
                };
            }
        }





        public async Task<ApiResponseModel<dynamic>> InsertUser(string data, IFormFile formFile)
        {
            try
            {
                var result1 = "";
                var jsonData = "";
                var pathAppendedData = "";
                dynamic finalResult = new List<Dictionary<string, object>>();
                dynamic firstResult;
                using (var connection = _context.CreateConnection())
                {
                    data = data.Trim('"');
                    if (data != null)
                    {
                        pathAppendedData = await CreateFolder(data);
                    }
                    if (formFile != null)
                    {
                        jsonData = await Insertimage(pathAppendedData, formFile);

                    }
                    var result = await connection.QueryAsync(
                        "api_crud_sp",
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );
                    firstResult = result.FirstOrDefault() as IDictionary<string, object>;
                    if (firstResult != null)
                    {
                        var serializeData = JsonConvert.SerializeObject(firstResult);
                        var deserializerData = JsonConvert.DeserializeObject<IDictionary<string, Object>>(serializeData);

                        var email = await _communicationUtilities.SendMail(deserializerData);

                        return new ApiResponseModel<dynamic>
                        {
                            Data = deserializerData,
                            Message = deserializerData["message"]?.ToString(),
                            Status = Convert.ToInt32(deserializerData["status"])
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
                    Status = 500 // Internal Server Error
                };
            }
        }







        public async Task<string> CreateFolder(string data1)
        {
            dynamic userData = JsonConvert.DeserializeObject(data1);
            string email = userData?.data.t5_email;


            string folderPath = Path.Combine(Directory.GetCurrentDirectory() + "\\Media\\Reports\\", email);

            if (!Directory.Exists(folderPath))
            {
                /*Directory.CreateDirectory(folderPath);*/
                string keyword = Path.Combine(Directory.GetCurrentDirectory());
                int index = folderPath.IndexOf(keyword);
                string result = folderPath.Substring(index + keyword.Length);
                string modifiedResult = result.Replace("\\", "/");

                JObject jsonData = JObject.Parse(data1);


                foreach (var item in jsonData["data"] as JObject)
                {
                    string key = item.Key;
                    if (key == "t5_users_report_folder")
                    {
                        jsonData["data"]["t5_users_report_folder"] = modifiedResult;

                    }
                }

                var jsonData1 = JsonConvert.SerializeObject(jsonData);

                return jsonData1;

            }
            else
            {
                return ("Folder already exists!");
            }
        }







        public async Task<string> Insertimage(string data, IFormFile file)
        {
            string filePath = "";
            string fileName = "";
            dynamic userData = JsonConvert.DeserializeObject(data);
            string email = userData?.data.t5_email;
            dynamic atIndex = email.LastIndexOf('.');
            string modifiedEmail = email.Substring(0, atIndex) ;
            string fileExtension = Path.GetExtension(file.FileName);

            string newImageName = Path.GetFileNameWithoutExtension(file.FileName);
 
            newImageName = modifiedEmail + DateTime.Now.Ticks + fileExtension; // Replace the   name with the employee name

            string folderPath = Path.Combine(Directory.GetCurrentDirectory() + "\\Media\\UserProfiles\\");// Save the file to a specified folder

            // Get the file name
            filePath = Path.Combine(folderPath, newImageName);
            string keyword = Path.Combine(Directory.GetCurrentDirectory());
            int index = filePath.IndexOf(keyword);
            string result = filePath.Substring(index + keyword.Length);
            string modifiedResult = result.Replace("\\", "/");
            JObject jsonData = JObject.Parse(data);

            foreach (var item in jsonData["data"] as JObject)
            {
                string key = item.Key;
                if (key == "t15_file_path")
                {
                    jsonData["data"]["t15_file_path"] = modifiedResult;

                }
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            dynamic userData1 = JsonConvert.SerializeObject(jsonData);
            return userData1;
        }












        public async Task<ApiResponseModel<dynamic>> commonApiForEmail(CreateModel createModel)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    string modifiedEmail = "";
                    // Serialize the request object to JSON
                    var jsonData = JsonConvert.SerializeObject(createModel);

                    // Call the stored procedure
                    var result = await connection.QueryAsync(
                        "api_crud_sp", // Stored procedure name
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );

                    var emailDTOModel = new EmailDtoModel();

                    var row = (IDictionary<string, object>)result.First();

                    /*  emailDTOModel.t16_email_subject = row["t16_email_subject"]?.ToString();
                      emailDTOModel.t16_email_cc = row["t16_email_cc"]?.ToString();
                      emailDTOModel.t16_email_bcc = row["t16_email_bcc"]?.ToString();
                      emailDTOModel.t16_email_html_body = row["t16_email_html_body"]?.ToString();
                      emailDTOModel.from_email_password = row["from_email_password"]?.ToString();
                      emailDTOModel.from_email = row["from_email"]?.ToString();
                      emailDTOModel.to_email = row["to_email"]?.ToString();
                      emailDTOModel.signature_content = row["signature_content"]?.ToString();
  */
                    var emailStatus = await _communicationUtilities.SendMail(row);
                    var toMail = row["to_email"].ToString();

                    int atIndex = toMail.IndexOf('@');

                    if (atIndex >= 0)
                    {
                        // Split the email address into two parts: prefix and domain
                        string prefix = toMail.Substring(0, atIndex);
                        string domain = toMail.Substring(atIndex);

                        // Replace the prefix with asterisks
                        prefix = new string('*', prefix.Length - 2);
                        string beforeAtSymbol = toMail.Substring(0, toMail.IndexOf('@'));

                        // Combine the modified prefix and the domain
                        modifiedEmail = prefix + beforeAtSymbol.Substring(beforeAtSymbol.Length - 2) + domain;

                        // Now 'modifiedEmail' contains "****mailto:99@gmail.com"

                    }
                    else
                    {
                        // Handle invalid email address
                        Console.WriteLine("Invalid email address");
                    }

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
                            Data = new Dictionary<string, string>() { { "encryptedData", encryptedData }, { "time", currentTime }, { "email", modifiedEmail } },
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

                var errorDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);

                return new ApiResponseModel<dynamic>
                {
                    Data = null,
                    Message = errorDict["Message"],
                    Status = Convert.ToInt32(errorDict["Status"])
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


        public async Task<ApiResponseModel<dynamic>> commonGetwithImage(CreateModel createModel)
        {
            try
            {
                dynamic finalResult = new List<Dictionary<string, object>>();
                using (var connection = _context.CreateConnection())
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
                        foreach (var kvp in firstResult)
                        {
                            var accumulatedPairs = new Dictionary<string, object>();
                            var propertyName = kvp.Key;
                            var propertyValue = kvp.Value;

                            if (propertyName != "message" && propertyName != "status")
                            {
                                if (propertyValue is string stringValue)
                                {
                                    try
                                    {
                                        var jsonArray = JArray.Parse(stringValue);
                                        string currentDirectory = Directory.GetCurrentDirectory();
                                        foreach (JObject item in jsonArray)
                                        {
                                            if (item.TryGetValue("t15_file_path", out JToken value))
                                            {
                                                string updatedPath = Path.Combine(currentDirectory, value.ToString());
                                                string modifiedPath = updatedPath.Replace("\\", "/");
                                                item["t15_file_path"] = modifiedPath; // Update the path in the JSON object
                                            }
                                        }
                                        accumulatedPairs[propertyName] = jsonArray.ToObject<List<Dictionary<string, object>>>();
                                    }
                                    catch (JsonException)
                                    {
                                        accumulatedPairs[propertyName] = stringValue;
                                    }
                                }
                                else
                                {
                                    accumulatedPairs[propertyName] = propertyValue;
                                }

                                finalResult.Add(accumulatedPairs);
                            }
                        }
                        return new ApiResponseModel<dynamic>
                        {
                            Data = finalResult,
                            Message = firstResult["message"]?.ToString(),
                            Status = Convert.ToInt32(firstResult["status"])
                        };
                    }
                    else
                    {
                        return new ApiResponseModel<dynamic>
                        {
                            Data = null,
                            Message = "No data returned",
                            Status = 204
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
                    Status = 500
                };
            }
        }




    }
}
