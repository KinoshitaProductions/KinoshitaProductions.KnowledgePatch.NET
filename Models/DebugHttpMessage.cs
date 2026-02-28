namespace KinoshitaProductions.KnowledgePatch.Models;

public class DebugHttpMessage
{
    private Uri? Uri { get; init; }
    private string? Method { get; init; }
    private string? Path { get; init; }
    private KeyValuePair<string, string>[]? Query { get; init; }
    private KeyValuePair<string, string>[]? Headers { get; init; }
    private string? Payload { get; init; }
    private int StatusCode { get; init; }

    public static DebugHttpMessage? From(HttpRequestMessage? request)
    {
        if (request?.RequestUri == null) return null;
        var queryList = new List<KeyValuePair<string, string>>();
        var parsedQuery = HttpUtility.ParseQueryString(request.RequestUri.Query);
        foreach (var key in parsedQuery.AllKeys)
        {
            if (key == null) continue;
            var values = parsedQuery.GetValues(key);
            if (values == null) continue;
            foreach (var value in values)
            {
                queryList.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        return new DebugHttpMessage
        {
            Uri = request.RequestUri,
            Path = request.RequestUri.AbsolutePath,
            Method = request.Method.ToString(),
#if WINDOWS_UWP
            Headers = request.Headers.Select(x => KeyValuePair.Create(x.Key, x.Value)).ToArray(),
#else
            Headers = request.Headers.SelectMany(x => x.Value, (x, y) => KeyValuePair.Create(x.Key, y)).ToArray(),
#endif
            Query = queryList.ToArray(),
        };
    }

    public static DebugHttpMessage? From(HttpResponseMessage? response, string payload)
    {
        if (response?.StatusCode is null or 0) return null;
        return new DebugHttpMessage
        {
#if WINDOWS_UWP
                Headers = response.Headers.Select(x => KeyValuePair.Create(x.Key, x.Value)).ToArray(),
#else
            Headers = response.Headers.SelectMany(x => x.Value, (x, y) => KeyValuePair.Create(x.Key, y)).ToArray(),
#endif
            StatusCode = (int)response.StatusCode,
            Payload = payload,
        };
    }

    /// <summary>
    /// This function should be called letting the user know about the sensitivity of the information to be shown beforehand.
    /// It requires showing API keys and other sensitive information due to potential typos, extra spaces, or misconfigurations on how credentials should be used.
    /// </summary>
    /// <returns></returns>
    // ReSharper disable once UnusedMember.Global
    public string PrettyPrint()
    {
        var sb = new StringBuilder();
        if (Method != null && Path != null)
        {
            sb.AppendLine("HTTP Request:");
            sb.AppendLine($"{Method} {Path}");
        }

        if (Query is { Length: > 0 })
        {
            sb.AppendLine("\nQuery Parameters:");
            foreach (var queryParameter in Query)
            {
                sb.AppendLine($"{queryParameter.Key}: {queryParameter.Value}");
            }
        }

        if (StatusCode != 0)
        {
            sb.AppendLine("HTTP Response:");
            sb.AppendLine($"Status Code: {StatusCode}");
        }

        if (Headers is { Length: > 0 })
        {
            sb.AppendLine("\nHeaders:");
            foreach (var header in Headers)
            {
                sb.AppendLine($"{header.Key}: {header.Value}");
            }
        }

        if (!string.IsNullOrEmpty(Payload))
        {
            sb.AppendLine($"\nPayload:\n{Payload}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// This function should be called letting the user know about the sensitivity of the information to be shown beforehand.
    /// It requires showing API keys and other sensitive information due to potential typos, extra spaces, or misconfigurations on how credentials should be used.
    /// </summary>
    /// <returns></returns>
    // ReSharper disable once UnusedMember.Global
    public string AsCurl()
    {
        if (Uri == null) return "Not captured";
        var sb = new StringBuilder();
        if (Method != null && Path != null)
        {
            sb.AppendLine("curl \\");
            sb.AppendLine($"  -X {Method} {Uri.AbsoluteUri} \\");
        }

        if (Headers is { Length: > 0 })
        {
            foreach (var header in Headers)
            {
                sb.AppendLine($"  -H '{header.Key}: {header.Value}'");
            }
        }

        if (Payload != null)
        {
            sb.AppendLine($"  -d '{Payload}' \\");
        }

        return sb.ToString().TrimEnd('\\').TrimEnd();
    }
}