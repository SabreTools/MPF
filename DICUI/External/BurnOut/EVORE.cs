//this file is part of BurnOut
//Copyright (C)2005-2010 Gernot Knippen
//
//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either
//version 2 of the License, or (at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.
//
//You can get a copy of the GNU General Public License
//by writing to the Free Software
//Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DICUI.External.BurnOut
{
    struct Section
    {
        public uint iVirtualSize;
        public uint iVirtualOffset;
        public uint iRawOffset;
    }

    public static class EVORE
    {
        private const int WaitSeconds = 20;

        private static Process StartSafe(string file)
        {
            Process startingprocess = new Process();
            startingprocess.StartInfo.FileName = file;
            startingprocess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startingprocess.StartInfo.CreateNoWindow = true;
            startingprocess.StartInfo.ErrorDialog = false;
            try
            {
                startingprocess.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return startingprocess;
        }

        private static string MakeTempFile(string file, string sExtension = ".exe")
        {
            FileInfo filei = new FileInfo(file);
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "tmp", filei.Name + "*" + sExtension));
            }
            catch { }
            filei = filei.CopyTo(Path.GetTempPath() + "tmp" + filei.Name + Directory.GetFiles(Path.GetTempPath(), "tmp" + filei.Name + "*" + sExtension).Length + sExtension, true);
            filei.Attributes = FileAttributes.Temporary | FileAttributes.NotContentIndexed;
            return filei.FullName;
        }

        private static bool IsEXE(string file)
        {
            BinaryReader breader = new BinaryReader(File.OpenRead(file));
            breader.ReadBytes(60);
            int PEHeaderOffset = breader.ReadInt32();
            breader.BaseStream.Seek(PEHeaderOffset, SeekOrigin.Begin);
            breader.ReadBytes(22);
            short Characteristics = breader.ReadInt16();
            breader.Close();
            //check if file is dll
            if ((Characteristics & 0x2000) == 0x2000)
                return false;
            else
                return true;
        }

        private static string[] CopyDependentDlls(string exefile)
        {
            FileInfo fiExe = new FileInfo(exefile);
            Section[] sections = ReadSections(exefile);
            BinaryReader breader = new BinaryReader(File.OpenRead(exefile), Encoding.Default);
            long lastPosition;
            string[] saDependentDLLs = null;
            breader.ReadBytes(60);
            int PEHeaderOffset = breader.ReadInt32();
            breader.BaseStream.Seek(PEHeaderOffset + 120 + 8, SeekOrigin.Begin); //120 Bytes till IMAGE_DATA_DIRECTORY array,8 Bytes=size of IMAGE_DATA_DIRECTORY
            uint ImportTableRVA = breader.ReadUInt32();
            uint ImportTableSize = breader.ReadUInt32();
            breader.BaseStream.Seek(RVA2Offset(ImportTableRVA, sections), SeekOrigin.Begin);
            breader.BaseStream.Seek(12, SeekOrigin.Current);
            uint DllNameRVA = breader.ReadUInt32();
            while (DllNameRVA != 0)
            {
                string sDllName = "";
                byte bChar;
                lastPosition = breader.BaseStream.Position;
                uint DLLNameOffset = RVA2Offset(DllNameRVA, sections);
                if (DLLNameOffset > 0)
                {
                    breader.BaseStream.Seek(DLLNameOffset, SeekOrigin.Begin);
                    if (breader.PeekChar() > -1)
                    {
                        do
                        {
                            bChar = breader.ReadByte();
                            sDllName += (char)bChar;
                        } while (bChar != 0 && breader.PeekChar() > -1);

                        sDllName = sDllName.Remove(sDllName.Length - 1, 1);
                        if (File.Exists(Path.Combine(fiExe.DirectoryName, sDllName)))
                        {
                            if (saDependentDLLs == null)
                                saDependentDLLs = new string[0];
                            else
                                saDependentDLLs = new string[saDependentDLLs.Length];

                            FileInfo fiDLL = new FileInfo(Path.Combine(fiExe.DirectoryName, sDllName));
                            saDependentDLLs[saDependentDLLs.Length - 1] = fiDLL.CopyTo(Path.GetTempPath() + sDllName, true).FullName;
                        }
                    }

                    breader.BaseStream.Seek(lastPosition, SeekOrigin.Begin);
                }

                breader.BaseStream.Seek(4 + 12, SeekOrigin.Current);
                DllNameRVA = breader.ReadUInt32();
            }

            breader.Close();
            return saDependentDLLs;
        }

        private static Section[] ReadSections(string exefile)
        {
            BinaryReader breader = new BinaryReader(File.OpenRead(exefile));
            breader.ReadBytes(60);
            uint PEHeaderOffset = breader.ReadUInt32();
            breader.BaseStream.Seek(PEHeaderOffset + 6, SeekOrigin.Begin);
            ushort NumberOfSections = breader.ReadUInt16();
            breader.BaseStream.Seek(PEHeaderOffset + 120 + 16 * 8, SeekOrigin.Begin);
            Section[] sections = new Section[NumberOfSections];
            for (int i = 0; i < NumberOfSections; i++)
            {
                breader.BaseStream.Seek(8, SeekOrigin.Current);
                uint ivs = breader.ReadUInt32();
                uint ivo = breader.ReadUInt32();
                breader.BaseStream.Seek(4, SeekOrigin.Current);
                uint iro = breader.ReadUInt32();
                breader.BaseStream.Seek(16, SeekOrigin.Current);

                sections[i] = new Section()
                {
                    iVirtualSize = ivs,
                    iVirtualOffset = ivo,
                    iRawOffset = iro,
                };
            }
            breader.Close();
            return sections;
        }

        private static uint RVA2Offset(uint RVA, Section[] sections)
        {
            int i = 0;
            while (i != sections.Length)
            {
                if (sections[i].iVirtualOffset <= RVA && sections[i].iVirtualOffset + sections[i].iVirtualSize > RVA)
                    return RVA - sections[i].iVirtualOffset + sections[i].iRawOffset;
                i++;
            }
            return 0;
        }

