using System.Text.Json.Serialization;

namespace HomeControl.Integrations.TPLink.JSON
{
    public class SysInfo : Response
    {
        [JsonPropertyName("alias")]
        public string Alias { get; set; }

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("dev_name")]
        public string DeviceName { get; set; }

        [JsonPropertyName("oemId")]
        public string OemId { get; set; }

        [JsonPropertyName("sw_ver")]
        public string SoftwareVersion { get; set; }

        [JsonPropertyName("hwId")]
        public string HardwareId { get; set; }

        [JsonPropertyName("hw_ver")]
        public string HardwareVersion { get; set; }

        [JsonPropertyName("fwId")]
        public string FirmwareId { get; set; }

        [JsonPropertyName("mic_type")]
        public string Type { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("mac")]
        public string MacAddress { get; set; }

        [JsonPropertyName("rssi")]
        public int Rssi { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("latitude_i")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude_i")]
        public double Longitude { get; set; }
    }
}
