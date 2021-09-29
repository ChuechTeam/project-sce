using System.ComponentModel.DataAnnotations;

namespace Chuech.ProjectSce.Core.API.Features.Files
{
    public class FilesOptions
    {
        public const string ConfigurationSection = "Files";

        public string[] AllowedExtensions { get; init; } = Array.Empty<string>();
        [Required] public string Location { get; init; } = null!;
        
        public long MaximumSize { get; init; }
    }
}