#region "EVORE version-search-functions"

        public static string SearchProtectDiscVersion(string file)
        {
            Process exe = new Process();
            Process[] processes = new Process[0];
            string version = "";
            DateTime timestart;
            if (!IsEXE(file))
                return "";
            string tempexe = MakeTempFile(file);
            string[] DependentDlls = CopyDependentDlls(file);
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "a*.tmp"));
            }
            catch { }
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "PCD*.sys"));
            }
            catch { }
            if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProtectDisc")))
            {
                try
                {
                    File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProtectDisc", "p*.dll"));
                }
                catch { }
            }
            exe = StartSafe(tempexe);
            if (exe == null)
                return "";
            timestart = DateTime.Now;
            do
            {
                exe.Refresh();
                string[] files = null;

                //check for ProtectDisc 8.2-x
                if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProtectDisc")))
                {
                    files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProtectDisc"), "p*.dll");
                }
                if (files != null)
                {
                    if (files.Length > 0)
                    {
                        FileVersionInfo fvinfo = FileVersionInfo.GetVersionInfo(files[0]);
                        if (fvinfo.FileVersion != "")
                        {
                            version = fvinfo.FileVersion.Replace(" ", "").Replace(",", ".");
                            //ProtectDisc 9 uses a ProtectDisc-Core dll version 8.0.x
                            if (version.StartsWith("8.0"))
                                version = "";
                            fvinfo = null;
                            break;
                        }
                    }
                }

                //check for ProtectDisc 7.1-8.1
                files = Directory.GetFiles(Path.GetTempPath(), "a*.tmp");
                if (files.Length > 0)
                {
                    FileVersionInfo fvinfo = FileVersionInfo.GetVersionInfo(files[0]);
                    if (fvinfo.FileVersion != "")
                    {
                        version = fvinfo.FileVersion.Replace(" ", "").Replace(",", ".");
                        fvinfo = null;
                        break;
                    }
                }

                if (exe.HasExited)
                    break;

                processes = Process.GetProcessesByName(exe.ProcessName);
                if (processes.Length == 2)
                {
                    processes[0].Refresh();
                    processes[1].Refresh();
                    if (processes[1].WorkingSet64 > exe.WorkingSet64)
                        exe = processes[1];
                    else if (processes[0].WorkingSet64 > exe.WorkingSet64) //else if (processes[0].Modules.Count > exe.Modules.Count)
                        exe = processes[0];
                }
            } while (processes.Length > 0 && DateTime.Now.Subtract(timestart).TotalSeconds < WaitSeconds);

            Thread.Sleep(500);
            if (!exe.HasExited)
            {
                processes = Process.GetProcessesByName(exe.ProcessName);
                if (processes.Length == 2)
                {
                    try
                    {
                        processes[0].Kill();
                    }
                    catch { }
                    processes[0].Close();
                    try
                    {
                        processes[1].Kill();
                    }
                    catch { }
                }
                else
                {
                    exe.Refresh();
                    try
                    {
                        exe.Kill();
                    }
                    catch { }
                }
            }

            exe.Close();
            Thread.Sleep(500);
            if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProtectDisc")))
            {
                try
                {
                    File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProtectDisc", "p*.dll"));
                }
                catch { }
            }
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "a*.tmp"));
            }
            catch { }
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "PCD*.sys"));
            }
            catch { }
            File.Delete(tempexe);
            if (DependentDlls != null)
            {
                for (int i = 0; i < DependentDlls.Length; i++)
                {
                    try
                    {
                        File.Delete(DependentDlls[i]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("!error while deleting file " + DependentDlls[i] + "; " + ex.Message);
                    }
                }
            }

            return version;
        }

        public static string SearchSafeDiscVersion(string file)
        {
            Process exe = new Process();
            string version = "";
            DateTime timestart;
            if (!IsEXE(file))
                return "";

            string tempexe = MakeTempFile(file);
            string[] DependentDlls = CopyDependentDlls(file);
            try
            {
                Directory.Delete(Path.Combine(Path.GetTempPath(), "~e*"), true);
            }
            catch { }
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "~e*"));
            }
            catch { }

            exe = StartSafe(tempexe);
            if (exe == null)
                return "";

            timestart = DateTime.Now;
            do
            {
                if (Directory.GetDirectories(Path.GetTempPath(), "~e*").Length > 0)
                {
                    string[] files = Directory.GetFiles(Directory.GetDirectories(Path.GetTempPath(), "~e*")[0], "~de*.tmp");
                    if (files.Length > 0)
                    {
                        StreamReader sr;
                        try
                        {
                            sr = new StreamReader(files[0], Encoding.Default);
                            string FileContent = sr.ReadToEnd();
                            sr.Close();
                            int position = FileContent.IndexOf("%ld.%ld.%ld, %ld, %s,") - 1;
                            if (position > -1)
                                version = FileContent.Substring(position + 28, 12);
                            break;
                        }
                        catch { }
                    }
                }
            } while (!exe.HasExited && DateTime.Now.Subtract(timestart).TotalSeconds < WaitSeconds);

            if (!exe.HasExited)
                exe.Kill();
            exe.Close();

            try
            {
                Directory.Delete(Path.Combine(Path.GetTempPath(), "~e*"), true);
            }
            catch { }
            try
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "~e*"));
            }
            catch { }
            File.Delete(tempexe);

            if (DependentDlls != null)
            {
                for (int i = 0; i < DependentDlls.Length; i--)
                {
                    try
                    {
                        File.Delete(DependentDlls[i]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("!error while deleting file " + DependentDlls[i] + "; " + ex.Message);
                    }
                }
            }

            return version;
        }

#endregion
    }
}
