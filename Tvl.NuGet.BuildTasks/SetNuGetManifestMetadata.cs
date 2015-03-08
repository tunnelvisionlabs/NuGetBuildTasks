namespace Tvl.NuGet.BuildTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using AssemblyFileVersionAttribute = System.Reflection.AssemblyFileVersionAttribute;
    using AssemblyInformationalVersionAttribute = System.Reflection.AssemblyInformationalVersionAttribute;
    using File = System.IO.File;

    public class SetNuGetManifestMetadata : Task
    {
        [Required]
        public ITaskItem AssemblyFile
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

        [Output]
        public ITaskItem[] OutputItems
        {
            get;
            set;
        }

        public override bool Execute()
        {
            OutputItems = new ITaskItem[0];

            if (Manifests.All(i => !string.IsNullOrEmpty(i.GetMetadata("Version"))))
                return true;

            if (!File.Exists(AssemblyFile.ItemSpec))
                return false;

            using (PEReader targetExecutable = new PEReader(File.OpenRead(AssemblyFile.ItemSpec), PEStreamOptions.PrefetchMetadata))
            {
                string informationalVersion = null;
                string fileVersion = null;

                MetadataReader reader = targetExecutable.GetMetadataReader();
                foreach (CustomAttributeHandle customAttributeHandle in reader.GetAssemblyDefinition().GetCustomAttributes())
                {
                    CustomAttribute customAttribute = reader.GetCustomAttribute(customAttributeHandle);
                    if (customAttribute.Constructor.Kind != HandleKind.MemberReference)
                        continue;

                    MemberReference constructorMemberReference = reader.GetMemberReference((MemberReferenceHandle)customAttribute.Constructor);
                    if (constructorMemberReference.Parent.Kind != HandleKind.TypeReference)
                        continue;

                    TypeReference attributeTypeReference = reader.GetTypeReference((TypeReferenceHandle)constructorMemberReference.Parent);
                    if (reader.StringComparer.Equals(attributeTypeReference.Name, nameof(AssemblyInformationalVersionAttribute))
                        && reader.StringComparer.Equals(attributeTypeReference.Namespace, typeof(AssemblyInformationalVersionAttribute).Namespace))
                    {
                        BlobReader blobReader = reader.GetBlobReader(customAttribute.Value);
                        var prolog = blobReader.ReadUInt16();
                        // the version is the first and only fixed arg
                        informationalVersion = blobReader.ReadSerializedString();

                        if (!string.IsNullOrEmpty(informationalVersion))
                            break;
                    }
                    else if (reader.StringComparer.Equals(attributeTypeReference.Name, nameof(AssemblyFileVersionAttribute))
                        && reader.StringComparer.Equals(attributeTypeReference.Namespace, typeof(AssemblyFileVersionAttribute).Namespace))
                    {
                        BlobReader blobReader = reader.GetBlobReader(customAttribute.Value);
                        var prolog = blobReader.ReadUInt16();
                        // the version is the first and only fixed arg
                        fileVersion = blobReader.ReadSerializedString();
                    }
                }

                string version;
                if (!string.IsNullOrEmpty(informationalVersion))
                    version = informationalVersion;
                else if (!string.IsNullOrEmpty(fileVersion))
                    version = fileVersion;
                else
                    version = reader.GetAssemblyDefinition().Version.ToString();

                List<ITaskItem> updatedItems = new List<ITaskItem>();
                foreach (var manifest in Manifests)
                {
                    if (!string.IsNullOrEmpty(manifest.GetMetadata("Version")))
                        continue;

                    manifest.SetMetadata("Version", version);
                    updatedItems.Add(manifest);
                }

                OutputItems = updatedItems.ToArray();
            }

            return true;
        }
    }
}
