namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class CommonMessage
{   
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public CommonMessageType Type { get; set; }
    [JsonProperty("s")]
    [JsonPropertyName("s")]
    public string? Subject { get; set; }
    [JsonProperty("b")]
    [JsonPropertyName("b")]
    public string? Body { get; set; }
    [JsonProperty("u")]
    [JsonPropertyName("u")]
    public string? Url { get; set; }
    [JsonProperty("h")]
    [JsonPropertyName("h")]
    public string? HelpUrl { get; set; }
    [JsonProperty("ic")]
    [JsonPropertyName("ic")]
    public string? Icon { get; set; }
    [JsonProperty("i")]
    [JsonPropertyName("i")]
    public string? Image { get; set; }
}
