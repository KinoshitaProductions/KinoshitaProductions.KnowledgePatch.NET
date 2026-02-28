namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class AuthenticationConfiguration
{
    [JsonProperty("c")]
    [JsonPropertyName("c")]
    public bool CustomHandling { get; set; }
    [JsonProperty("u")]
    [JsonPropertyName("u")]
    public string? UserKey { get; set; }
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public string? TokenKey { get; set; }
}
