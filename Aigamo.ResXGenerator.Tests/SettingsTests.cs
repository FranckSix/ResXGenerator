using Aigamo.ResXGenerator.Tools;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Aigamo.ResXGenerator.Tests;

public class SettingsTests
{
	private static readonly GlobalOptions s_globalOptions = GlobalOptions.Select(
		provider: new AnalyzerConfigOptionsProviderStub(
			globalOptions: new AnalyzerConfigOptionsStub
			{
				RootNamespace = "namespace1",
				MSBuildProjectFullPath = "project1.csproj",
				MSBuildProjectName = "project1",
			},
			fileOptions: null!
		),
		token: default
	);

	[Fact]
	public void GlobalDefaults()
	{
		var globalOptions = s_globalOptions;
		_ = globalOptions.ProjectName.Should().Be("project1");
		_ = globalOptions.RootNamespace.Should().Be("namespace1");
		_ = globalOptions.ProjectFullPath.Should().Be("project1.csproj");
		_ = globalOptions.InnerClassName.Should().BeNullOrEmpty();
		_ = globalOptions.ClassNamePostfix.Should().BeNullOrEmpty();
		_ = globalOptions.InnerClassInstanceName.Should().BeNullOrEmpty();
		_ = globalOptions.InnerClassVisibility.Should().Be(InnerClassVisibility.NotGenerated);
		_ = globalOptions.NullForgivingOperators.Should().Be(false);
		_ = globalOptions.StaticClass.Should().Be(true);
		_ = globalOptions.StaticMembers.Should().Be(true);
		_ = globalOptions.PublicClass.Should().Be(false);
		_ = globalOptions.InternalRegistration.Should().Be(false);
		_ = globalOptions.PartialClass.Should().Be(false);
		_ = globalOptions.IsValid.Should().Be(true);
		_ = globalOptions.GenerateCode.Should().Be(false);
		_ = globalOptions.GenerationType.Should().Be(GenerationType.ResourceManager);
	}

	[Fact]
	public void GlobalSettings_CanReadAll()
	{
		var globalOptions = GlobalOptions.Select(
			provider: new AnalyzerConfigOptionsProviderStub(
				globalOptions: new AnalyzerConfigOptionsStub
				{
					RootNamespace = "namespace1",
					MSBuildProjectFullPath = "project1.csproj",
					MSBuildProjectName = "project1",
					ResXGenerator_InnerClassName = "test1",
					ResXGenerator_InnerClassInstanceName = "test2",
					ResXGenerator_ClassNamePostfix = "test3",
					ResXGenerator_InnerClassVisibility = "public",
					ResXGenerator_NullForgivingOperators = "true",
					ResXGenerator_StaticClass = "false",
					ResXGenerator_StaticMembers = "false",
					ResXGenerator_GenerateCode = "true",
					ResXGenerator_PublicClass = "true",
					ResXGenerator_InternalRegistration = "true",
					ResXGenerator_PartialClass = "true",
					ResXGenerator_GenerationType = "ResourceManager"
				},
				fileOptions: null!
			),
			TestContext.Current.CancellationToken
		);
		_ = globalOptions.RootNamespace.Should().Be("namespace1");
		_ = globalOptions.ProjectFullPath.Should().Be("project1.csproj");
		_ = globalOptions.ProjectName.Should().Be("project1");
		_ = globalOptions.InnerClassName.Should().Be("test1");
		_ = globalOptions.InnerClassInstanceName.Should().Be("test2");
		_ = globalOptions.ClassNamePostfix.Should().Be("test3");
		_ = globalOptions.InnerClassVisibility.Should().Be(InnerClassVisibility.Public);
		_ = globalOptions.NullForgivingOperators.Should().Be(true);
		_ = globalOptions.StaticClass.Should().Be(false);
		_ = globalOptions.GenerateCode.Should().Be(true);
		_ = globalOptions.StaticMembers.Should().Be(false);
		_ = globalOptions.PublicClass.Should().Be(true);
		_ = globalOptions.InternalRegistration.Should().Be(true);
		_ = globalOptions.PartialClass.Should().Be(true);
		_ = globalOptions.IsValid.Should().Be(true);
		_ = globalOptions.GenerationType.Should().Be(GenerationType.ResourceManager);
	}

