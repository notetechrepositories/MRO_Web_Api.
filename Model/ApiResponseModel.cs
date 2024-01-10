namespace MRO_Api.Model
{
    public class ApiResponseModel<T>
    {
       
            public T Data { get; set; }
            public object Message { get; set; }
            public long Status { get; set; }
        
    }
}
