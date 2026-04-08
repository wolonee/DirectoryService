using DirectoryService.Shared;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DirectoryService.Presentation;

public class EnvelopeSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Envelope<Errors>))
        {
            if (schema.Properties.TryGetValue("errorList", out var errorsProp))
            {
                errorsProp.Items = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Error",
                    },
                };
            }
        }
    }
}