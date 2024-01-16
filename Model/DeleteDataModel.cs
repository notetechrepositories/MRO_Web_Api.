namespace MRO_Api.Model
{
    public class DeleteDataModel
    {
        public Dictionary<string, string> condition { get; set; }
        public int updateAction { get; set; }
        public string updateActionValue { get; set; }
    }
}
