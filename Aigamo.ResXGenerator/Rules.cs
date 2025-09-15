using Microsoft.CodeAnalysis;

namespace Aigamo.ResXGenerator;

internal static class Rules
{
	public static readonly DiagnosticDescriptor DuplicateWarning = new(
		id: "AigamoResXGenerator001",
		title: "Duplicate member",
		messageFormat: "Ignored added member '{0}'",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor MemberSameAsClassWarning = new(
		id: "AigamoResXGenerator002",
		title: "Member same name as class",
		messageFormat: "Ignored member '{0}' has same name as class",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor MemberWithStaticError = new(
		id: "AigamoResXGenerator003",
		title: "Incompatible settings",
		messageFormat: "Cannot have static members/class with an class instance",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static DiagnosticDescriptor FatalError(string resourceDetail, Exception exception) => new(
		id: "AigamoResXGenerator999",
		title: "Fatal Error generated",
		messageFormat: $"An error occured on generation file {resourceDetail} error {exception.Message}",
		category: "ResXGenerator",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);
}
