namespace Tvl.NuGet.BuildTasks
{
    using System.Collections.Generic;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Path = System.IO.Path;

    public class FindPackageInputs : Task
    {
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
        public ITaskItem[] Manifests
        {
            get;
            set;
        }

        [Required]
        public string OutputDirectory
        {
            get;
            set;
        }

        [Required]
        public string BasePath
        {
            get;
            set;
        }

        [Required]
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

        [Output]
        public ITaskItem[] PackageInputs
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] PackageOutputs
        {
            get;
            set;
        }

        public override bool Execute()
        {
            // Identify the package inputs
            PackageInputs = new ITaskItem[0];

            // Identify the package outputs
            List<ITaskItem> outputs = new List<ITaskItem>();
            foreach (var manifest in Manifests)
            {
                // TODO: use the actual <id> value instead of the file name.
                string packageId = Path.GetFileNameWithoutExtension(manifest.ItemSpec);
                outputs.Add(new TaskItem(Path.Combine(OutputDirectory, $"{packageId}.{Version}.nupkg")));
                if (Symbols)
                    outputs.Add(new TaskItem(Path.Combine(OutputDirectory, $"{packageId}.{Version}.symbols.nupkg")));
            }

            PackageOutputs = outputs.ToArray();

            return true;
        }
    }
}
