namespace form_builder.Configuration
{
    public class EmailConfiguration
    {
        public bool AttachPdf { get; set; }
        public string Body { get; set; }
        public List<string> FormName { get; set; }
        public List<string> Recipient { get; set; }
        public string Sender { get; set; } = "noreply@stockport.gov.uk";
        public string Subject { get; set; }
        public string CustomerBody { get; set; }
        public string EmailQuestionID { get; set; }
        public string CustomerSubject { get; set; }
    }
}
