using Dapper;
using Microsoft.AspNetCore.SignalR;
using MRO_Api.Context;
using MRO_Api.Hubs;
using MRO_Api.IRepository;
using MRO_Api.Model;
using Newtonsoft.Json;
using System.Data;

namespace MRO_Api.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly DapperContext _dapperContext;
      
       

        public AuthenticationRepository(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
       
        }


        



    }
}