	[Fact]
	public void FileDefaults()
	{
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub("Path1.resx"), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub()
			),
			globalOptions: s_globalOptions
		);
		_ = fileOptions.InnerClassName.Should().BeNullOrEmpty();
		_ = fileOptions.InnerClassInstanceName.Should().BeNullOrEmpty();
		_ = fileOptions.InnerClassVisibility.Should().Be(InnerClassVisibility.NotGenerated);
		_ = fileOptions.NullForgivingOperators.Should().Be(false);
		_ = fileOptions.StaticClass.Should().Be(true);
		_ = fileOptions.StaticMembers.Should().Be(true);
		_ = fileOptions.PublicClass.Should().Be(false);
		_ = fileOptions.InternalRegistration.Should().Be(false);
		_ = fileOptions.PartialClass.Should().Be(false);
		_ = fileOptions.GenerateCode.Should().Be(false);
		_ = fileOptions.LocalNamespace.Should().Be("namespace1");
		_ = fileOptions.CustomToolNamespace.Should().BeNullOrEmpty();
		_ = fileOptions.GroupedFile.MainFile.File.Path.Should().Be("Path1.resx");
		_ = fileOptions.ClassName.Should().Be("Path1");
		_ = fileOptions.SkipFile.Should().Be(false);
		_ = fileOptions.IsValid.Should().Be(true);
		_ = fileOptions.GenerationType.Should().Be(GenerationType.ResourceManager);
	}

	[Theory]
	[InlineData("project1.csproj", "Path1.resx", null, "project1", "project1.Path1")]
	[InlineData("project1.csproj", "Path1.resx", "", "project1", "project1.Path1")]
	[InlineData("project1.csproj", "Path1.resx", "rootNamespace", "rootNamespace", "rootNamespace.Path1")]
	[InlineData(@"ProjectFolder/project1.csproj", @"ProjectFolder/SubFolder/Path1.resx", "rootNamespace", "rootNamespace.SubFolder", "rootNamespace.SubFolder.Path1")]
	[InlineData(@"ProjectFolder/project1.csproj", @"ProjectFolder/SubFolder With Space/Path1.resx", "rootNamespace", "rootNamespace.SubFolder_With_Space", "rootNamespace.SubFolder_With_Space.Path1")]
	[InlineData(@"ProjectFolder/project1.csproj", @"ProjectFolder/SubFolder/Path1.resx", null, "SubFolder", "SubFolder.Path1")]
	[InlineData(@"ProjectFolder/8 project.csproj", @"ProjectFolder/Path1.resx", null, "_8_project", "_8_project.Path1")]
	[InlineData(@"ProjectFolder/8 project.csproj", @"ProjectFolder/Path1.resx", "", "_8_project", "_8_project.Path1")]
	[InlineData(@"ProjectFolder/8 project.csproj", @"ProjectFolder/SubFolder/Path1.resx", null, "SubFolder", "SubFolder.Path1")]
	[InlineData(@"ProjectFolder/8 project.csproj", @"ProjectFolder/SubFolder/Path1.resx", "", "SubFolder", "SubFolder.Path1")]
	public void FileSettings_RespectsEmptyRootNamespace(
		string msBuildProjectFullPath,
		string mainFile,
		string? rootNamespace,
		string expectedLocalNamespace,
		string expectedEmbeddedFilename
	)
	{
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(mainFile), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub()
			),
			globalOptions: GlobalOptions.Select(
				provider: new AnalyzerConfigOptionsProviderStub(
					globalOptions: new AnalyzerConfigOptionsStub
					{
						MSBuildProjectName = Path.GetFileNameWithoutExtension(msBuildProjectFullPath),
						RootNamespace = rootNamespace,
						MSBuildProjectFullPath = msBuildProjectFullPath
					},
					fileOptions: null!
				),
				TestContext.Current.CancellationToken
			)
		);
		_ = fileOptions.InnerClassName.Should().BeNullOrEmpty();
		_ = fileOptions.InnerClassInstanceName.Should().BeNullOrEmpty();
		_ = fileOptions.InnerClassVisibility.Should().Be(InnerClassVisibility.NotGenerated);
		_ = fileOptions.NullForgivingOperators.Should().Be(false);
		_ = fileOptions.StaticClass.Should().Be(true);
		_ = fileOptions.StaticMembers.Should().Be(true);
		_ = fileOptions.PublicClass.Should().Be(false);
		_ = fileOptions.PartialClass.Should().Be(false);
		_ = fileOptions.GenerateCode.Should().Be(false);
		_ = fileOptions.LocalNamespace.Should().Be(expectedLocalNamespace);
		_ = fileOptions.CustomToolNamespace.Should().BeNullOrEmpty();
		_ = fileOptions.GroupedFile.MainFile.File.Path.Should().Be(mainFile);
		_ = fileOptions.EmbeddedFilename.Should().Be(expectedEmbeddedFilename);
		_ = fileOptions.ClassName.Should().Be("Path1");
		_ = fileOptions.IsValid.Should().Be(true);
		_ = fileOptions.GenerationType.Should().Be(GenerationType.ResourceManager);
	}

	[Fact]
	public void File_PostFix()
	{
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub("Path1.resx"), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub { ClassNamePostfix = "test1" }
			),
			globalOptions: s_globalOptions
		);
		_ = fileOptions.ClassName.Should().Be("Path1test1");
		_ = fileOptions.IsValid.Should().Be(true);
	}

	[Fact]
	public void FileSettings_CanReadAll()
	{
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub("Path1.resx"), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub
				{
					RootNamespace = "namespace1",
					MSBuildProjectFullPath = "project1.csproj",
					CustomToolNamespace = "ns1",
					InnerClassName = "test1",
					InnerClassInstanceName = "test2",
					InnerClassVisibility = "public",
					NullForgivingOperators = "true",
					StaticClass = "false",
					StaticMembers = "false",
					PublicClass = "true",
					PartialClass = "true",
					GenerateCode = "true",
					GenerationType = "ResourceManager"
				}
			),
			globalOptions: s_globalOptions
		);
		_ = fileOptions.InnerClassName.Should().Be("test1");
		_ = fileOptions.InnerClassInstanceName.Should().Be("test2");
		_ = fileOptions.InnerClassVisibility.Should().Be(InnerClassVisibility.Public);
		_ = fileOptions.NullForgivingOperators.Should().Be(false);
		_ = fileOptions.StaticClass.Should().Be(false);
		_ = fileOptions.StaticMembers.Should().Be(false);
		_ = fileOptions.PublicClass.Should().Be(true);
		_ = fileOptions.PartialClass.Should().Be(true);
		_ = fileOptions.IsValid.Should().Be(true);
		_ = fileOptions.GenerateCode.Should().Be(true);
		_ = fileOptions.LocalNamespace.Should().Be("namespace1");
		_ = fileOptions.CustomToolNamespace.Should().Be("ns1");
		_ = fileOptions.GroupedFile.MainFile.File.Path.Should().Be("Path1.resx");
		_ = fileOptions.ClassName.Should().Be("Path1");
		_ = fileOptions.GenerationType.Should().Be(GenerationType.ResourceManager);
	}

	[Fact]
	public void FileSettings_RespectsGlobalDefaults()
	{
		var globalOptions = GlobalOptions.Select(
			provider: new AnalyzerConfigOptionsProviderStub(
				globalOptions: new AnalyzerConfigOptionsStub
				{
					RootNamespace = "namespace1",
					MSBuildProjectFullPath = "project1.csproj",
					MSBuildProjectName = "project1",
					ResXGenerator_InnerClassName = "test1",
					ResXGenerator_InnerClassInstanceName = "test2",
					ResXGenerator_ClassNamePostfix = "test3",
					ResXGenerator_InnerClassVisibility = "public",
					ResXGenerator_NullForgivingOperators = "true",
					ResXGenerator_StaticClass = "false",
					ResXGenerator_StaticMembers = "false",
					ResXGenerator_PublicClass = "true",
					ResXGenerator_InternalRegistration = "true",
					ResXGenerator_PartialClass = "true",
				},
				fileOptions: null!
			),
			TestContext.Current.CancellationToken
		);
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub("Path1.resx"), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub()
			),
			globalOptions: globalOptions
		);
		_ = fileOptions.InnerClassName.Should().Be("test1");
		_ = fileOptions.InnerClassInstanceName.Should().Be("test2");
		_ = fileOptions.InnerClassVisibility.Should().Be(InnerClassVisibility.Public);
		_ = fileOptions.NullForgivingOperators.Should().Be(true);
		_ = fileOptions.StaticClass.Should().Be(false);
		_ = fileOptions.StaticMembers.Should().Be(false);
		_ = fileOptions.PublicClass.Should().Be(true);
		_ = fileOptions.InternalRegistration.Should().Be(true);
		_ = fileOptions.PartialClass.Should().Be(true);
		_ = fileOptions.GenerateCode.Should().Be(false);
		_ = fileOptions.LocalNamespace.Should().Be("namespace1");
		_ = fileOptions.CustomToolNamespace.Should().BeNullOrEmpty();
		_ = fileOptions.GroupedFile.MainFile.File.Path.Should().Be("Path1.resx");
		_ = fileOptions.ClassName.Should().Be("Path1test3");
		_ = fileOptions.IsValid.Should().Be(true);
	}

	[Fact]
	public void FileSettings_CanSkipIndividualFile()
	{
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub("Path1.resx"), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub
				{
					RootNamespace = "namespace1",
					MSBuildProjectFullPath = "project1.csproj",
					SkipFile = "true",
				}
			),
			globalOptions: s_globalOptions
		);

		_ = fileOptions.LocalNamespace.Should().Be("namespace1");
		_ = fileOptions.SkipFile.Should().Be(true);
		_ = fileOptions.IsValid.Should().Be(true);
	}

	[Fact]
	public void FileSettings_CanSwitchGenerationType()
	{
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub("Path1.resx"), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub
				{
					RootNamespace = "namespace1",
					MSBuildProjectFullPath = "project1.csproj",
					GenerationType = "StringLocalizer",
				}
			),
			globalOptions: s_globalOptions
		);
		_ = fileOptions.LocalNamespace.Should().Be("namespace1");
		_ = fileOptions.GenerationType.Should().Be(GenerationType.StringLocalizer);
		_ = fileOptions.IsValid.Should().Be(true);
	}

	[Fact]
	public void GlobalOptions_CanSwitchGenerationType()
	{
		var globalOptions = GlobalOptions.Select(
			 provider: new AnalyzerConfigOptionsProviderStub(
				 globalOptions: new AnalyzerConfigOptionsStub
				 {
					 RootNamespace = "namespace1",
					 MSBuildProjectFullPath = "project1.csproj",
					 MSBuildProjectName = "project1",
					 ResXGenerator_GenerationType = "StringLocalizer"
				 },
				 fileOptions: null!
			 ),
			 TestContext.Current.CancellationToken
		 );

		_ = globalOptions.RootNamespace.Should().Be("namespace1");
		_ = globalOptions.GenerationType.Should().Be(GenerationType.StringLocalizer);
		_ = globalOptions.IsValid.Should().Be(true);
	}

	[Fact]
	public void GlobalOptions_StringLocalizerMustIgnoreStaticOptions()
	{
		var globalOptions = GlobalOptions.Select(
			 provider: new AnalyzerConfigOptionsProviderStub(
				 globalOptions: new AnalyzerConfigOptionsStub
				 {
					 RootNamespace = "namespace1",
					 MSBuildProjectFullPath = "project1.csproj",
					 MSBuildProjectName = "project1",
					 ResXGenerator_GenerationType = "StringLocalizer",
					 ResXGenerator_StaticClass = "true",
					 ResXGenerator_StaticMembers = "true"
				 },
				 fileOptions: null!
			 ),
			 TestContext.Current.CancellationToken
		 );

		_ = globalOptions.RootNamespace.Should().Be("namespace1");
		_ = globalOptions.GenerationType.Should().Be(GenerationType.StringLocalizer);
		_ = globalOptions.IsValid.Should().Be(true);
		_ = globalOptions.StaticClass.Should().Be(false);
		_ = globalOptions.StaticMembers.Should().Be(false);
	}

	[Fact]
	public void TargetPath_IsUsedForLocalNamespace()
	{
		var fileOptions = GenFileOptions.Select(
			file: new GroupedAdditionalFile(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub("Other/Strings.resx"), Guid.NewGuid()),
				subFiles: []
			),
			options: new AnalyzerConfigOptionsProviderStub(
				globalOptions: null!,
				fileOptions: new AnalyzerConfigOptionsStub
				{
					TargetPath = "Resources\\Strings.resx"
				}
			),
			globalOptions: s_globalOptions
		);

		_ = fileOptions.LocalNamespace.Should().Be("namespace1.Resources");
		_ = fileOptions.ClassName.Should().Be("Strings");
		_ = fileOptions.IsValid.Should().Be(true);
	}

	private class AnalyzerConfigOptionsStub : AnalyzerConfigOptions
	{
		// ReSharper disable InconsistentNaming
		public string? MSBuildProjectFullPath { get; init; }
		// ReSharper disable InconsistentNaming
		public string? MSBuildProjectName { get; init; }
		public string? RootNamespace { get; init; }
		public string? ResXGenerator_ClassNamePostfix { get; init; }
		public string? ResXGenerator_PublicClass { get; init; }
		public string? ResXGenerator_InternalRegistration { get; init; }
		public string? ResXGenerator_NullForgivingOperators { get; init; }
		public string? ResXGenerator_StaticClass { get; init; }
		public string? ResXGenerator_StaticMembers { get; init; }
		public string? ResXGenerator_PartialClass { get; init; }
		public string? ResXGenerator_InnerClassVisibility { get; init; }
		public string? ResXGenerator_InnerClassName { get; init; }
		public string? ResXGenerator_InnerClassInstanceName { get; init; }
		public string? ResXGenerator_GenerateCode { get; init; }
		public string? ResXGenerator_GenerationType { get; init; }
		public string? CustomToolNamespace { get; init; }
		public string? TargetPath { get; init; }
		public string? ClassNamePostfix { get; init; }
		public string? PublicClass { get; init; }
		public string? NullForgivingOperators { get; init; }
		public string? StaticClass { get; init; }
		public string? StaticMembers { get; init; }
		public string? PartialClass { get; init; }
		public string? InnerClassVisibility { get; init; }
		public string? InnerClassName { get; init; }
		public string? InnerClassInstanceName { get; init; }
		public string? GenerateCode { get; init; }
		public string? GenerationType { get; init; }
		public string? SkipFile { get; init; }

		// ReSharper restore InconsistentNaming

		public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
		{
			string? GetVal()
			{
				return key switch
				{
					"build_property.MSBuildProjectFullPath" => MSBuildProjectFullPath,
					"build_property.MSBuildProjectName" => MSBuildProjectName,
					"build_property.RootNamespace" => RootNamespace,
					"build_property.ResXGenerator_GenerateCode" => ResXGenerator_GenerateCode,
					"build_property.ResXGenerator_ClassNamePostfix" => ResXGenerator_ClassNamePostfix,
					"build_property.ResXGenerator_PublicClass" => ResXGenerator_PublicClass,
					"build_property.ResXGenerator_InternalRegistration" => ResXGenerator_InternalRegistration,
					"build_property.ResXGenerator_NullForgivingOperators" => ResXGenerator_NullForgivingOperators,
					"build_property.ResXGenerator_StaticClass" => ResXGenerator_StaticClass,
					"build_property.ResXGenerator_StaticMembers" => ResXGenerator_StaticMembers,
					"build_property.ResXGenerator_PartialClass" => ResXGenerator_PartialClass,
					"build_property.ResXGenerator_InnerClassVisibility" => ResXGenerator_InnerClassVisibility,
					"build_property.ResXGenerator_InnerClassName" => ResXGenerator_InnerClassName,
					"build_property.ResXGenerator_InnerClassInstanceName" => ResXGenerator_InnerClassInstanceName,
					"build_property.ResXGenerator_GenerationType" => ResXGenerator_GenerationType,
					"build_metadata.EmbeddedResource.CustomToolNamespace" => CustomToolNamespace,
					"build_metadata.EmbeddedResource.TargetPath" => TargetPath,
					"build_metadata.EmbeddedResource.ClassNamePostfix" => ClassNamePostfix,
					"build_metadata.EmbeddedResource.PublicClass" => PublicClass,
					"build_metadata.EmbeddedResource.NullForgivingOperators" => NullForgivingOperators,
					"build_metadata.EmbeddedResource.StaticClass" => StaticClass,
					"build_metadata.EmbeddedResource.StaticMembers" => StaticMembers,
					"build_metadata.EmbeddedResource.PartialClass" => PartialClass,
					"build_metadata.EmbeddedResource.InnerClassVisibility" => InnerClassVisibility,
					"build_metadata.EmbeddedResource.InnerClassName" => InnerClassName,
					"build_metadata.EmbeddedResource.InnerClassInstanceName" => InnerClassInstanceName,
					"build_metadata.EmbeddedResource.GenerateCode" => GenerateCode,
					"build_metadata.EmbeddedResource.SkipFile" => SkipFile,
					"build_metadata.EmbeddedResource.GenerationType" => GenerationType,
					_ => null
				};
			}

			value = GetVal();
			return value is not null;
		}
	}

	private class AnalyzerConfigOptionsProviderStub(AnalyzerConfigOptions globalOptions, AnalyzerConfigOptions fileOptions) : AnalyzerConfigOptionsProvider
	{
		private readonly AnalyzerConfigOptions _fileOptions = fileOptions;

		public override AnalyzerConfigOptions GlobalOptions { get; } = globalOptions;

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => throw new NotImplementedException();

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _fileOptions;
	}
}
