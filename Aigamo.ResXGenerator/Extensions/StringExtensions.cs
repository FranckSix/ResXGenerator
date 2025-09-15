using System.Web;
using Aigamo.ResXGenerator.Tools;

namespace Aigamo.ResXGenerator.Extensions;

internal static class StringExtensions
{
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

	public static string ToXmlCommentSafe(this string input, string indent)
	{
		var lines = HttpUtility.HtmlEncode(input.Trim())?.Split(["\n\r", "\r", "\n"], StringSplitOptions.None) ?? [];
		return string.Join($"{Environment.NewLine}{indent}/// ", lines);
	}
}
