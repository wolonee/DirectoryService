using System.Text.Json.Serialization;

namespace DirectoryService.Shared;

public record Envelope
{
    public object? Result { get; }
    
    public Errors? ErrorList { get; }
    
    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(object? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = null) =>
        new(result, null);
    
    public static Envelope Errors(Errors? errorList = null) =>
        new(null, errorList);
}

public record Envelope<T>
{
    public T? Result { get; }
    
    public Errors? ErrorList { get; }
    
    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(T? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope<T> Ok(T? result = default) =>
        new(result, null);
    
    public static Envelope<T> Errors(Errors? errorList = null) =>
        new(default, errorList);
}

