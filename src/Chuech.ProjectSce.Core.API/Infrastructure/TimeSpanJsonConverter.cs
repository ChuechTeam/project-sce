using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Chuech.ProjectSce.Core.API.Infrastructure
{
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            try
            {
                return stringValue is null ? default : XmlConvert.ToTimeSpan(stringValue);
            }
            catch (Exception e)
            {
                throw new JsonException("Invalid TimeSpan.", e);
            }
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(XmlConvert.ToString(value));
        }
    }
}