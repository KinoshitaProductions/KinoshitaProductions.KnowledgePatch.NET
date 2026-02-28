namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class ListItemRule
{
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public ListItemAction Type { get; set; }
    [JsonProperty("k")]
    [JsonPropertyName("k")]
    public string? Key { get; set; }
    [JsonProperty("v")]
    [JsonPropertyName("v")]
    public string? Value { get; set; }
    [JsonProperty("rp")]
    [JsonPropertyName("rp")]
    public FlexiblePropertyMatcher? RequestPathMatcher { get; set; }
    [JsonProperty("rh")]
    [JsonPropertyName("rh")]
    public FlexiblePropertyMatcher[]? RequestHeaderMatchers { get; set; }
}