namespace Tvl.NuGet.BuildTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

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

        [Output]
        public ITaskItem[] PackageInputs
        {
            get;
            set;
        }

        public override bool Execute()
        {
            PackageInputs = new ITaskItem[0];
            return true;
        }
    }
}
