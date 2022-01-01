using System.Security.Cryptography;
using System.Text;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Invitations;

public class Invitation : IHaveCreationDate
{
    private static readonly RandomNumberGenerator s_rng = RandomNumberGenerator.Create();

    private Invitation()
    {
        Id = null!;
        CanonicalId = null!;
    }
        
    public Invitation(string canonicalId, int institutionId, int creatorId, int usagesLeft,
        Instant expirationDate)
    {
        Id = NormalizeId(canonicalId);
        CanonicalId = canonicalId;
        InstitutionId = institutionId;
        ExpirationDate = expirationDate;
        CreatorId = creatorId;
        UsagesLeft = usagesLeft;
    }

    /// <summary>
    ///     <para>
    ///         The identifier of this invitation.
    ///     </para>
    ///     <para>
    ///         It is composed of characters a-z, A-Z, 0-9 and _ (the underscore).
    ///         The dash (<c>-</c>), when normalized, is ignored. Some characters with
    ///         diacritics will have their diacritics removed.
    ///     </para>
    /// </summary>
    public string Id { get; }

    public string CanonicalId { get; }

    public Institution Institution { get; private set; } = null!;
    public int InstitutionId { get; private set; }

    public Instant ExpirationDate { get; set; }
    public Instant CreationDate { get; set; }
    public int UsagesLeft { get; private set; }

    public User Creator { get; private set; } = null!;
    public int CreatorId { get; private set; }

    public void ConsumeOneUsage()
    {
        if (UsagesLeft == 0)
        {
            throw new Error("Invitation expired.", "invitation.expired").AsException();
        }

        UsagesLeft--;
    }

    public static string GenerateUniversalInvitationId()
    {
        // 0 (number) and O (letter) are purposefully excluded as they can be easily
        // mistaken for each other.
        const string Characters = "abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ123456789";
        const int Length = 8;

        Span<char> result = stackalloc char[Length];
        Span<byte> randomBuffer = stackalloc byte[4 * Length];
        s_rng.GetBytes(randomBuffer); // Fill the buffer
        for (var i = 0; i < Length; i++)
        {
            var randomNumber = BitConverter.ToUInt32(randomBuffer.Slice(i * 4, 4));
            var character = Characters[(int) (randomNumber % Characters.Length)];
            result[i] = character;
        }

        return new string(result);
    }

    public static string NormalizeId(string id)
    {
        var newId = new StringBuilder(id.Length);
        foreach (var character in id)
        {
            if (character is
                (>= 'a' and <= 'z') or
                (>= 'A' and <= 'Z') or
                (>= '0' and <= '9') or
                '_')
            {
                // We can insert it safely!
                newId.Append(character);
                continue;
            }

            if (character is '-' or ' ')
                // The dash and the space should be ignored.
                // This way, pomme-banane or pomme banane is the same as pommebanane
            {
                continue;
            }

            if (!char.IsLetter(character))
            {
                // Non letters should be underscores.
                newId.Append('_');
                continue;
            }

            // Try to remove the diacritics.
            var lowerCaseCharacter = char.ToLower(character);
            var wasLowerCase = character == lowerCaseCharacter;

            var diacriticRemovedString = lowerCaseCharacter switch
            {
                'â' or 'à' or 'ä' => "a",
                'ê' or 'é' or 'è' or 'ë' => "e",
                'î' or 'ï' or 'ì' => "i",
                'ô' or 'ö' or 'ò' => "o",
                'û' or 'ù' or 'ü' => "u",
                'ñ' => "n",
                'ç' => "c",
                'œ' => "oe",
                'æ' => "ae",
                _ => null
            };

            if (diacriticRemovedString is not null)
            {
                newId.Append(wasLowerCase
                    ? diacriticRemovedString
                    : diacriticRemovedString.ToUpper());
            }
            else
            {
                newId.Append('_');
            }
        }

        return newId.ToString();
    }

    internal class Configuration : IEntityTypeConfiguration<Invitation>
    {
        public void Configure(EntityTypeBuilder<Invitation> builder)
        {
            builder.HasIndex(x => new { x.Id, x.ExpirationDate, x.UsagesLeft });
            builder.Property(x => x.UsagesLeft).IsConcurrencyToken();
        }
    }
}