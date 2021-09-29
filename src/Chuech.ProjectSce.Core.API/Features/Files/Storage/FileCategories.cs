using System.Diagnostics.CodeAnalysis;

namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public static class FileCategories
    {
        public static readonly FileCategory PrivateTestCategory = new("private-test", false);
        public static readonly FileCategory PublicTestCategory = new("public-test", true);

        public static readonly FileCategory DocumentResources = new("document-resources", false);

        public static readonly IReadOnlyList<FileCategory> AllCategories = new[]
        {
            PrivateTestCategory,
            PublicTestCategory,
            DocumentResources
        };
        
        public static bool TryFind(string rawCategory, [MaybeNullWhen(false)] out FileCategory fileCategory)
        {
            foreach (var category in AllCategories)
            {
                if (category.Name == rawCategory)
                {
                    fileCategory = category;
                    return true;
                }
            }

            fileCategory = null;
            return false;
        }
        
        public static FileCategory? Find(string rawCategory)
        {
            foreach (var category in AllCategories)
            {
                if (category.Name == rawCategory)
                {
                    return category;
                }
            }

            return null;
        }
    }
}