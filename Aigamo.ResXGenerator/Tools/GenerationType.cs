namespace Aigamo.ResXGenerator.Tools;

/// <summary>
/// Specifies the strategy used for generating or retrieving localized resources.
/// </summary>
/// <remarks>Use this enumeration to indicate how localization resources should be obtained or generated within an
/// application. The selected value determines whether resources are managed through a resource manager, generated as
/// code, accessed via a string localizer, or inherit the strategy from an outer context.</remarks>
public enum GenerationType
{
	/// <summary>
	/// Specifies that a resource manager should be used to handle localization resources.
	/// </summary>
	ResourceManager,
	/// <summary>
	/// Provides functionality to obtain resources programmatically.
	/// </summary>
	CodeGeneration,
	/// <summary>
	/// Specifies that a string localizer should be used to manage localization resources.
	/// </summary>
	StringLocalizer,
	/// <summary>
	/// Specifies that the generation type should be the same as the project option.
	/// </summary>
	SameAsOuter
}
