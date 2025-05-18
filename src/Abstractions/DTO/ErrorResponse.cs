using System.Text.Json.Serialization;

namespace Abstractions.DTO;

/// <summary>
/// Standardized error response for the API
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error status code
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Error title or message
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed error information
    /// </summary>
    public string? Detail { get; set; }
    
    /// <summary>
    /// Error code for specific application error types
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Path where the error occurred
    /// </summary>
    public string? Path { get; set; }
    
    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Request ID for tracing
    /// </summary>
    public string? RequestId { get; set; }
    
    /// <summary>
    /// Additional error details for development environments
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, List<string>>? Errors { get; set; }
}
