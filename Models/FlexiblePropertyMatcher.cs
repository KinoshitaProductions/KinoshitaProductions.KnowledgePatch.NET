namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class FlexiblePropertyMatcher
{
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public FlexiblePropertyMatcherTypes Type { get; set; }
    [JsonProperty("v")]
    [JsonPropertyName("v")]
    public string[]? ValuesToMatch { get; set; }

    private string[][]? _valuesToMatchSplit;
    private string[][]? ValuesToMatchSplit => _valuesToMatchSplit ??= ValuesToMatch?.Select(v => new [] { v }).ToArray();
    public bool Matches(string inputValue, string[]? overrideValuesToMatch = null)
    {
        var operation = (FlexiblePropertyMatcherTypes)((int)Type % 32);
        var valuesToMatch = overrideValuesToMatch ?? ValuesToMatch;
        if (valuesToMatch == null || valuesToMatch.Length == 0) return false;
        switch (operation)
        {
            case FlexiblePropertyMatcherTypes.Literal:
                return Type.HasFlag(FlexiblePropertyMatcherTypes.EachItemMatchesAllValues) ? valuesToMatch.All(value => inputValue == value) : valuesToMatch.Any(value => inputValue == value);
            case FlexiblePropertyMatcherTypes.Contains:
                return Type.HasFlag(FlexiblePropertyMatcherTypes.EachItemMatchesAllValues) ? valuesToMatch.All(inputValue.Contains) : valuesToMatch.Any(inputValue.Contains);
            case FlexiblePropertyMatcherTypes.Prefix:
                return Type.HasFlag(FlexiblePropertyMatcherTypes.EachItemMatchesAllValues) ? valuesToMatch.All(inputValue.StartsWith) : valuesToMatch.Any(inputValue.StartsWith);
            case FlexiblePropertyMatcherTypes.Suffix:
                return Type.HasFlag(FlexiblePropertyMatcherTypes.EachItemMatchesAllValues) ? valuesToMatch.All(inputValue.EndsWith) : valuesToMatch.Any(inputValue.EndsWith);
            case FlexiblePropertyMatcherTypes.Regex:
                return Type.HasFlag(FlexiblePropertyMatcherTypes.EachItemMatchesAllValues) ? valuesToMatch.All(value => Regex.IsMatch(inputValue, value, RegexOptions.None, TimeSpan.FromMilliseconds(100))) : valuesToMatch.Any(value => Regex.IsMatch(inputValue, value, RegexOptions.None, TimeSpan.FromMilliseconds(100)));
        }
        return false; // not supported/unknown method
    }
    
    private bool AllListItemsMatchAllValues(string[] inputValues)
    {
         var allMatch = true;
        foreach (var checkingValue in inputValues)
        {
            allMatch = Matches(checkingValue);
            if (!allMatch) break;
        }
        return allMatch;
    }

    private bool EachValueMatchesAtLeastOneListItem(string[] inputValues)
    {
        var thereAreValuesToMatch = ValuesToMatch is { Length: > 0 };
        var allMatch = thereAreValuesToMatch;
        if (thereAreValuesToMatch)
        {
            foreach (var requiredValue in ValuesToMatchSplit!)
            {
                var atLeastOneMatches = false;
                foreach (var checkingValue in inputValues)
                {
                    atLeastOneMatches = Matches(checkingValue, requiredValue);
                    if (atLeastOneMatches) break;
                }
                if (!atLeastOneMatches)
                {
                    allMatch = false;
                    break;
                }
            }
        }
        return allMatch;
    }

    private bool AnyValueMatchesAtLeastOneListItem(string[] inputValues)
    {
        var oneMatches = false;
        foreach (var checkingValue in inputValues)
        {
            oneMatches = Matches(checkingValue);
            if (oneMatches) break;
        }
        return oneMatches;
    }

    public bool MatchesList(string[] inputValues)
    {
        var truthyValue = !Type.HasFlag(FlexiblePropertyMatcherTypes.NegatedMatch);
        if (Type.HasFlag(FlexiblePropertyMatcherTypes.AllListItemsMatchAllValues))
           return AllListItemsMatchAllValues(inputValues) ? truthyValue : !truthyValue;
        if (Type.HasFlag(FlexiblePropertyMatcherTypes.EachValueMatchesAtLeastOneListItem))
            return EachValueMatchesAtLeastOneListItem(inputValues) ? truthyValue : !truthyValue;
        return AnyValueMatchesAtLeastOneListItem(inputValues) ? truthyValue : !truthyValue;
    }
}


