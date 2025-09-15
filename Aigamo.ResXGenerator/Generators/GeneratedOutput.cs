using Microsoft.CodeAnalysis;

namespace Aigamo.ResXGenerator.Generators;
public class GeneratedOutput(string fileName, string sourceCode, IEnumerable<Diagnostic> errorsAndWarnings)
{
	public GeneratedOutput(string fileName, string sourceCode) : this(fileName, sourceCode, [])
	{
		
	}

	public string FileName { get; internal set; } = fileName;
	public string SourceCode { get; internal set; } = sourceCode;
	public IEnumerable<Diagnostic> ErrorsAndWarnings { get; internal set; } = errorsAndWarnings;
}
