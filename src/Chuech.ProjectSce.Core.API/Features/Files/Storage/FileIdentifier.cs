using System.Diagnostics.CodeAnalysis;

namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public readonly struct FileIdentifier
    {
        public static bool IsValidFileName([NotNullWhen(true)] string? fileName)
        {
            if (fileName is null)
            {
                return false;
            }

            foreach (var character in fileName)
            {
                if (character is
                    (>= 'a' and <= 'z') or
                    (>= 'A' and <= 'Z') or
                    (>= '0' and <= '9') or
                    '_' or '.')
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public static bool TryCreate(FileCategory category, string? name, out FileIdentifier identifier)
        {
            if (!IsValidFileName(name))
            {
                identifier = default;
                return false;
            }

            identifier = new FileIdentifier(category, name, false);
            return true;
        }

        public FileIdentifier(FileCategory category, string fileName) : this(category, fileName, true)
        {
        }

        private FileIdentifier(FileCategory category, string fileName, bool checkName)
        {
            if (checkName && !IsValidFileName(fileName))
            {
                throw new ArgumentException("The given file name contains invalid characters.", nameof(fileName));
            }

            FileName = fileName;
            Category = category;
        }

        public FileCategory Category { get; }
        public string FileName { get; }

        public override string ToString()
        {
            return Category.Name + "/" + FileName;
        }
    }
}