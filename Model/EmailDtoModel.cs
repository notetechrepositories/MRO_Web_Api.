namespace MRO_Api.Model
{
    public class EmailDtoModel
    {
        public string to_email { get;set; }
        public string t16_email_subject { get; set; }
        public string t16_email_bcc { get; set; }
        public string t16_email_html_body { get; set; }
        public string t16_email_cc { get; set; }
        public string from_email { get; set; }
        public string from_email_password { get; set; }       
        public string signature_content { get; set; }   
    }
}
