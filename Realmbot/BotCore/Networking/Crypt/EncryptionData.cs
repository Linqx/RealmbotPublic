using Newtonsoft.Json;

namespace BotCore.Networking.Crypt {
    public struct EncryptionData {
        [JsonProperty("rsa")] public string RSA { get; set; }

        [JsonProperty("incoming_rc4")] public string IncomingRC4 { get; set; }

        [JsonProperty("outgoing_rc4")] public string OutgoingRC4 { get; set; }
    }
}