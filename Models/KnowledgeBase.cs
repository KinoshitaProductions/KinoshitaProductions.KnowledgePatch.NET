namespace KinoshitaProductions.KnowledgePatch.Models;
public sealed class KnowledgeBase<T> where T : Knowledge, new()
{
    [JsonProperty("kpf")]
    [JsonPropertyName("kpf")]
    // ReSharper disable once UnusedMember.Global
    public Dictionary<string, T> BuildingKnowledgePerFqdn { get; set; } = new ();
    [JsonProperty("bkpf")] // to be removed in the future, this is for legacy API support
    [JsonPropertyName("bkpf")] // to be removed in the future, this is for legacy API support
    // ReSharper disable once UnusedMember.Global
    public Dictionary<string, T> BuildingKnowledgePerFqdnLegacy { get => BuildingKnowledgePerFqdn; set => BuildingKnowledgePerFqdn = value; }
    private FrozenDictionary<string, T> _knowledgePerFqdn = FrozenDictionary<string, T>.Empty;
    private readonly T _none = new();
    private readonly char[] _separatorChars = { '/', ':', '?', '&' };
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsPrepared { get; private set; } // once prepared, 
    // ReSharper disable once MemberCanBePrivate.Global
    public void MarkAsPrepared()
    {
        _knowledgePerFqdn = BuildingKnowledgePerFqdn.ToFrozenDictionary();
        IsPrepared = true;
    }
    // ReSharper disable once MemberCanBePrivate.Global
    public T LookupKnowledgeFor(string fqdn)
    {
        if (!IsPrepared || String.IsNullOrWhiteSpace(fqdn)) return _none;
        if (fqdn.Contains('@')) fqdn = fqdn.Split('@')[1];
        fqdn = fqdn.Split(_separatorChars)[0];
        var subdomainFqdn = fqdn;
        var key = HexToBase64(GetChecksumFor(fqdn, ChecksumAlgorithm.Md5));
        if (_knowledgePerFqdn.TryGetValue(key, out var value)) return ResolveDelegations(value);
        var subdomainLevel = 1;
        while (true)
        {
            var cutPosition = fqdn.IndexOf('.', StringComparison.InvariantCulture) + 1;
            if (cutPosition <= 0 || cutPosition >= fqdn.Length) break;
            fqdn = fqdn[cutPosition..];
            key = HexToBase64(GetChecksumFor(fqdn, ChecksumAlgorithm.Md5));
            if (_knowledgePerFqdn.TryGetValue(key, out var tldKnowledge) && 
                    tldKnowledge.AllowedSubdomainLevels.HasValue &&
                    subdomainLevel <= tldKnowledge.AllowedSubdomainLevels.Value &&
                    (tldKnowledge.AllowedSubdomainsMatchers == null ||
                    tldKnowledge.AllowedSubdomainsMatchers.Length == 0 ||
                    tldKnowledge.AllowedSubdomainsMatchers.Any(x => x.Matches(subdomainFqdn))))
            {
                return ResolveDelegations(tldKnowledge);
            }
            ++ subdomainLevel;
        }
        return _none;
    }

    private T ResolveDelegations(T booruKnowledge, int attempts = 0, T? topLevel = null)
    {
        if (booruKnowledge.ExtendsFrom == null) 
            return booruKnowledge;
        if (attempts > 10) return topLevel!;
        if (_knowledgePerFqdn.TryGetValue(booruKnowledge.ExtendsFrom, out var matchingKnowledge))
        {
            if (matchingKnowledge.ExtendsFrom != null) 
                return ResolveDelegations(matchingKnowledge, attempts+1, topLevel ?? booruKnowledge);
            return CloneForDelegation(matchingKnowledge, topLevel ?? booruKnowledge);
        }
        return booruKnowledge;
    }

    // ReSharper disable once UnusedMember.Global
    public void AddKnowledgeFrom(KnowledgeBase<T> knowledgeBase)
    {
        if (_knowledgePerFqdn.Count == 0)
            BuildingKnowledgePerFqdn = knowledgeBase.BuildingKnowledgePerFqdn;
        else
            foreach (var tuple in knowledgeBase._knowledgePerFqdn)
                BuildingKnowledgePerFqdn[tuple.Key] = tuple.Value;
    }

    private static T CloneForDelegation(T knowledge, T delegator)
    {
        var overrides = JObject.FromObject(delegator);
        var result = JObject.FromObject(knowledge);
        foreach (var propertyName in overrides.Properties().Select(x => x.Name))
            result[propertyName] = overrides[propertyName];
        var clonedKnowledge = result.ToObject<T>()!;
        clonedKnowledge.ExtendsFrom = null;
        return clonedKnowledge;
    }
}
