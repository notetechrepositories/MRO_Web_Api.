using Dapper;
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


 
        public async Task<ApiResponseModel<dynamic>> commonGet(CommonModel commonModel)
        {
            
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    // Serialize the request object to JSON
                    var jsonData = JsonConvert.SerializeObject(commonModel);

                    // Call the stored procedure
                    var result = await connection.QueryAsync<dynamic>(
                        "api_crud_sp", // Stored procedure name
                        new { jsonData },
                        commandType: CommandType.StoredProcedure
                    );
                    return new ApiResponseModel<dynamic>
                    {
                        Data =result,
                        Message = "Successfully retrieved data",
                        Status = 200
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

                    _communicationUtilities.SendMail(emailDTOModel);
                    

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
                        Data = encryptedData,
                        Message = "Successfully retrieved data",
                        Status = 200
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


       
        public async Task<string> otpVerification(string encryptedData, string otp)
        {
            try
            {
                string status = "";

                // Decrypt the encrypted data
                var otpAndTimeDecrypted = Decryption.Decrypt(encryptedData);

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
                    if (storedOTP == otp.Trim())
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
