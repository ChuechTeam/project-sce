using System.Text.Json.Serialization;

namespace Chuech.ProjectSce.Core.API.Data
{
    [JsonConverter(typeof(JsonStringEnumConverter))]    
    public enum SpaceMemberCategory
    {
        Participant,
        Manager
    }
}