namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class ErrorDetectionRule
{
    [JsonProperty("s")]
    [JsonPropertyName("s")]
    public HttpStatusCode? StatusCode { get; set; }
    
    [JsonProperty("if")]
    [JsonPropertyName("if")]
    public string[]? IfAnnotatedWith { get; set; }
    
    [JsonProperty("r")]
    [JsonPropertyName("r")]
    public FlexiblePropertyMatcher? RawContentMatcher { get; set; }
    
    [JsonProperty("h")]
    [JsonPropertyName("h")]
    public FlexiblePropertyMatcher? HeaderMatcher { get; set; }
    [JsonProperty("an")]
    [JsonPropertyName("an")]
    public FlexiblePropertyMatcher? AnnotationMatcher { get; set; }
    
    [JsonProperty("p")]
    [JsonPropertyName("p")]
    public FlexiblePropertyMatcher? PropertyMatcher { get; set; }
    
    [JsonProperty("pv")]
    [JsonPropertyName("pv")]
    public FlexiblePropertyMatcher? PropertyValueMatcher { get; set; }
    
    [JsonProperty("a")]
    [JsonPropertyName("a")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ErrorDetectionAction Action { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}