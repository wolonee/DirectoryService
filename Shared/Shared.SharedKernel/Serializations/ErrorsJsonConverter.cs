using System.Text.Json;
using System.Text.Json.Serialization;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Shared.Serializations;

public class ErrorsJsonConverter : JsonConverter<Errors.Errors>
{
    public override Errors.Errors Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var errors = JsonSerializer.Deserialize<List<Error>>(ref reader, options);

        return errors is null
            ? new Errors.Errors([])
            : new Errors.Errors(errors);
    }

    public override void Write(Utf8JsonWriter writer, Errors.Errors value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.ToList(), options);
}