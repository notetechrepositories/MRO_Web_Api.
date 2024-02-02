namespace MRO_Api.Model
{
    public class ApiResponseModel<T>
    {       
            public T Data { get; set; }
            public object Message { get; set; }
            public dynamic Status { get; set; }
        
    }
}
