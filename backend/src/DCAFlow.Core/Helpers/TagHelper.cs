using SideEffect.Extensions;
using System.Text.RegularExpressions;

namespace DCAFlow.Core.Helpers;

internal static partial class TagHelper
{
    [GeneratedRegex(@"[^A-Z0-9-]")]
    private static partial Regex AllowedCharsReplaceRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex SpaceCleanUpRegex();

    [GeneratedRegex(@"^[A-Z0-9]+(?:-[A-Z0-9]+)*$")]
    private static partial Regex ValidTagRegex();

    public static List<string> NormalizeTags(IEnumerable<string> tags)
    {
        if (tags.IsEmpty())
        {
            return [];
        }

        return [.. tags
            .Select(NormalizeTag)
            .Where(tag => !tag.IsEmpty())
            .Distinct()];
    }

    public static string NormalizeTag(string tag)
    {
        if (tag.IsEmpty())
        {
            return null;
        }

        tag = tag.Trim().Trim('-');

        tag = tag.ToUpperInvariant();

        tag = SpaceCleanUpRegex().Replace(tag, "-");

        tag = AllowedCharsReplaceRegex().Replace(tag, "");

        return tag;
    }

    public static bool IsValidTag(string tag)
    {
        if (tag.IsEmpty())
        { 
            return false;
        }

        return ValidTagRegex().IsMatch(tag);
    }
}
