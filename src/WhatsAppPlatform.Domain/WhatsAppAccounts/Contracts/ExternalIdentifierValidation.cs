using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

internal static class ExternalIdentifierValidation
{
    public static bool IsValid([NotNullWhen(true)] string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 200 || value.Any(char.IsControl))
            return false;
        var characterCount = 0;
        for (var index = 0; index < value.Length; index++)
        {
            if (!Rune.TryGetRuneAt(value, index, out var rune) || ++characterCount > 100)
                return false;
            index += rune.Utf16SequenceLength - 1;
        }
        return true;
    }
}
