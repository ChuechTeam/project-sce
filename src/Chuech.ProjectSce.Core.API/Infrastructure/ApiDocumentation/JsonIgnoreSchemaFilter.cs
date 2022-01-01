using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Chuech.ProjectSce.Core.API.Infrastructure.ApiDocumentation;

public class JsonIgnoreSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties is null)
        {
            return;
        }

        var skipProperties = context.Type.GetProperties()
            .Where(t => t.GetCustomAttribute<JsonIgnoreAttribute>() is not null);

        foreach (var skipProperty in skipProperties)
        {
            var propertyToSkip = schema.Properties.Keys
                .SingleOrDefault(x => string.Equals(x, skipProperty.Name, StringComparison.OrdinalIgnoreCase));

            if (propertyToSkip is not null)
            {
                schema.Properties.Remove(propertyToSkip);
            }
        }
    }
}