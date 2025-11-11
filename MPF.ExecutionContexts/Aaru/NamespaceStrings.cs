namespace MPF.ExecutionContexts.Aaru
{
    /// <summary>
    /// Supported namespaces for Aaru
    /// </summary>
    /// TODO: Use to verify namespace settings
    public static class NamespaceStrings
    {
        // Namespaces for Apple Lisa File System
        public const string LisaOfficeSystem = "office";
        public const string LisaPascalWorkshop = "workshop"; // Default

        // Namespaces for ISO9660 Filesystem
        public const string JolietVolumeDescriptor = "joliet"; // Default
        public const string PrimaryVolumeDescriptor = "normal";
        public const string PrimaryVolumeDescriptorwithEncoding = "romeo";
        public const string RockRidge = "rrip";
        public const string PrimaryVolumeDescriptorVersionSuffix = "vms";

        // Namespaces for Microsoft File Allocation Table
        public const string DOS83UpperCase = "dos";
        public const string LFNWhenAvailableWithFallback = "ecs"; // Default
        public const string LongFileNames = "lfn";
        public const string WindowsNT83MixedCase = "nt";
        public const string OS2Extended = "os2";
    }
}
