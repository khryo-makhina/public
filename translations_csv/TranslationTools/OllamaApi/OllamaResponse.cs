using Newtonsoft.Json;

namespace TranslationTools.OllamaApi;

public class OllamaResponse
{
    [JsonProperty("response")]
    public string Response { get; set; } = string.Empty;

    [JsonProperty("context")]
    public List<long>? Context { get; set; } = null;

    [JsonProperty("done")]
    public bool Done { get; set; }
}