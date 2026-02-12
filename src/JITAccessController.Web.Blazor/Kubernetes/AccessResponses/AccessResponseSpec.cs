using System.Text.Json.Serialization;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessResponseSpec
{
    [JsonPropertyName("requestRef")]
    public string RequestRef { get; set; } = "";

    [JsonPropertyName("response")]
    public string Response { get; set; } = "";

    [JsonPropertyName("approver")]
    public string Approver { get; set; } = "";

    [JsonPropertyName("groups")]
    public List<string> Groups { get; set; } = new List<string>();
}