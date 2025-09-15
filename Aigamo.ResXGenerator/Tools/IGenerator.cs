using Aigamo.ResXGenerator.Generators;

namespace Aigamo.ResXGenerator.Tools;

public interface IGenerator<in T>
{
	GeneratedOutput Generate(T options, CancellationToken cancellationToken = default);
}

public interface IResXGenerator : IGenerator<GenFileOptions>;
public interface IComboGenerator : IGenerator<CultureInfoCombo>
{
	string GeneratedFileName(CultureInfoCombo combo);
}
