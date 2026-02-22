using System.Collections.Generic;
namespace TranslationTools.OllamaApi;

/// <summary>
/// Represents the response object returned by the Ollama API when making a request to generate a response from a model.
/// </summary>
public class OllamaResponseObject
{
    public string Model { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public bool Done { get; set; }
    public string DoneReason { get; set; } = string.Empty;
    public List<int> Context { get; set; } = [];
    public int TotalDuration { get; set; }
    public int LoadDuration { get; set; }
    public int PromptEvalCount { get; set; }
    public int PromptEvalDuration { get; set; }
    public int EvalCount { get; set; }
    public int EvalDuration { get; set; }
}
