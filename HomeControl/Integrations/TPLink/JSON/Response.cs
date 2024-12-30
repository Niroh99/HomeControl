using System.Text.Json.Serialization;

namespace HomeControl.Integrations.TPLink.JSON
{
    public class Response
    {
        [JsonPropertyName("err_code")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("err_msg")]
        public string ErrorMessage { get; set; }
    }
}