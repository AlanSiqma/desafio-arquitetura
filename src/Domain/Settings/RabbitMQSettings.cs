namespace Domain.Settings
{
    public class RabbitMQSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
