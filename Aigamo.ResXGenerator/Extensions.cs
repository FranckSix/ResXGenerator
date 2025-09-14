using System.Web;
using Microsoft.CodeAnalysis.CSharp;

namespace Aigamo.ResXGenerator;

internal static class Extensions
{
    public static string TrimAndNormalize(this string input) => HttpUtility.HtmlEncode(input.Trim().Replace("\r\n", "\n").Replace("\r", "\n"));
    public static string ToXmlComment(this string input) => input.Replace("\n", Environment.NewLine + "/// ");
    public static string InterpolateCondition(this bool condition, string ifTrue, string ifFalse) => condition ? ifTrue : ifFalse;
    public static string IndentCode(this string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        return syntaxTree.GetRoot().ToFullString();
    }

    public static void ForEach<T>(this IEnumerable<T> col, Action<T> action)
    {
        foreach (var item in col) action(item);
    }
}
