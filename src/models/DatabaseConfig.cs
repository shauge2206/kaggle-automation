using System.Text.Json.Serialization;

namespace src.models
{

    public class DatabaseConfig
    {

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("database")]
        public string Database { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("port")]
        public string Port { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }


        public string GetConnectionString()
        {
            return $"Host={Host};Port={Port};Username={User};Password={Password};Database={Database}";
        }
    }
}