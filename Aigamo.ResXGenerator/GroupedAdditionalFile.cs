namespace Aigamo.ResXGenerator;

public readonly record struct GroupedAdditionalFile
{
    public AdditionalTextWithHash MainFile { get; }
    public IReadOnlyList<AdditionalTextWithHash> SubFiles { get; }

    public GroupedAdditionalFile(AdditionalTextWithHash mainFile, IReadOnlyList<AdditionalTextWithHash> subFiles)
    {
        MainFile = mainFile;
        SubFiles = subFiles.OrderBy(x => x.File.Path, StringComparer.Ordinal).ToArray();
    }

    public bool Equals(GroupedAdditionalFile other) => MainFile.Equals(other.MainFile) && SubFiles.SequenceEqual(other.SubFiles);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = MainFile.GetHashCode();
            return SubFiles.Aggregate(hashCode, (current, additionalText) => (current * 397) ^ additionalText.GetHashCode());
        }
    }

    public override string ToString() => $"{nameof(MainFile)}: {MainFile}, {nameof(SubFiles)}: {string.Join("; ", SubFiles ?? [])}";
}
