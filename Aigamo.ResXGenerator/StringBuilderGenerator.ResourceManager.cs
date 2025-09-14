using System.Globalization;
using System.Resources;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Aigamo.ResXGenerator;

public sealed partial class StringBuilderGenerator
{
    private static void GenerateResourceManager(
        FileOptions options,
        SourceText content,
        StringBuilder builder,
        List<Diagnostic> errorsAndWarnings,
        CancellationToken cancellationToken
    )
    {
        GenerateResourceManagerMembers(builder, options);

        var members = ReadResxFile(content);
        if (members is null)
        {
            return;
        }

        foreach (var (key, value, line) in members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreateMember(
                builder,
                options,
                key,
                value,
                line,
                errorsAndWarnings
            );
        }
    }

    private static void CreateMember(
        StringBuilder builder,
        FileOptions options,
        string name,
        string value,
        IXmlLineInfo line,
        List<Diagnostic> errorsAndWarnings
    )
    {
        if (!GenerateMember(builder, options, name, value, line, errorsAndWarnings, out var resourceAccessByName))
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

        builder.Append("CultureInfo");
        builder.Append(")");
        builder.Append(options.NullForgivingOperators ? "!" : null);
        builder.AppendLine(";");
    }

    private static void GenerateResourceManagerMembers(
        StringBuilder builder,
        FileOptions options
    )
    {
        builder.Append("private static ");
        builder.Append(nameof(ResourceManager));
        builder.Append("? ");
        builder.Append("ResourceManager");
        builder.AppendLine(";");

        builder.Append("public static ");
        builder.Append(nameof(ResourceManager));
        builder.Append(" ");
        builder.Append("ResourceManager");
        builder.Append(" => ");
        builder.Append("ResourceManager");
        builder.Append(" ??= new ");
        builder.Append(nameof(ResourceManager));
        builder.Append("(\"");
        builder.Append(options.EmbeddedFilename);
        builder.Append("\", typeof(");
        builder.Append(options.ClassName);
        builder.AppendLine(").Assembly);");

        builder.Append("public ");
        builder.Append(options.StaticMembers ? "static " : string.Empty);
        builder.Append(nameof(CultureInfo));
        builder.Append("? ");
        builder.Append("ResourceManager");
        builder.AppendLine(" { get; set; }");
    }
}
