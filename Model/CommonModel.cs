namespace MRO_Api.Model
{
    public class CommonModel
    {
       public class CreateModel
       {
            public Dictionary<string, string> data { get; set; }
            public string action { get; set; }
            public string fakeName { get; set; }
            public string programId { get; set; }
       }


        public class  DeleteModel 
        {
            public DeleteDataModel data { get; set; }
            public string action { get; set; }
            public string fakeName { get; set; }
            public string programId { get; set; }
        }


    }
}
