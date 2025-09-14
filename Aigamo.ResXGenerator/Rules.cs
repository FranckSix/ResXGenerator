using Microsoft.CodeAnalysis;

namespace Aigamo.ResXGenerator;
internal class Rules
{
    public static readonly DiagnosticDescriptor s_duplicateWarning = new(
        id: "AigamoResXGenerator001",
        title: "Duplicate member",
        messageFormat: "Ignored added member '{0}'",
        category: "ResXGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor s_memberSameAsClassWarning = new(
        id: "AigamoResXGenerator002",
        title: "Member same name as class",
        messageFormat: "Ignored member '{0}' has same name as class",
        category: "ResXGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor s_memberWithStaticError = new(
        id: "AigamoResXGenerator003",
        title: "Incompatible settings",
        messageFormat: "Cannot have static members/class with an class instance",
        category: "ResXGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
