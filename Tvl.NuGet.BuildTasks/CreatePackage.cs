namespace Tvl.NuGet.BuildTasks
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Path = System.IO.Path;

    public class CreatePackage : ToolTask
    {
        public CreatePackage()
        {
            GeneratedPackageFiles = new ITaskItem[0];
        }

        [Required]
        public string Configuration
        {
            get;
            set;
        }

        [Required]
        public string Platform
        {
            get;
            set;
        }

        [Required]
        public ITaskItem Manifest
        {
            get;
            set;
        }

        [Required]
        public string PackagesFolder
        {
            get;
            set;
        }

        public string OutputDirectory
        {
            get;
            set;
        }

        public string BasePath
        {
            get;
            set;
        }

        [Required]
        public bool Verbose
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }

        [Required]
        public bool Symbols
        {
            get;
            set;
        }

        [Required]
        public bool DefaultExcludes
        {
            get;
            set;
        }

        [Required]
        public bool PackageAnalysis
        {
            get;
            set;
        }

        [Required]
        public bool NonInteractive
        {
            get;
            set;
        }

        public string Verbosity
        {
            get;
            set;
        }

        public string Exclude
        {
            get;
            set;
        }

        public string MinClientVersion
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] GeneratedPackageFiles
        {
            get;
            set;
        }

        protected override string ToolName
        {
            get
            {
                return "NuGet Pack";
            }
        }

        protected override string GenerateFullPathToTool()
        {
            return Path.GetFullPath(Path.Combine(PackagesFolder, "NuGet.CommandLine.2.8.3", "tools", "NuGet.exe"));
        }

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();

            builder.AppendSwitch("pack");
            builder.AppendFileNameIfNotNull(Manifest);

            if (Verbose)
                builder.AppendSwitch("-Verbose");

            if (Symbols)
                builder.AppendSwitch("-Symbols");

            if (!DefaultExcludes)
                builder.AppendSwitch("-NoDefaultExcludes");

            if (!PackageAnalysis)
                builder.AppendSwitch("-NoPackageAnalysis");

            if (NonInteractive)
                builder.AppendSwitch("-NonInteractive");

            if (!string.IsNullOrEmpty(OutputDirectory))
                builder.AppendSwitchIfNotNull("-OutputDirectory ", OutputDirectory);

            if (!string.IsNullOrEmpty(BasePath))
                builder.AppendSwitchIfNotNull("-BasePath ", BasePath);

            if (!string.IsNullOrEmpty(Version))
                builder.AppendSwitchIfNotNull("-Version ", Version);

            if (!string.IsNullOrEmpty(Verbosity))
                builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);

            if (!string.IsNullOrEmpty(Exclude))
                builder.AppendSwitchIfNotNull("-Exclude ", Exclude);

            if (!string.IsNullOrEmpty(MinClientVersion))
                builder.AppendSwitchIfNotNull("-MinClientVersion ", MinClientVersion);

            builder.AppendSwitchIfNotNull("-Prop ", $"OutDir={OutputDirectory}");
            builder.AppendSwitchIfNotNull("-Prop ", $"Configuration={Configuration}");
            builder.AppendSwitchIfNotNull("-Prop ", $"Platform={Platform}");

            return builder.ToString();
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            base.LogEventsFromTextOutput(singleLine, messageImportance);

            var match = Regex.Match(singleLine, "^Successfully created package '(.*?)'");
            if (match.Success && !string.IsNullOrEmpty(match.Groups[1].Value))
            {
                var generatedPackages = GeneratedPackageFiles.ToList();
                generatedPackages.Add(new TaskItem(match.Groups[1].Value));
                GeneratedPackageFiles = generatedPackages.ToArray();
            }
        }
    }
}
