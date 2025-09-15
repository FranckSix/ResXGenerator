using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Generators;
using Aigamo.ResXGenerator.Tools;
using Microsoft.CodeAnalysis;

namespace Aigamo.ResXGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	private IComboGenerator ComboGenerator { get; } = new ComboGenerator();
	private IResXGenerator ResXGenerator { get; } = new ResourceManagerGenerator();

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var globalOptions = context.AnalyzerConfigOptionsProvider.Select(GlobalOptions.Select);

		// Note: Each Resx file will get a hash (random guid) so we can easily differentiate in the pipeline when the file changed or just some options
		var allResxFiles = context.AdditionalTextsProvider.Where(static af => af.Path.EndsWith(".resx"))
			.Select(static (f, _) => new AdditionalTextWithHash(f, Guid.NewGuid()));

		var monitor = allResxFiles.Collect().SelectMany(static (x, _) => GroupResxFiles.Group(x));

		var inputs = monitor
			.Combine(globalOptions)
			.Combine(context.AnalyzerConfigOptionsProvider)
			.Select(static (x, _) => GenFileOptions.Select(
				file: x.Left.Left,
				options: x.Right,
				globalOptions: x.Left.Right
			))
			.Where(static x => x is { IsValid: true, SkipFile: false });

		GenerateResXFiles(context, inputs);
		GenerateResxCombos(context, monitor);
	}

	private void GenerateResxCombos(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GroupedAdditionalFile> monitor)
	{
		var detectAllCombosOfResx = monitor.Collect().SelectMany((x, _) => GroupResxFiles.DetectChildCombos(x));
		context.RegisterSourceOutput(detectAllCombosOfResx, (ctx, combo) =>
		{
			try
			{
				var output = ComboGenerator.Generate(combo, ctx.CancellationToken);
				output.ErrorsAndWarnings.ForEach(ctx.ReportDiagnostic);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Rules.FatalError(ComboGenerator.GeneratedFileName(combo), e), Location.None));
			}
		});
	}

	private void GenerateResXFiles(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GenFileOptions> inputs)
	{
		context.RegisterSourceOutput(inputs, (ctx, file) =>
		{
			try
			{
				var output = ResXGenerator.Generate(file, ctx.CancellationToken);
				output.ErrorsAndWarnings.ForEach(ctx.ReportDiagnostic);
				ctx.AddSource(output.FileName, output.SourceCode);
			}
			catch (Exception e)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(Rules.FatalError(file.ClassName, e), Location.None));
			}

		});
	}
}
