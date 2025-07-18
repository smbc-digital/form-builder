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
		public bool PageTitlesInEmailGeneration { get; set; } = false;
	}
}
