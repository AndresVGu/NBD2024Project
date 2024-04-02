namespace NBDProject2024.ViewModels
{
    public class EmailMessage
    {
        public List<EmailAddress> ToAdresses { get; set; }
            = new List<EmailAddress>();

        public string Subject { get; set; }

        public string Content { get; set; }
    }
}
