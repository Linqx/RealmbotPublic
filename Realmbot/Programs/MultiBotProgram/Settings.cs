using System.IO;
using Newtonsoft.Json;

namespace MultiBot {
    public class Settings {
        [JsonProperty("encryption_data")] public EncryptionData EncryptionData { get; set; }

        [JsonProperty("preferredServer")] public string PreferredServer { get; set; }

        [JsonProperty("accounts")] public AccountDetails[] Accounts { get; set; }

        public static Settings FromResource(string resource) {
            using (StreamReader rdr = new StreamReader(resource)) {
                string json = rdr.ReadToEnd();
                return JsonConvert.DeserializeObject<Settings>(json);
            }
        }
    }
}