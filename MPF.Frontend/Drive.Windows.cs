using System;
#if NET462_OR_GREATER || NETCOREAPP
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
#endif

namespace MPF.Frontend
{
    public partial class Drive
    {
        /// <summary>
        /// Mark drives that Windows reports as floppy media via WMI.
        /// </summary>
        /// <param name="drives">Drives to update in place</param>
        private static void MarkWindowsFloppyDrives(Drive[] drives)
        {
#if NET462_OR_GREATER || NETCOREAPP
            try
            {
                CimSession session = CimSession.Create(null);
                var collection = session.QueryInstances("root\\CIMV2", "WQL", "SELECT * FROM Win32_LogicalDisk");

                foreach (CimInstance instance in collection)
                {
                    CimKeyedCollection<CimProperty> properties = instance.CimInstanceProperties;
                    uint? mediaType = properties["MediaType"]?.Value as uint?;
                    if (mediaType is not null && ((mediaType > 0 && mediaType < 11) || (mediaType > 12 && mediaType < 22)))
                    {
                        char devId = (properties["Caption"].Value as string ?? string.Empty)[0];
                        Array.ForEach(drives, d =>
                        {
                            if (d?.DevicePath is not null && d.DevicePath[0] == devId)
                                d.InternalDriveType = Frontend.InternalDriveType.Floppy;
                        });
                    }
                }
            }
            catch
            {
                // No-op
            }
#endif
        }
    }
}
