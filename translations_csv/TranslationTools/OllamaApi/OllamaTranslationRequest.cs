using Newtonsoft.Json;
using System.Text.Json;

namespace TranslationTools.OllamaApi;

public class OllamaTranslationRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = "translategemma:12b";
    [JsonProperty("prompt")]
    public string Prompt { get; set; } = string.Empty;
    [JsonProperty("stream")]
    public bool Stream { get; set; } = false;   
}
