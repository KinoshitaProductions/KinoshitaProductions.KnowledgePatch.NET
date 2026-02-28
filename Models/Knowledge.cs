using System.Collections.Specialized;

namespace KinoshitaProductions.KnowledgePatch.Models;
public class Knowledge
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IsDebugMode { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DebugHttpMessage? DebugRequestMessage { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DebugHttpMessage? DebugResponseMessage { get; set; }
    [JsonProperty("ef", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("ef")]
    public string? ExtendsFrom { get; set; }
    
    [JsonProperty("asl", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("asl")]
    public int? AllowedSubdomainLevels { get; set; }
    [JsonProperty("asm", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("asm")]
    public FlexiblePropertyMatcher[]? AllowedSubdomainsMatchers { get; set; }
    [JsonProperty("at", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("at")]
    public AuthenticationType? AuthenticationType { get; set; }
    [JsonProperty("ac", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("ac")]
    public AuthenticationConfiguration? AuthenticationConfiguration { get; set; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    protected AuthenticationValues? AuthenticationValues { get; set; }
    [JsonProperty("ed", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("ed")]
    public ErrorDetectionRule[]? ErrorDetections { get; set; }
    [JsonProperty("rh", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("rh")]
    public ListItemRule[]? RequestHeaders { get; set; }
    [JsonProperty("rp", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("rp")]
    public ListItemRule[]? RequestParameters { get; set; }
    [JsonProperty("rrt", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("rrt")]
    public FlexiblePropertyTransform[]? RawResponseTransforms { get; set; }
    // it'd be nice to have options set based on headers, but this will work for now
    [JsonProperty("ob", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("ob")]
    public Dictionary<string, string>? OptionsBase { get; set; }
    protected FrozenDictionary<string, string> Options { get; set; } = FrozenDictionary<string, string>.Empty;
    // ReSharper disable once MemberCanBePrivate.Global
    protected FrozenDictionary<string, string> RequestVariables { get; set; } = FrozenDictionary<string, string>.Empty;
    // ReSharper disable once MemberCanBePrivate.Global
    protected FrozenDictionary<string, string> ResponseVariables { get; set; } = FrozenDictionary<string, string>.Empty;
    // ReSharper disable once MemberCanBePrivate.Global
    protected FrozenDictionary<string, string> AuthVariables { get; set; } = FrozenDictionary<string, string>.Empty;

    private static bool MatchesAnnotations(HashSet<string> annotations, ErrorDetectionRule errorDetection)
    {
        bool matchedAnnotations = true;
        if (errorDetection.IfAnnotatedWith is { Length: > 0 })
            foreach (var requiredAnnotation in errorDetection.IfAnnotatedWith)
            {
                matchedAnnotations = annotations.Contains(requiredAnnotation);
                if (!matchedAnnotations) break;
            }
        return matchedAnnotations && 
            (errorDetection.AnnotationMatcher == null || 
            errorDetection.AnnotationMatcher.MatchesList(annotations.ToArray()));
    }
    private static bool MatchesContentOrHeaders(ErrorDetectionRule errorDetection, Web.DisposableResponseStream drs)
    {
        if (errorDetection.RawContentMatcher?.Matches(drs.ResponseBody ?? string.Empty) == false)
            return false;

        if (errorDetection.HeaderMatcher != null &&
            (drs.Response == null || !errorDetection.HeaderMatcher.MatchesList(drs.Response.Headers.Select(x => $"{x.Key}: {x.Value}").ToArray())))
            return false;
        return true;
    }

    public ErrorDetectionAction? MatchForErrorDetection(Web.DisposableResponseStream drs)
    {
        if (ErrorDetections == null || ErrorDetections.Length == 0) return null;
        var annotations = new HashSet<string>();
        foreach (var errorDetection in ErrorDetections)
        {
            // the status code check always runs if set
            if (errorDetection.StatusCode != null && drs.Response?.StatusCode == errorDetection.StatusCode)
                return errorDetection.Action;
            if (!MatchesAnnotations(annotations, errorDetection)) continue;
            if (!MatchesContentOrHeaders(errorDetection, drs)) continue;
      
            switch (errorDetection.Action.ActionType)
            {
                case ErrorDetectionActionType.Annotate:
                    annotations.Add(errorDetection.Action.Value!);
                    break;
                case ErrorDetectionActionType.RemoveAnnotation:
                    annotations.Remove(errorDetection.Action.Value!);
                    break;
                default:
                    return errorDetection.Action;
            }
        }
        return null;
    }
    // ReSharper disable once MemberCanBePrivate.Global
    public string ApplyRequestTemplating(string src)
    {
        if (AuthenticationConfiguration?.CustomHandling == true)
        {
            foreach (var tuple in AuthVariables)
                src = src.Replace(tuple.Key, tuple.Value);
        }
        foreach (var tuple in RequestVariables)
            src = src.Replace(tuple.Key, tuple.Value);
        return src;
    }
    // ReSharper disable once MemberCanBePrivate.Global
    public string ApplyResponseTemplating(string src)
    {
        foreach (var tuple in ResponseVariables)
            src = src.Replace(tuple.Key, tuple.Value);
        return src;
    }

    private void AddLiteralRequestHeaderIfMatches(HttpRequestMessage request, ListItemRule rule)
    {
        if (rule.Key == null || rule.Value == null) return;
        if (rule.RequestPathMatcher?.Matches(request.RequestUri!.AbsolutePath) != false 
            && rule.RequestHeaderMatchers?.All(x => x.MatchesList(request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value)).Select(t => $"{t.Key}: {t.Value}").ToArray())) != false)
            request.Headers.Add(rule.Key, ApplyRequestTemplating(rule.Value));
    }
    
    private static void DeleteRequestHeaderIfMatches(HttpRequestMessage request, ListItemRule rule)
    {
        if (rule.Key == null) return;
        if (rule.RequestPathMatcher?.Matches(request.RequestUri!.AbsolutePath) != false
            && rule.RequestHeaderMatchers?.All(x => x.MatchesList(request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value)).Select(t => $"{t.Key}: {t.Value}").ToArray())) != false)
            request.Headers.Remove(rule.Key);
    }

    
    private void InjectRequestHeaders(HttpRequestMessage request)
    {
        if (RequestHeaders is { Length: > 0 })
        {
            foreach (var header in RequestHeaders)
            {
                switch (header.Type)
                {
                    case ListItemAction.AddLiteral:
                        AddLiteralRequestHeaderIfMatches(request, header);
                        break;
                    case ListItemAction.Delete:
                        DeleteRequestHeaderIfMatches(request, header);
                        break;
                }
            }
        }
    }

    private static void InjectQueryStringAuthentication(NameValueCollection query, AuthenticationConfiguration configuration, AuthenticationValues values)
    {
        var userKey = configuration.UserKey;
        var tokenKey = configuration.TokenKey;
        if (!string.IsNullOrWhiteSpace(values.User))
            query[userKey] = values.User;
        if (!string.IsNullOrWhiteSpace(values.Token))
            query[tokenKey] = values.Token;
    }

    private static void InjectExtraRequestQueryParameters(HttpRequestMessage request, NameValueCollection query, ListItemRule[] rules)
    {
        foreach (var rule in rules)
        {
            if (rule.Key == null) continue;
            switch (rule.Type)
            {
                case ListItemAction.AddLiteral:
                    if (rule.Value != null && rule.RequestPathMatcher?.Matches(request.RequestUri!.AbsolutePath) != false)
                        query.Add(rule.Key, rule.Value);
                    break;
                case ListItemAction.Delete:
                    if (rule.RequestPathMatcher?.Matches(request.RequestUri!.AbsolutePath) != false)
                        query.Remove(rule.Key);
                    break;
            }
        }
    }

    private void InjectRequestAuthentication(HttpRequestMessage request)
    {
        bool usesAuthenticationQueryString = AuthenticationType == Common.Enums.AuthenticationType.QueryString;
        bool usesExtraParamsOnQueryString = RequestParameters is { Length: > 0 };
        if (AuthenticationValues != null && AuthenticationConfiguration?.CustomHandling != true && (usesAuthenticationQueryString || usesExtraParamsOnQueryString))
        {
            var builder = new UriBuilder(request.RequestUri!);
            var query = HttpUtility.ParseQueryString(builder.Query);
            if (usesAuthenticationQueryString && AuthenticationConfiguration != null)
                InjectQueryStringAuthentication(query, AuthenticationConfiguration, AuthenticationValues);
            if (usesExtraParamsOnQueryString && RequestParameters is { Length: > 0 })
                InjectExtraRequestQueryParameters(request, query, RequestParameters);
            builder.Query = query.ToString();
            request.RequestUri = builder.Uri;
        }
    }
    
    // ReSharper disable once UnusedMember.Global
    public HttpRequestMessage InjectWithKnowledgeInto(HttpRequestMessage request)
    {
        InjectRequestHeaders(request);
        InjectRequestAuthentication(request);
        if (IsDebugMode)
            DebugRequestMessage = DebugHttpMessage.From(request);
        return request;
    }

    private static bool MatchesRequestQueryParameters(HttpRequestMessage request, FlexiblePropertyTransform transform)
    {
        if (transform.RequestQueryParameterMatchers is { Length: > 0 })
        {
            var parsedQuery = HttpUtility.ParseQueryString(request.RequestUri!.Query);
            var queryParameters = new List<string>();
            for (var i = 0; i < parsedQuery.Count; ++i)
                queryParameters.Add($"{parsedQuery.GetKey(i)}={parsedQuery.Get(i)}");
            if (!transform.RequestQueryParameterMatchers.Any(matcher => matcher.MatchesList(queryParameters.ToArray())))
                return false; // cannot apply this
        }
        return true;
    }
    
    // ReSharper disable once UnusedMember.Global
    public string TransformResponseWithKnowledge(HttpRequestMessage request, HttpResponseMessage response, string? responseBody)
    {
        responseBody ??= string.Empty;
        if (RawResponseTransforms is { Length: > 0 })
        {
            foreach (var transform in RawResponseTransforms.Where(t =>
                         (t.RequestPathMatcher == null || t.RequestPathMatcher.Matches(request.RequestUri!.AbsolutePath))
                         && (t.RequestQueryStringMatcher == null || t.RequestQueryStringMatcher.Matches(request.RequestUri!.Query))))
            {
                if (!MatchesRequestQueryParameters(request, transform)) continue;
                if (transform.ValueTransform != null)
                    responseBody = transform.Transform(responseBody, ApplyResponseTemplating, ApplyResponseTemplating(transform.ValueTransform));
            }
        }
        if (IsDebugMode)
            DebugResponseMessage = DebugHttpMessage.From(response, responseBody); 
        return responseBody;
    }
}
