using System.Globalization;
using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Tools;

namespace Aigamo.ResXGenerator.Generators;

public sealed class ComboGenerator : StringBuilderGenerator<CultureInfoCombo>, IComboGenerator
{
	private const string OutputStringFilenameFormat = "Aigamo.ResXGenerator.{0}.g.cs";
	private static readonly Dictionary<int, List<int>> s_allChildren = new();

	/// <summary>
	/// Build all CultureInfo children
	/// </summary>
	static ComboGenerator()
	{
		var all = CultureInfo.GetCultures(CultureTypes.AllCultures);

		all.ForEach(cultureInfo =>
		{
			if (cultureInfo.LCID == 4096 || cultureInfo.IsNeutralCulture || cultureInfo.Name.IsNullOrEmpty())
				return;

			var parent = cultureInfo.Parent;
			if (!s_allChildren.TryGetValue(parent.LCID, out var v))
				s_allChildren[parent.LCID] = v = [];
			v.Add(cultureInfo.LCID);
		});
	}

	public override GeneratedOutput Generate(CultureInfoCombo options, CancellationToken cancellationToken = default)
	{
		var definedLanguages = options.GetDefinedLanguages();
		var builder = GetBuilder("Aigamo.ResXGenerator");

		builder.AppendLine("internal static partial class Helpers");
		builder.AppendLine("{");

		builder.Append("    public static string GetString_");
		var functionNamePostFix = FunctionNamePostFix(definedLanguages);
		builder.Append(functionNamePostFix);
		builder.Append("(string fallback");
		definedLanguages.ForEach((name, _, _) =>
		{
			builder.Append(", ");
			builder.Append("string ");
			builder.Append(name);
		});

		builder.Append(") => ");
		builder.Append(Constants.SystemGlobalization);
		builder.AppendLine(".CultureInfo.CurrentUICulture.LCID switch");
		builder.AppendLine("    {");
		var already = new HashSet<int>();
		definedLanguages.ForEach((name, lcid, _) =>
		{
			var findParents = FindParents(lcid).Except(already).ToList();
			findParents
				.Select(parent =>
				{
					already.Add(parent);
					return $"        {parent} => {name.Replace('-', '_')},";
				})
				.ForEach(l => builder.AppendLine(l));
		});

		builder.AppendLine("        _ => fallback");
		builder.AppendLine("    };");
		builder.AppendLine("}");

		return new GeneratedOutput(string.Format(OutputStringFilenameFormat, functionNamePostFix), builder.ToString());
	}

	public string GeneratedFileName(CultureInfoCombo combo)
	{
		var definedLanguages = combo.GetDefinedLanguages();
		var functionNamePostFix = FunctionNamePostFix(definedLanguages);
		return string.Format(OutputStringFilenameFormat, functionNamePostFix) ;
	}

	private static IEnumerable<int> FindParents(int toFind) => s_allChildren.TryGetValue(toFind, out var v) ? v.Prepend(toFind) : [toFind];
}
