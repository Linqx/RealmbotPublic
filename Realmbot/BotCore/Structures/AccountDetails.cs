using Newtonsoft.Json;

namespace BotCore.Structures {
    public class AccountDetails {
        [JsonProperty("email")] public string Email { get; set; }

        [JsonProperty("password")] public string Password { get; set; }

        [JsonProperty("charId")] public int CharId { get; set; }
    }
}