namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class ErrorDetectionAction
{
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public ErrorDetectionActionType ActionType { get; set; }
    [JsonProperty("s")]
    [JsonPropertyName("s")]
    public int? StatusCode { get; set; }
    [JsonProperty("v")]
    [JsonPropertyName("v")]
    public string? Value { get; set; }
    [JsonProperty("m")]
    [JsonPropertyName("m")]
    public CommonMessage? Message { get; set; }
}