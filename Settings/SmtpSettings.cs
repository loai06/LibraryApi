namespace LibraryManagementSystem.Settings
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } =587;
        public string FromName { get; set; } = "Library API";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail {get; set;} =string.Empty;
        public string Fromname { get; set;} =string.Empty;
    }
}