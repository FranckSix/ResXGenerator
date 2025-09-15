using System.Collections.Immutable;
using System.Text;
using System.Xml;
using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Tools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Aigamo.ResXGenerator.Generators;

public sealed class ResourceManagerGenerator : StringBuilderGenerator<GenFileOptions>, IResXGenerator
{
	public override GeneratedOutput Generate(GenFileOptions options, CancellationToken cancellationToken = default)
	{
		var errorsAndWarnings = new List<Diagnostic>();
		var generatedFileName = $"{options.LocalNamespace}.{options.ClassName}.g.cs";

		var content = options.GroupedFile.MainFile.File.GetText(cancellationToken);
		if (content is null) return new GeneratedOutput (options.GroupedFile.MainFile.File.Path, "//ERROR reading file:", errorsAndWarnings);

		// HACK: netstandard2.0 doesn't support improved interpolated strings?
		var builder = GetBuilder(options.CustomToolNamespace ?? options.LocalNamespace);

		if (options.GenerateCode)
			AppendCodeUsings(builder);
		else
			AppendResourceManagerUsings(builder);

		builder.Append(options.PublicClass ? "public" : "internal");
		builder.Append(options.StaticClass ? " static" : string.Empty);
		builder.Append(options.PartialClass ? " partial class " : " class ");
		builder.AppendLine(options.ClassName);
		builder.AppendLine("{");

		var indent = "    ";
		string containerClassName = options.ClassName;

		if (options.InnerClassVisibility != InnerClassVisibility.NotGenerated)
		{
			containerClassName = string.IsNullOrEmpty(options.InnerClassName) ? "Resources" : options.InnerClassName;
			if (!string.IsNullOrEmpty(options.InnerClassInstanceName))
			{
				if (options.StaticClass || options.StaticMembers)
				{
					errorsAndWarnings.Add(Diagnostic.Create(
						descriptor: Rules.MemberWithStaticError,
						location: Location.Create(
							filePath: options.GroupedFile.MainFile.File.Path,
							textSpan: new TextSpan(),
							lineSpan: new LinePositionSpan()
						)
					));
				}

				builder.Append(indent);
				builder.Append("public ");
				builder.Append(containerClassName);
				builder.Append(" ");
				builder.Append(options.InnerClassInstanceName);
				builder.AppendLine(" { get; } = new();");
				builder.AppendLine();
			}

			builder.Append(indent);
			builder.Append(GetInnerClassVisibility(options));
			builder.Append(options.StaticClass ? " static" : string.Empty);
			builder.Append(options.PartialClass ? " partial class " : " class ");

			builder.AppendLine(containerClassName);
			builder.Append(indent);
			builder.AppendLine("{");

			indent += "    ";
		}

		if (options.GenerateCode)
			GenerateCode(options, content, indent, containerClassName, builder, errorsAndWarnings, cancellationToken);
		else
			GenerateResourceManager(options, content, indent, containerClassName, builder, errorsAndWarnings, cancellationToken);

		if (options.InnerClassVisibility != InnerClassVisibility.NotGenerated)
		{
			builder.AppendLine("    }");
		}

		builder.AppendLine("}");

		return new GeneratedOutput(generatedFileName, builder.ToString(), errorsAndWarnings);
	}

	private static void GenerateCode(
		GenFileOptions options,
		SourceText content,
		string indent,
		string containerClassName,
		StringBuilder builder,
		List<Diagnostic> errorsAndWarnings,
		CancellationToken cancellationToken)
	{
		var combo = new CultureInfoCombo(options.GroupedFile.SubFiles);
		var definedLanguages = combo.GetDefinedLanguages();

		var fallback = ReadResxFile(content);
		var subfiles = definedLanguages.Select(lang =>
		{
			var subcontent = lang.FileWithHash.File.GetText(cancellationToken);
			return subcontent is null
				? null
				: ReadResxFile(subcontent)?
					.GroupBy(x => x.key)
					.ToImmutableDictionary(x => x.Key, x => x.First().value);
		}).ToList();
		if (fallback is null || subfiles.Any(x => x is null))
		{
			builder.AppendFormat("//could not read {0} or one of its children", options.GroupedFile.MainFile.File.Path);
			return;
		}

		var alreadyAddedMembers = new HashSet<string>();
		fallback.ForEach((key, value, line) =>
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (
				!GenerateMember(
					indent,
					builder,
					options,
					key,
					value,
					line,
					alreadyAddedMembers,
					errorsAndWarnings,
					containerClassName,
					out _
				)
			) return;

			builder.Append(" => GetString_");
			builder.Append(FunctionNamePostFix(definedLanguages));
			builder.Append("(");
			builder.Append(SymbolDisplay.FormatLiteral(value, true));

			subfiles.ForEach(xml =>
			{
				builder.Append(", ");
				if (!xml!.TryGetValue(key, out var langValue))
					langValue = value;
				builder.Append(SymbolDisplay.FormatLiteral(langValue, true));
			});

			builder.AppendLine(");");
		});
	}

	private static void GenerateResourceManager(
		GenFileOptions options,
		SourceText content,
		string indent,
		string containerClassName,
		StringBuilder builder,
		List<Diagnostic> errorsAndWarnings,
		CancellationToken cancellationToken)
	{
		GenerateResourceManagerMembers(builder, indent, containerClassName, options);

		var members = ReadResxFile(content);
		if (members is null)
		{
			return;
		}

		var alreadyAddedMembers = new HashSet<string> { Constants.CultureInfoVariable };
		members.ForEach((key, value, line) =>
		{
			cancellationToken.ThrowIfCancellationRequested();
			CreateMember(
				indent,
				builder,
				options,
				key,
				value,
				line,
				alreadyAddedMembers,
				errorsAndWarnings,
				containerClassName
			);
		});
	}

	private static void CreateMember(
		string indent,
		StringBuilder builder,
		GenFileOptions options,
		string name,
		string value,
		IXmlLineInfo line,
		HashSet<string> alreadyAddedMembers,
		List<Diagnostic> errorsAndWarnings,
		string containerClassName
	)
	{
		if (!GenerateMember(indent, builder, options, name, value, line, alreadyAddedMembers, errorsAndWarnings, containerClassName, out var resourceAccessByName))
		{
			return;
		}

		if (resourceAccessByName)
		{
			builder.Append(" => ResourceManager.GetString(nameof(");
			builder.Append(name);
			builder.Append("), ");
		}
		else
		{
			builder.Append(@" => ResourceManager.GetString(""");
			builder.Append(name.Replace(@"""", @"\"""));
			builder.Append(@""", ");
		}

		builder.Append(Constants.CultureInfoVariable);
		builder.Append(")");
		builder.Append(options.NullForgivingOperators ? "!" : null);
		builder.AppendLine(";");
	}

	private static string GetInnerClassVisibility(GenFileOptions options)
	{
		if (options.InnerClassVisibility == InnerClassVisibility.SameAsOuter)
			return options.PublicClass ? "public" : "internal";

		return options.InnerClassVisibility.ToString().ToLowerInvariant();
	}
}
