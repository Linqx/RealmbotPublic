using System.IO;
using BotCore.Networking.Crypt;
using Newtonsoft.Json;

namespace KeyBotProgram {
    public class Settings {
        [JsonProperty("encryption_data")] public EncryptionData EncryptionData { get; set; }

        [JsonProperty("email")] public string Email { get; set; }

        [JsonProperty("password")] public string Password { get; set; }

        [JsonProperty("charId")] public int CharId { get; set; }

        [JsonProperty("whiteList")] public string[] Whitelist { get; set; }

        public static Settings FromResource(string resource) {
            using (StreamReader rdr = new StreamReader(resource)) {
                string json = rdr.ReadToEnd();
                return JsonConvert.DeserializeObject<Settings>(json);
            }
        }
    }
}