using System.Text.Json.Serialization;

namespace DirectoryService.Shared;

public record Error
{
    public string Message { get; }
    public string Code { get; }
    public ErrorType Type { get; }
    public string? InvalidField { get; }

    [JsonConstructor]
    private Error(string message, string code, ErrorType type, string? invalidField = null)
    {
        Message = message;
        Code = code;
        Type = type;
        InvalidField = invalidField;
    }
    
    // паттерн - фабричные методы
    public static Error NotFound(string? code, string message)  
        => new(code ?? "record.not.found", message, ErrorType.NOT_FOUND);  
  
    public static Error Validation(string? code, string message, string?
        invalidField = null)  
        => new(code ?? "value.is.invalid", message, ErrorType.VALIDATION, invalidField);  
  
    public static Error Conflict(string? code, string message)  
        => new(code ?? "value.is.conflict", message, ErrorType.CONFLICT);  
  
    public static Error Failure(string? code, string message)  
        => new(code ?? "failure", message, ErrorType.FAILURE);
    
    public Errors ToErrors() => this;
}

public enum ErrorType
{
    VALIDATION,  
    NOT_FOUND,  
    FAILURE,  
    CONFLICT,
}