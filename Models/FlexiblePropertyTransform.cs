namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class FlexiblePropertyTransform
{
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public FlexiblePropertyTransformType Type { get; set; }
    [JsonProperty("v")]
    [JsonPropertyName("v")]
    public string[]? ValuesToMatch { get; set; }
    [JsonProperty("vt")]
    [JsonPropertyName("vt")]
    public string? ValueTransform { get; set; }
    [JsonProperty("rp")]
    [JsonPropertyName("rp")]
    public FlexiblePropertyMatcher? RequestPathMatcher { get; set; }
    [JsonProperty("rqs")]
    [JsonPropertyName("rqs")]
    public FlexiblePropertyMatcher? RequestQueryStringMatcher { get; set; }
    [JsonProperty("rqp")]
    [JsonPropertyName("rqp")]
    public FlexiblePropertyMatcher[]? RequestQueryParameterMatchers { get; set; }
    public string Transform(string? inputValue, Func<string, string>? valueToMatchTransform, string templatedValueTransform)
    {
        inputValue ??= string.Empty;
        if (ValuesToMatch == null || ValuesToMatch.Length == 0) return inputValue;
        var operation = (FlexiblePropertyTransformType)((int)Type % 32);
        switch (operation)
        {
            case FlexiblePropertyTransformType.LiteralReplace:
                foreach (var value in ValuesToMatch)
                {
                    var templatedRequiredValue = valueToMatchTransform?.Invoke(value) ?? value;
                    inputValue = inputValue.Replace(templatedRequiredValue, templatedValueTransform);
                    if (!Type.HasFlag(FlexiblePropertyTransformType.MatchesAll)) break;
                }
                break;
        }

        return inputValue;
    }
}