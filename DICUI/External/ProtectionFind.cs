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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DICUI.External
{
    // TODO: Add any missing protection schemes
    public class ProtectionFind
    {
        // Special protections
        private bool IsCDCheck;
        private bool IsDummyFiles;
        private bool IsImpulseReactorWithoutVersion;
        private bool IsSafeDiscRemovedVersion;
        private bool IsSolidShieldWithoutVersion;
        private bool IsStarForceWithoutVersion;
        private bool IsLaserLockWithoutVersion;

        private string SecuROMpaulversion;

        public ProtectionFind()
        {
        }

        /// <summary>
        /// Scan a path to find any known copy protection(s)
        /// </summary>
        public Dictionary<string, string> Scan(string path)
        {
            var protections = new Dictionary<string, string>();

            // Set all of the output variables
            IsImpulseReactorWithoutVersion = false;
            IsLaserLockWithoutVersion = false;
            IsSafeDiscRemovedVersion = false;
            IsSolidShieldWithoutVersion = false;
            IsStarForceWithoutVersion = false;
            SecuROMpaulversion = "";

            // Create mappings for checking against
            var mappings = CreateFilenameProtectionMapping();

            // Get the lists of files to be used
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            // DVD-Movie-PROTECT
            if (DVDMoviePROTECT(path, files))
                protections[path] = "DVD-Movie-PROTECT";

            // LaserLock
            if (Directory.Exists(Path.Combine(path, "LASERLOK")))
                protections[path] = "LaserLock";

            // Protect DVD-Video
            if (ProtectDVDVideo(path, files))
                protections[path] = "Protect DVD-Video";

            // Loop through all files and scan them
            foreach (string file in files)
            {
                // If the file is in the list of known files, add that to the protections found
                if (mappings.ContainsKey(Path.GetFileName(file)))
                    protections[file] = mappings[Path.GetFileName(file)];

                // If the extension matches one of the known extension, add that to the protections found
                if (mappings.ContainsKey(Path.GetExtension(file)))
                    protections[file] = mappings[Path.GetExtension(file)];

                // Now check to see if the file contains any additional information
                string protectionname = ScanInFile(file);
                if (!String.IsNullOrEmpty(protectionname))
                {
                    if (IsImpulseReactorWithoutVersion)
                    {
                        IsImpulseReactorWithoutVersion = false;
                        if (ImpulseReactor(out string version, files))
                            protections[file] = "Impulse Reactor " + version;
                    }
                    else if (IsLaserLockWithoutVersion)
                    {
                        IsLaserLockWithoutVersion = false;
                        if (LaserLock(out string version, path, files) && version.Length > 0)
                            protections[file] = "LaserLock " + version;
                    }
                    else if (IsSafeDiscRemovedVersion)
                    {
                        IsSafeDiscRemovedVersion = false;
                        if (SafeDisc2Plus(out string version, files) && version != "2-4")
                            protections[file] = "SafeDisc " + version;
                    }
                    else if (IsSolidShieldWithoutVersion)
                    {
                        IsSolidShieldWithoutVersion = false;
                        if (SolidShield(out string version, files) && version.Length > 0)
                            protections[file] = "SolidShield " + version;
                    }
                    else if (IsStarForceWithoutVersion)
                    {
                        IsStarForceWithoutVersion = false;
                        if (StarForce(out string version, files) && version.Length > 0)
                            protections[file] = "StarForce " + version;
                    }

                    if (SecuROMpaulversion.Length > 0)
                    {
                        if (!protectionname.StartsWith("SecuROM Product Activation"))
                            protections[file] = protectionname + " + SecuROM Product Activation" + SecuROMpaulversion;
                    }
                    else
                        protections[file] = protectionname;
                }
            }

            // If we have an empty list, we need to take care of that
            if (protections.Count(p => !String.IsNullOrWhiteSpace(p.Value)) == 0)
            {
                protections = null;
            }

            return protections;
        }

        /// <summary>
        /// Scan an individual file for copy protection
        /// </summary>
        /// <remarks>
        /// TODO: Handle archives (zip, arc, cab[ms], cab[is])
        /// TODO: Find protection mentions in text files
        /// </remarks>
        private string ScanInFile(string file)
        {
            #region Content Checks

            try
            {
                StreamReader sr = new StreamReader(file, Encoding.Default);
                int position = -1;
                string FileContent = sr.ReadToEnd();
                sr.Close();

                // Create mappings for checking against
                var mappings = CreateInternalProtectionMapping();

                // Loop through all of the string-only possible matches
                foreach (string key in mappings.Keys)
                {
                    if (FileContent.Contains(key))
                    {
                        return mappings[key];
                    }
                }

                // CD-Cops
                position = FileContent.IndexOf("CD-Cops,  ver. ");
                if (position > -1)
                {
                    return "CD-Cops " + GetCDDVDCopsVersion(file, position);
                }

                // DVD-Cops
                position = FileContent.IndexOf("DVD-Cops,  ver. ");
                if (position > -1)
                {
                    return "DVD-Cops " + GetCDDVDCopsVersion(file, position);
                }

                // Impulse Reactor
                if (FileContent.Contains("CVPInitializeClient"))
                {
                    if (FileContent.Contains("A" + (char)0x00 + "T" + (char)0x00 + "T" + (char)0x00 + "L" + (char)0x00 + "I" + (char)0x00 + "S" + (char)0x00 + "T" + (char)0x00 + (char)0x00 + (char)0x00 + "E" + (char)0x00 + "L" + (char)0x00 + "E" + (char)0x00 + "M" + (char)0x00 + "E" + (char)0x00 + "N" + (char)0x00 + "T" + (char)0x00 + (char)0x00 + (char)0x00 + "N" + (char)0x00 + "O" + (char)0x00 + "T" + (char)0x00 + "A" + (char)0x00 + "T" + (char)0x00 + "I" + (char)0x00 + "O" + (char)0x00 + "N"))
                    {
                        return "Impulse Reactor " + GetFileVersion(file);
                    }
                    else
                    {
                        IsImpulseReactorWithoutVersion = true;
                        return "Impulse Reactor";
                    }
                }

                // JoWooD X-Prot
                if (FileContent.Contains(".ext    "))
                {
                    position = FileContent.IndexOf("kernel32.dll" + (char)0x00 + (char)0x00 + (char)0x00 + "VirtualProtect");
                    if (position > -1)
                    {
                        position--; ;
                        return "JoWooD X-Prot " + GetJoWooDXProt1Version(file, position);
                    }
                    else
                        return "JoWooD X-Prot v1";
                }

                // LaserLock
                if (FileContent.Contains("Packed by SPEEnc V2 Asterios Parlamentas.PE"))
                {
                    position = FileContent.IndexOf("GetModuleHandleA" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00 + "GetProcAddress" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00 + "LoadLibraryA" + (char)0x00 + (char)0x00 + "KERNEL32.dll" + (char)0x00 + "ëy" + (char)0x01 + "SNIF");
                    if (position > -1)
                    {
                        position--;
                        return "LaserLock " + GetLaserLockVersion(FileContent, position) + " " + GetLaserLockBuild(FileContent, true);
                    }
                    else
                        return "LaserLock Marathon " + GetLaserLockBuild(FileContent, false);
                }

                position = FileContent.IndexOf("GetModuleHandleA" + (char)0x00 + (char)0x00 + (char)0x00
                    + (char)0x00 + "GetProcAddress" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00
                    + "LoadLibraryA" + (char)0x00 + (char)0x00 + "KERNEL32.dll" + (char)0x00 + "ëy" + (char)0x01 + "SNIF");
                if (position > -1)
                {
                    position--; ;
                    return "LaserLock " + GetLaserLockVersion(FileContent, position) + " " + GetLaserLockBuild(FileContent, false);
                }

                // ProtectDisc
                position = FileContent.IndexOf("HúMETINF");
                if (position > -1)
                {
                    position--;
                    string version = EVORE.SearchProtectDiscVersion(file);
                    if (version.Length > 0)
                    {
                        string[] astrVersionArray = version.Split('.');
                        if (astrVersionArray[0] == "9")
                        {
                            if (GetProtectDiscVersionBuild76till10(file, position, out int ibuild).Length > 0)
                                return "ProtectDisc " + astrVersionArray[0] + "." + astrVersionArray[1] + astrVersionArray[2] + "." + astrVersionArray[3] + " (Build " + ibuild + ")";
                        }
                        else
                            return "ProtectDisc " + astrVersionArray[0] + "." + astrVersionArray[1] + "." + astrVersionArray[2] + " (Build " + astrVersionArray[3] + ")";
                    }
                }

                position = FileContent.IndexOf("ACE-PCD");
                if (position > -1)
                {
                    position--;
                    string version;
                    version = EVORE.SearchProtectDiscVersion(file);
                    if (version.Length > 0)
                    {
                        string[] astrVersionArray = version.Split('.');
                        return "ProtectDisc " + astrVersionArray[0] + "." + astrVersionArray[1] + "." + astrVersionArray[2] + " (Build " + astrVersionArray[3] + ")";
                    }

                    return "ProtectDisc " + GetProtectDiscVersionBuild6till8(file, position);
                }

                // SafeDisc / SafeCast
                position = FileContent.IndexOf("BoG_ *90.0&!!  Yy>");
                if (position > -1)
                {
                    position--;
                    if (FileContent.IndexOf("product activation library") > 0)
                        return "SafeCast " + GetSafeDiscVersion(file, position);
                    else
                        return "SafeDisc " + GetSafeDiscVersion(file, position);
                }

                if (FileContent.Contains((char)0x00 + (char)0x00 + "BoG_")
                    || FileContent.Contains("stxt774")
                    || FileContent.Contains("stxt371"))
                {
                    string version = EVORE.SearchSafeDiscVersion(file);
                    if (version.Length > 0)
                        return "SafeDisc " + version;

                    IsSafeDiscRemovedVersion = true;
                    return "SafeDisc 3.20-4.xx (version removed)";
                }

                // SecuROM
                position = FileContent.IndexOf("AddD" + (char)0x03 + (char)0x00 + (char)0x00 + (char)0x00);
                if (position > -1)
                {
                    return "SecuROM " + GetSecuROM4Version(file, position);
                }

                position = FileContent.IndexOf("" + (char)0xCA + (char)0xDD + (char)0xDD + (char)0xAC + (char)0x03);
                if (position > -1)
                {
                    position--; ;
                    return "SecuROM " + GetSecuROM4and5Version(file, position);
                }

                if (FileContent.StartsWith(".securom" + (char)0xE0 + (char)0xC0))
                    return "SecuROM " + GetSecuROM7Version(file);

                if (FileContent.Contains("_and_play.dll" + (char)0x00 + "drm_pagui_doit"))
                {
                    SecuROMpaulversion = GetFileVersion(file);
                    return "SecuROM Product Activation " + SecuROMpaulversion;
                }

                // SolidShield
                if (FileContent.Contains("D" + (char)0x00 + "V" + (char)0x00 + "M" + (char)0x00 + " " + (char)0x00 + "L" + (char)0x00
                    + "i" + (char)0x00 + "b" + (char)0x00 + "r" + (char)0x00 + "a" + (char)0x00 + "r" + (char)0x00 + "y"))
                    return "SolidShield " + GetFileVersion(file);

                position = FileContent.IndexOf("" + (char)0xEF + (char)0xBE + (char)0xAD + (char)0xDE);
                if (position > -1)
                {
                    position--;
                    if (FileContent.Substring(position + 5, 3) == "" + (char)0x00 + (char)0x00 + (char)0x00
                    && FileContent.Substring(position + 16, 4) == "" + (char)0x00 + (char)0x10 + (char)0x00 + (char)0x00)
                    {
                        IsSolidShieldWithoutVersion = true;
                        return "SolidShield 1";
                    }
                }

                position = FileContent.IndexOf("" + (char)0xAD + (char)0xDE + (char)0xFE + (char)0xCA + (char)0x4);
                position = position == -1 ? FileContent.IndexOf("" + (char)0xAD + (char)0xDE + (char)0xFE + (char)0xCA + (char)0x5) : position;
                if (position > -1)
                {
                    position--;
                    if (FileContent.Substring(position + 5, 3) == "" + (char)0x00 + (char)0x00 + (char)0x00
                        && FileContent.Substring(position + 16, 4) == "" + (char)0x00 + (char)0x10 + (char)0x00 + (char)0x00)
                    {
                        return "SolidShield 2";
                    }
                    else if (FileContent.Substring(position + 5, 3) == "" + (char)0x00 + (char)0x00 + (char)0x00
                        && FileContent.Substring(position + 16, 4) == "" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00)
                    {
                        position = FileContent.IndexOf("T" + (char)0x00 + "a" + (char)0x00 + "g" + (char)0x00 + "e" + (char)0x00 + "s"
                            + (char)0x00 + "S" + (char)0x00 + "e" + (char)0x00 + "t" + (char)0x00 + "u" + (char)0x00 + "p"
                            + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00 + "0" + (char)0x00 + (char)0x8
                            + (char)0x00 + (char)0x1 + (char)0x0 + "F" + (char)0x00 + "i" + (char)0x00 + "l" + (char)0x00 + "e"
                            + (char)0x00 + "V" + (char)0x00 + "e" + (char)0x00 + "r" + (char)0x00 + "s" + (char)0x00 + "i" + (char)0x00
                            + "o" + (char)0x00 + "n" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00);
                        if (position > -1)
                        {
                            position--;
                            return "SolidShield 2 + Tagès " + FileContent.Substring(position + 0x38, 1) + "." + FileContent.Substring(position + 0x38 + 4, 1) + "." + FileContent.Substring(position + 0x38 + 8, 1) + "." + FileContent.Substring(position + 0x38 + 12, 1);
                        }
                        else
                        {
                            IsSolidShieldWithoutVersion = true;
                            return "SolidShield 2";
                        }
                    }
                }

                // StarForce
                if (FileContent.Contains(".sforce") || FileContent.Contains(".brick"))
                {
                    IsStarForceWithoutVersion = true;
                    return "StarForce 3-5";
                }

                if (FileContent.Contains("P" + (char)0x00 + "r" + (char)0x00 + "o" + (char)0x00 + "t" + (char)0x00 + "e" + (char)0x00
                    + "c" + (char)0x00 + "t" + (char)0x00 + "e" + (char)0x00 + "d" + (char)0x00 + " " + (char)0x00 + "M" + (char)0x00
                    + "o" + (char)0x00 + "d" + (char)0x00 + "u" + (char)0x00 + "l" + (char)0x00 + "e"))
                {
                    IsStarForceWithoutVersion = true;
                    return "StarForce 5";
                }

                if (FileContent.Contains("(" + (char)0x00 + "c" + (char)0x00 + ")" + (char)0x00 + " " + (char)0x00 + "P" + (char)0x00
                    + "r" + (char)0x00 + "o" + (char)0x00 + "t" + (char)0x00 + "e" + (char)0x00 + "c" + (char)0x00 + "t" + (char)0x00
                    + "i" + (char)0x00 + "o" + (char)0x00 + "n" + (char)0x00 + " " + (char)0x00 + "T" + (char)0x00 + "e" + (char)0x00
                    + "c" + (char)0x00 + "h" + (char)0x00 + "n" + (char)0x00 + "o" + (char)0x00 + "l" + (char)0x00 + "o" + (char)0x00
                    + "g" + (char)0x00 + "y" + (char)0x00)
                || FileContent.Contains("Protection Technology, Ltd."))
                {
                    //if (FileContent.Contains("PSA_GetDiscLabel")
                    //if (FileContent.Contains("(c) Protection Technology")
                    position = FileContent.IndexOf("TradeName") - 1;
                    if (position != -1 && position != -2)
                        return "StarForce " + GetFileVersion(file) + " (" + FileContent.Substring(position + 22, 30).Split((char)0x00)[0] + ")";
                    else
                        return "StarForce " + GetFileVersion(file);
                }

                // TAGES
                if (FileContent.Contains("protected-tages-runtime.exe") ||
                    FileContent.Contains("tagesprotection.com"))
                {
                    return "Tagès " + GetFileVersion(file);
                }

                position = FileContent.IndexOf("" + (char)0xE8 + "u" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0xE8);
                if (position > -1)
                {
                    position--;
                    if (FileContent.Substring(position + 8, 3) == "" + (char)0xFF + (char)0xFF + "h")
                    {
                        return "Tagès " + GetTagesVersion(file, position);
                    }
                }

                // VOB ProtectCD/DVD
                position = FileContent.IndexOf("VOB ProtectCD");
                if (position > -1)
                {
                    position--;
                    return "VOB ProtectCD/DVD " + GetProtectCDoldVersion(file, position);
                }

                position = FileContent.IndexOf("DCP-BOV" + (char)0x00 + (char)0x00);
                if (position > -1)
                {
                    position--;
                    string version;
                    version = GetVOBProtectCDDVDVersion(file, position);
                    if (version.Length > 0)
                    {
                        return "VOB ProtectCD/DVD " + version;
                    }

                    version = EVORE.SearchProtectDiscVersion(file);
                    if (version.Length > 0)
                    {
                        if (version.StartsWith("2"))
                        {
                            version = "6" + version.Substring(1);
                        }
                        return "VOB ProtectCD/DVD " + version;
                    }

                    return "VOB ProtectCD/DVD 5.9-6.0" + GetVOBProtectCDDVDBuild(file, position);
                }

                if (!IsCDCheck)
                {
                    if (FileContent.Contains("GetDriveType") || FileContent.Contains("GetVolumeInformation"))
                    {
                        IsCDCheck = true;
                    }
                }
            }
            catch { }

            #endregion

            #region Name Checks

            FileInfo fi = new FileInfo(file);

            // Cactus Data Shield
            if (Path.GetFileName(file) == "CDSPlayer.app")
                using (StreamReader cactusReader = new StreamReader(file))
                    return "Cactus Data Shield " + cactusReader.ReadLine().Substring(3) + "(" + cactusReader.ReadLine() + ")";

            // Impulse Reactor
            if (Path.GetFileName(file) == "ImpulseReactor.dll")
                return "Impulse Reactor " + GetFileVersion(file);

            // LaserLock
            if (Path.GetFileName(file) == "NOMOUSE.SP")
                return "LaserLock " + GetLaserLockVersion16Bit(file);

            // SafeDisc
            if (Path.GetFileName(file) == "DPLAYERX.DLL")
            {
                if (fi.Length == 81408)
                    return "SafeDisc 1.0x";
                else if (fi.Length == 155648)
                    return "SafeDisc 1.1x";
                else if (fi.Length == 156160)
                    return "SafeDisc 1.1x-1.2x";
                else if (fi.Length == 163328)
                    return "SafeDisc 1.3x";
                else if (fi.Length == 165888)
                    return "SafeDisc 1.35";
                else if (fi.Length == 172544)
                    return "SafeDisc 1.40";
                else if (fi.Length == 173568)
                    return "SafeDisc 1.4x";
                else if (fi.Length == 136704)
                    return "SafeDisc 1.4x";
                else if (fi.Length == 138752)
                    return "SafeDisc 1.5x";
                else
                    return "SafeDisc 1";
            }
            else if (Path.GetFileName(file).ToLower() == "drvmgt.dll")
            {
                if (fi.Length == 34816)
                    return "SafeDisc 1.0x";
                else if (fi.Length == 32256)
                    return "SafeDisc 1.1x-1.3x";
                else if (fi.Length == 31744)
                    return "SafeDisc 1.4x";
                else if (fi.Length == 34304)
                    return "SafeDisc 1.5x-2.40";
                else if (fi.Length == 35840)
                    return "SafeDisc 2.51-2.60";
                else if (fi.Length == 40960)
                    return "SafeDisc 2.70";
                else if (fi.Length == 23552)
                    return "SafeDisc 2.80";
                else if (fi.Length == 41472)
                    return "SafeDisc 2.90-3.10";
                else if (fi.Length == 24064)
                    return "SafeDisc 3.15-3.20";
                else
                    return "SafeDisc 2-4";
            }
            else if (Path.GetFileName(file).ToLower() == "secdrv.sys")
            {
                if (fi.Length == 20128)
                    return "SafeDisc 2.10";
                else if (fi.Length == 27440)
                    return "SafeDisc 2.30";
                else if (fi.Length == 28624)
                    return "SafeDisc 2.40";
                else if (fi.Length == 18768)
                    return "SafeDisc 2.50";
                else if (fi.Length == 28400)
                    return "SafeDisc 2.51";
                else if (fi.Length == 29392)
                    return "SafeDisc 2.60";
                else if (fi.Length == 11376)
                    return "SafeDisc 2.70";
                else if (fi.Length == 12464)
                    return "SafeDisc 2.80";
                else if (fi.Length == 12400)
                    return "SafeDisc 2.90";
                else if (fi.Length == 12528)
                    return "SafeDisc 3.10";
                else if (fi.Length == 12528)
                    return "SafeDisc 3.15";
                else if (fi.Length == 11973)
                    return "SafeDisc 3.20";
                else
                    return "SafeDisc 2-4";
            }

            // SolidShield
            if (Path.GetFileName(file) == "dvm.dll")
                return "SolidShield " + ScanInFile(file);
            else if (Path.GetFileName(file) == "hc.dll")
                return "SolidShield " + ScanInFile(file);
            else if (Path.GetFileName(file) == "solidshield-cd.dll")
                return "SolidShield " + ScanInFile(file);
            else if (Path.GetFileName(file) == "c11prot.dll")
                return "SolidShield " + ScanInFile(file);

            // WTM Copy Protection
            if (Path.GetFileName(file) == "Viewer.exe")
                return "WTM Copy Protection " + GetFileVersion(file);

            // Dummy Files - False positive on DVDs
            //if (fi.Length > 681574400)
            //    return "Possibly Dummy Files";

            #endregion

            return "";
        }

        // TODO: Do any of these trigger an infinite loop when called? (Especially `out string version` ones
        #region Protections

        private bool DummyFiles(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    if (new FileInfo(files[i]).Length > 681574400) // 681574400 Bytes = 650 Mb
                        return true;
                }
                catch { }
            }

            return false;
        }

        private bool DVDMoviePROTECT(string path, string[] files)
        {
            if (Directory.Exists(Path.Combine(path, "VIDEO_TS")))
            {
                string[] bupfiles = files.Where(s => s.EndsWith(".bup")).ToArray();
                for (int i = 0; i < bupfiles.Length; i++)
                {
                    FileInfo bupfile = new FileInfo(bupfiles[i]);
                    FileInfo ifofile = new FileInfo(bupfile.DirectoryName + "\\" + bupfile.Name.Substring(0, bupfile.Name.Length - bupfile.Extension.Length) + ".ifo");
                    if (bupfile.Length != ifofile.Length)
                        return true;
                }
            }

            return false;
        }

        private bool ImpulseReactor(out string version, string[] files)
        {
            version = "";
            int i = files.ToList().IndexOf("ImpulseReactor.dll");
            if (i > -1)
            {
                version = GetFileVersion(files[i]);
            }

            return false;
        }

        private bool LaserLock(out string version, string path, string[] files)
        {
            version = "";
            int nomouseindex = files.ToList().IndexOf("NOMOUSE.SP");
            if (nomouseindex > -1)
            {
                version = GetLaserLockVersion16Bit(files[nomouseindex]);
                return true;
            }

            if (files.Count(s => s.EndsWith("NOMOUSE.COM")) > 0)
                return true;
            if (files.Count(s => s.EndsWith("l16dll.dll")) > 0)
                return true;
            if (files.Count(s => s.EndsWith("laserlok.in")) > 0)
                return true;
            if (files.Count(s => s.EndsWith("laserlok.o10")) > 0)
                return true;
            if (files.Count(s => s.EndsWith("laserlok.011")) > 0)
                return true;
            if (Directory.Exists(Path.Combine(path, "LASERLOK")))
                return true;

            return false;
        }

        private bool ProtectDVDVideo(string path, string[] files)
        {
            if (Directory.Exists(Path.Combine(path, "VIDEO_TS")))
            {
                string[] ifofiles = files.Where(s => s.EndsWith(".ifo")).ToArray();
                for (int i = 0; i < ifofiles.Length; i++)
                {
                    FileInfo ifofile = new FileInfo(ifofiles[i]);
                    if (ifofile.Length == 0)
                        return true;
                }
            }

            return false;
        }

        private bool SafeDisc1(out string version, string[] files)
        {
            version = "";
            bool found = true;

            // Check to make sure all 5 files exist
            if (files.Count(s => s.EndsWith("00000001.TMP")) > 0
                && files.Count(s => s.EndsWith("CLCD16.DLL")) > 0
                && files.Count(s => s.EndsWith("CLCD32.DLL")) > 0
                && files.Count(s => s.EndsWith("CLOKSPL.EXE")) > 0
                && files.Count(s => s.EndsWith("DPLAYERX.DLL")) > 0)
            {
                int dplayerIndex = Array.FindIndex(files, s => s.EndsWith("DPLAYERX.DLL"));
                if (dplayerIndex > -1)
                {
                    FileInfo fi = new FileInfo(files[dplayerIndex]);
                    if (fi.Length == 81408)
                        version = "1.0x";
                    else if (fi.Length == 155648)
                        version = "1.1x";
                    else if (fi.Length == 156160)
                        version = "1.1x-1.2x";
                    else if (fi.Length == 163328)
                        version = "1.3x";
                    else if (fi.Length == 165888)
                        version = "1.35";
                    else if (fi.Length == 172544)
                        version = "1.40";
                    else if (fi.Length == 173568)
                        version = "1.4x";
                    else if (fi.Length == 136704)
                        version = "1.4x";
                    else if (fi.Length == 138752)
                        version = "1.5x";
                }
            }

            // Check if the management dll exists
            int drvmgtIndex = Array.FindIndex(files, s => s.ToLower().EndsWith("drvmgt.dll"));
            if (drvmgtIndex > -1)
            {
                found = true;
                if (version.Length == 0)
                {
                    FileInfo fi = new FileInfo(files[drvmgtIndex]);

                    if (fi.Length == 34816)
                        version = "1.0x";
                    else if (fi.Length == 32256)
                        version = "1.1x-1.3x";
                    else if (fi.Length == 31744)
                        version = "1.4x";
                    else if (fi.Length == 34304)
                        version = "1.5x";
                }
            }

            // Finally, check to see if any telltale files are found
            if (files.Count(s => s.ToLower().EndsWith(".icd")) > 0)
                found = true;
            if (files.Count(s => s.ToLower().EndsWith(".016")) > 0)
                found = true;
            if (files.Count(s => s.ToLower().EndsWith(".256")) > 0)
                found = true;

            // If anything is found, set the version if it wasn't already
            if (found == true)
            {
                if (version.Length == 0)
                    version = "1";
                return true;
            }

            return false;
        }

        private bool SafeDisc2Plus(out string version, string[] files)
        {
            version = "";
            bool found = false;
            if (files.Count(s => s.EndsWith("00000002.TMP")) > 0)
                found = true;
            int fileindexdrvmgt = Array.FindIndex(files, s => s.ToLower().EndsWith("drvmgt.dll"));
            int fileindexsecdrv = Array.FindIndex(files, s => s.ToLower().EndsWith("secdrv.sys"));
            if (fileindexsecdrv > -1)
            {
                FileInfo sdi = new FileInfo(files[fileindexsecdrv]);
                if (sdi.Length == 18768)
                {
                    found = true;
                    version = "2.50";
                }
            }
            if (fileindexsecdrv > -1 && fileindexdrvmgt > -1)
            {
                FileInfo sdi = new FileInfo(files[fileindexsecdrv]);
                FileInfo dmi = new FileInfo(files[fileindexdrvmgt]);

                if (dmi.Length == 34304 && sdi.Length == 20128)
                    version = "2.10";
                else if (dmi.Length == 34304 && sdi.Length == 27440)
                    version = "2.30";
                else if (dmi.Length == 34304 && sdi.Length == 28624)
                    version = "2.40";
                else if (dmi.Length == 35840 && sdi.Length == 28400)
                    version = "2.51";
                else if (dmi.Length == 35840 && sdi.Length == 29392)
                    version = "2.60";
                else if (dmi.Length == 40960 && sdi.Length == 11376)
                    version = "2.70";
                else if (dmi.Length == 23552 && sdi.Length == 12464)
                    version = "2.80";
                else if (dmi.Length == 41472 && sdi.Length == 12400)
                    version = "2.90";
                else if (dmi.Length == 41472 && sdi.Length == 12528)
                    version = "3.10";
                else if (dmi.Length == 24064 && sdi.Length == 12528)
                    version = "3.15";
                else if (dmi.Length == 24064 && sdi.Length == 11973)
                    version = "3.20";

                if (version != "")
                    found = true;
            }
            if (fileindexdrvmgt > -1 && version == "")
            {
                FileInfo dmi = new FileInfo(files[fileindexdrvmgt]);

                if (dmi.Length == 34304)
                    version = "2.0x";
                else if (dmi.Length == 35840)
                    version = "2.6x";
                else if (dmi.Length == 40960)
                    version = "2.7x";
                else if (dmi.Length == 23552)
                    version = "2.8x";
                else if (dmi.Length == 41472)
                    version = "2.9x";

                if (version != "")
                    found = true;
            }
            if (found == true)
            {
                if (version == "")
                    version = "2-4";
                return true;
            }

            return false;
        }

        private bool SolidShield(out string version, string[] files)
        {
            version = "";
            int fileindex = files.ToList().IndexOf("dvm.dll");
            if (fileindex > -1)
            {
                version = ScanInFile(files[fileindex]);
                return true;
            }
            fileindex = files.ToList().IndexOf("hc.dll");
            if (fileindex > -1)
            {
                version = ScanInFile(files[fileindex]);
                return true;
            }
            fileindex = files.ToList().IndexOf("solidshield-cd.dll");
            if (fileindex > -1)
            {
                version = ScanInFile(files[fileindex]);
                return true;
            }
            fileindex = files.ToList().IndexOf("c11prot.dll");
            if (fileindex > -1)
            {
                version = ScanInFile(files[fileindex]);
                return true;
            }

            return false;
        }

        private bool StarForce(out string version, string[] files)
        {
            version = "";
            int fileindex = files.ToList().IndexOf("protect.dll");
            if (fileindex > -1)
            {
                version = ScanInFile(files[fileindex]);
                return true;
            }
            fileindex = files.ToList().IndexOf("protect.exe");
            if (fileindex > -1)
            {
                version = ScanInFile(files[fileindex]);
                return true;
            }

            return false;
        }

        #endregion

        #region Version detections

        private string GetCDDVDCopsVersion(string file, int position)
        {
            char[] version;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position + 15, SeekOrigin.Begin); // Begin reading after "CD-Cops,  ver."
            version = br.ReadChars(4);
            if (version[0] == 0x00)
                return "";
            return new string(version);
        }
        
        private string GetJoWooDXProt1Version(string file, int position)
        {
            char[] version = new char[5];
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position + 67, SeekOrigin.Begin);
            version[0] = br.ReadChar();
            br.ReadByte();
            version[1] = br.ReadChar();
            br.ReadByte();
            version[2] = br.ReadChar();
            br.ReadByte();
            version[3] = br.ReadChar();
            version[4] = br.ReadChar();
            br.Close();

            return version[0] + "." + version[1] + "." + version[2] + "." + version[3] + version[4];
        }

        private string GetLaserLockBuild(string FileContent, bool Version2)
        {
            int position = FileContent.IndexOf("Unkown" + (char)0 + "Unkown");
            string Year;
            string Month;
            string Day;
            if (Version2)
            {
                Day = FileContent.Substring(position + 14, 2);
                Month = FileContent.Substring(position + 14 + 3, 2);
                Year = "20" + FileContent.Substring(position + 14 + 6, 2);
            }
            else
            {
                Day = FileContent.Substring(position + 13, 2);
                Month = FileContent.Substring(position + 13 + 3, 2);
                Year = "20" + FileContent.Substring(position + 13 + 6, 2);
            }

            return "(Build " + Year + "-" + Month + "-" + Day + ")";
        }

        private string GetLaserLockVersion(string FileContent, int position)
        {
            return FileContent.Substring(position + 76, 4);
        }

        private string GetLaserLockVersion16Bit(string file)
        {
            char[] version = new char[3];
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(71, SeekOrigin.Begin);
            version[0] = br.ReadChar();
            br.ReadByte();
            version[1] = br.ReadChar();
            version[2] = br.ReadChar();
            br.Close();

            if (Char.IsNumber(version[0]) && Char.IsNumber(version[1]) && Char.IsNumber(version[2]))
                return version[0] + "." + version[1] + version[2];
            return "";
        }

        private string GetProtectCDoldVersion(string file, int position)
        {
            char[] version = new char[3];
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position + 16, SeekOrigin.Begin); // Begin reading after "VOB ProtectCD"
            version[0] = br.ReadChar();
            br.ReadByte();
            version[1] = br.ReadChar();
            version[2] = br.ReadChar();
            br.Close();
            if (Char.IsNumber(version[0]) && Char.IsNumber(version[1]) && Char.IsNumber(version[2]))
                return version[0] + "." + version[1] + version[2];
            return "old";
        }

        private string GetProtectDiscVersionBuild6till8(string file, int position)
        {
            string version;
            string strBuild;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);

            br.BaseStream.Seek(position - 12, SeekOrigin.Begin);
            if (br.ReadByte() == 0xA && br.ReadByte() == 0xD && br.ReadByte() == 0xA && br.ReadByte() == 0xD) // ProtectDisc 6-7 with Build Number in plain text
            {
                br.BaseStream.Seek(position - 12 - 6, SeekOrigin.Begin);
                if (new string(br.ReadChars(6)) == "Henrik") // ProtectDisc 7
                {
                    version = "7.1-7.5";
                    br.BaseStream.Seek(position - 12 - 6 - 6, SeekOrigin.Begin);
                }
                else // ProtectDisc 6
                {
                    version = "6";
                    br.BaseStream.Seek(position - 12 - 10, SeekOrigin.Begin);
                    while (true) //search for e.g. "Build 050913 -  September 2005"
                    {
                        if (Char.IsNumber(br.ReadChar()))
                            break;
                        br.BaseStream.Seek(-2, SeekOrigin.Current); //search upwards
                    }

                    br.BaseStream.Seek(-5, SeekOrigin.Current);
                }
            }
            else
            {
                br.BaseStream.Seek(position + 28, SeekOrigin.Begin);
                if (br.ReadByte() == 0xFB)
                {
                    br.Close();
                    return "7.6-7.x";
                }
                else
                {
                    br.Close();
                    return "8.0";
                }
            }
            strBuild = "" + br.ReadChar() + br.ReadChar() + br.ReadChar() + br.ReadChar() + br.ReadChar();
            br.Close();
            return version + " (Build " + strBuild + ")";
        }

        private string GetProtectDiscVersionBuild76till10(string file, int position, out int irefBuild)
        {
            irefBuild = 0;
            byte version, subversion, versionindicatorPD9, subsubversionPD9x, subversionPD9x1, subversionPD9x2;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position + 37, SeekOrigin.Begin);
            subversion = br.ReadByte();
            br.ReadByte();
            version = br.ReadByte();
            br.BaseStream.Seek(position + 49, SeekOrigin.Begin);
            irefBuild = br.ReadInt32();
            br.BaseStream.Seek(position + 53, SeekOrigin.Begin);
            versionindicatorPD9 = br.ReadByte();
            br.BaseStream.Seek(position + 0x40, SeekOrigin.Begin);
            subsubversionPD9x = br.ReadByte();
            subversionPD9x2 = br.ReadByte();
            subversionPD9x1 = br.ReadByte();
            br.Close();

            // version 7
            if (version == 0xAC)
                return "7." + (subversion ^ 0x43) + " (Build " + irefBuild + ")";
            // version 8
            else if (version == 0xA2)
            {
                if (subversion == 0x46)
                {
                    if ((irefBuild & 0x3A00) == 0x3A00)
                        return "8.2" + " (Build " + irefBuild + ")";
                    else
                        return "8.1" + " (Build " + irefBuild + ")";
                }
                return "8." + (subversion ^ 0x47) + " (Build " + irefBuild + ")";
            }
            // version 9
            else if (version == 0xA3)
            {
                // version removed or not given
                if ((subversionPD9x2 == 0x5F && subversionPD9x1 == 0x61) || (subversionPD9x1 == 0 && subversionPD9x2 == 0))
                {
                    if (versionindicatorPD9 == 0xB)
                        return "9.0-9.4" + " (Build " + irefBuild + ")";
                    else if (versionindicatorPD9 == 0xC)
                    {
                        if (subversionPD9x2 == 0x5F && subversionPD9x1 == 0x61)
                            return "9.5-9.11" + " (Build " + irefBuild + ")";
                        else if (subversionPD9x1 == 0 && subversionPD9x2 == 0)
                            return "9.11-9.20" + " (Build " + irefBuild + ")";
                    }
                    else
                        return "9." + subversionPD9x1 + subversionPD9x2 + "." + subsubversionPD9x + " (Build " + irefBuild + ")";
                }
            }
            else if (version == 0xA0)
            {
                // version removed
                if (subversionPD9x1 != 0 || subversionPD9x2 != 0)
                    return "10." + subversionPD9x1 + "." + subsubversionPD9x + " (Build " + irefBuild + ")";
                else
                    return "10.x (Build " + irefBuild + ")";
            }
            else
                return "7.6-10.x (Build " + irefBuild + ")";

            return "";
        }

        private string GetSafeDiscVersion(string file, int position)
        {
            int version;
            int subVersion;
            int subsubVersion;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position + 20, SeekOrigin.Begin); // Begin reading after "BoG_ *90.0&!!  Yy>" for old SafeDisc
            version = br.ReadInt32();
            subVersion = br.ReadInt32();
            subsubVersion = br.ReadInt32();
            if (version != 0)
                return version + "." + subVersion.ToString("00") + "." + subsubVersion.ToString("000");
            br.BaseStream.Seek(position + 18 + 14, SeekOrigin.Begin); // Begin reading after "BoG_ *90.0&!!  Yy>" for newer SafeDisc
            version = br.ReadInt32();
            subVersion = br.ReadInt32();
            subsubVersion = br.ReadInt32();
            br.Close();
            if (version == 0)
                return "";
            return version + "." + subVersion.ToString("00") + "." + subsubVersion.ToString("000");
        }

        private string GetSecuROM4Version(string file, int position)
        {
            char version;
            char subVersion1;
            char subVersion2;
            char subsubVersion1;
            char subsubVersion2;
            char subsubsubVersion1;
            char subsubsubVersion2;
            char subsubsubVersion3;
            char subsubsubVersion4;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream, Encoding.Default);
            br.BaseStream.Seek(position + 8, SeekOrigin.Begin); // Begin reading after "AddD"
            version = br.ReadChar();
            br.ReadByte();
            subVersion1 = br.ReadChar();
            subVersion2 = br.ReadChar();
            br.ReadByte();
            subsubVersion1 = br.ReadChar();
            subsubVersion2 = br.ReadChar();
            br.ReadByte();
            subsubsubVersion1 = br.ReadChar();
            subsubsubVersion2 = br.ReadChar();
            subsubsubVersion3 = br.ReadChar();
            subsubsubVersion4 = br.ReadChar();
            br.Close();
            return version + "." + subVersion1 + subVersion2 + "." + subsubVersion1 + subsubVersion2 + "." + subsubsubVersion1 + subsubsubVersion2 + subsubsubVersion3 + subsubsubVersion4;
        }

        private string GetSecuROM4and5Version(string file, int position)
        {
            byte version;
            byte subVersion1;
            byte subVersion2;
            byte subsubVersion1;
            byte subsubVersion2;
            byte subsubsubVersion1;
            byte subsubsubVersion2;
            byte subsubsubVersion3;
            byte subsubsubVersion4;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position + 8, SeekOrigin.Begin); // Begin reading after "ÊÝÝ¬"
            version = (byte)(br.ReadByte() & 0xF);
            br.ReadByte();
            subVersion1 = (byte)(br.ReadByte() ^ 36);
            subVersion2 = (byte)(br.ReadByte() ^ 28);
            br.ReadByte();
            subsubVersion1 = (byte)(br.ReadByte() ^ 42);
            subsubVersion2 = (byte)(br.ReadByte() ^ 8);
            br.ReadByte();
            subsubsubVersion1 = (byte)(br.ReadByte() ^ 16);
            subsubsubVersion2 = (byte)(br.ReadByte() ^ 116);
            subsubsubVersion3 = (byte)(br.ReadByte() ^ 34);
            subsubsubVersion4 = (byte)(br.ReadByte() ^ 22);
            br.Close();
            if (version == 0 || version > 9)
                return "";
            return version + "." + subVersion1 + subVersion2 + "." + subsubVersion1 + subsubVersion2 + "." + subsubsubVersion1 + subsubsubVersion2 + subsubsubVersion3 + subsubsubVersion4;
        }

        private string GetSecuROM7Version(string file)
        {
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(236, SeekOrigin.Begin);
            byte[] bytes = br.ReadBytes(4);
            // if bytes(0) = 0xED && bytes(3) = 0x5C {
            if (bytes[3] == 0x5C)
            {
                //SecuROM 7 new and 8
                br.Close();
                return (bytes[0] ^ 0xEA).ToString() + "." + (bytes[1] ^ 0x2C).ToString("00") + "." + (bytes[2] ^ 0x8).ToString("0000");
            }
            else // SecuROM 7 old
            {
                br.BaseStream.Seek(122, SeekOrigin.Begin);
                bytes = br.ReadBytes(2);
                br.Close();
                return "7." + (bytes[0] ^ 0x10).ToString("00") + "." + (bytes[1] ^ 0x10).ToString("0000");
                //return "7.01-7.10"
            }
        }

        private string GetSysiphusVersion(string file, int position)
        {
            char version;
            char subVersion;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream, Encoding.Default);
            br.BaseStream.Seek(position - 3, SeekOrigin.Begin);
            subVersion = br.ReadChar();
            br.ReadChar();
            version = br.ReadChar();
            br.Close();
            if (Char.IsNumber(version) && Char.IsNumber(subVersion))
                return version + "." + subVersion;
            else
                return "";
        }

        private string GetTagesVersion(string file, int position)
        {
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position + 7, SeekOrigin.Begin);
            byte bVersion = br.ReadByte();
            br.Close();
            switch(bVersion)
            {
                case 0x1B:
                    return "5.3-5.4";
                case 0x14:
                    return "5.5.0";
                case 0x4:
                    return "5.5.2";
            }
            return "";
        }

        private string GetVOBProtectCDDVDBuild(string file, int position)
        {
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream);
            br.BaseStream.Seek(position - 13, SeekOrigin.Begin);
            if (!Char.IsNumber(br.ReadChar()))
                return ""; //Build info removed
            br.BaseStream.Seek(position - 4, SeekOrigin.Begin);
            int build = br.ReadInt16();
            br.Close();
            return " (Build " + build + ")";
        }

        private string GetVOBProtectCDDVDVersion(string file, int position)
        {
            byte version;
            byte subVersion;
            byte subsubVersion;
            BinaryReader br = new BinaryReader(new StreamReader(file).BaseStream, Encoding.Default);
            br.BaseStream.Seek(position - 2, SeekOrigin.Begin);
            version = br.ReadByte();
            if (version == 5)
            {
                br.BaseStream.Seek(position - 4, SeekOrigin.Begin);
                subsubVersion = (byte)((br.ReadByte() & 0xF0) >> 4);
                subVersion = (byte)((br.ReadByte() & 0xF0) >> 4);
                br.Close();
                return version + "." + subVersion + "." + subsubVersion;
            }
            else
                return "";
        }

        #endregion

        /// <summary>
        /// Create a list of filenames and extensions mapped to protections for when only file existence matters
        /// </summary>
        /// <remarks>
        /// TODO: Create a case-insenstive dictionary for this since some filenames may have multiple cases
        /// </remarks>
        private static Dictionary<string, string> CreateFilenameProtectionMapping()
        {
            var mapping = new Dictionary<string, string>();

            // AACS
            mapping.Add("VTKF000.AACS", "AACS"); // Path.Combine("aacs", "VTKF000.AACS")
            mapping.Add("CPSUnit00001.cci", "AACS"); // Path.Combine("AACS", "CPSUnit00001.cci")

            // Alpha-DVD
            mapping.Add("PlayDVD.exe", "Alpha-DVD");

            // Bitpool
            mapping.Add("bitpool.rsc", "Bitpool");

            // ByteShield
            mapping.Add("Byteshield.dll", "ByteShield");
            mapping.Add(".bbz", "ByteShield");

            // Cactus Data Shield
            mapping.Add("yucca.cds", "Cactus Data Shield 200");
            mapping.Add("wmmp.exe", "Cactus Data Shield 200");
            mapping.Add("PJSTREAM.DLL", "Cactus Data Shield 200");
            mapping.Add("CACTUSPJ.exe", "Cactus Data Shield 200");
            mapping.Add("CDSPlayer.app", "Cactus Data Shield 200");

            // CD-Cops
            mapping.Add("CDCOPS.DLL", "CD-Cops");
            mapping.Add(".GZ_", "CD-Cops");
            mapping.Add(".W_X", "CD-Cops");
            mapping.Add(".Qz", "CD-Cops");
            mapping.Add(".QZ_", "CD-Cops");

            // CD-Lock
            mapping.Add(".AFP", "CD-Lock");

            // CD-Protector
            mapping.Add("_cdp16.dat", "CD-Protector");
            mapping.Add("_cdp16.dll", "CD-Protector");
            mapping.Add("_cdp32.dat", "CD-Protector");
            mapping.Add("_cdp32.dll", "CD-Protector");

            // CD-X
            mapping.Add("CHKCDX16.DLL", "CD-X");
            mapping.Add("CHKCDX32.DLL", "CD-X");
            mapping.Add("CHKCDXNT.DLL", "CD-X");

            // CopyKiller
            mapping.Add("Autorun.dat", "CopyKiller");

            // DiskGuard
            mapping.Add("IOSLINK.VXD", "DiskGuard");
            mapping.Add("IOSLINK.DLL", "DiskGuard");
            mapping.Add("IOSLINK.SYS", "DiskGuard");

            // DVD Crypt
            mapping.Add("DvdCrypt.pdb", "DVD Crypt");

            // DVD-Movie-PROTECT
            //mapping.Add("VIDEO_TS", "DVD-Movie-PROTECT"); // Directory name
            //mapping.Add(".bup", "DVD-Movie-PROTECT");
            //mapping.Add(".ifo", "DVD-Movie-PROTECT");

            // FreeLock
            mapping.Add("FREELOCK.IMG", "FreeLock");

            // Games for Windows - Live
            mapping.Add("XLiveRedist.msi", "Games for Windows - Live");

            // Hexalock AutoLock
            mapping.Add("Start_Here.exe", "Hexalock AutoLock");
            mapping.Add("HCPSMng.exe", "Hexalock AutoLock");
            mapping.Add("MFINT.DLL", "Hexalock AutoLock");
            mapping.Add("MFIMP.DLL", "Hexalock AutoLock");

            // Impulse Reactor
            mapping.Add("ImpulseReactor.dll", "Impulse Reactor");

            // IndyVCD
            mapping.Add("INDYVCD.AX", "IndyVCD");
            mapping.Add("INDYMP3.idt", "IndyVCD");

            // Key2Audio XS
            mapping.Add("SDKHM.EXE", "Key2Audio XS");
            mapping.Add("SDKHM.DLL", "Key2Audio XS");

            // LaserLock
            mapping.Add("NOMOUSE.SP", "LaserLock");
            mapping.Add("NOMOUSE.COM", "LaserLock");
            mapping.Add("l16dll.dll", "LaserLock");
            mapping.Add("laserlok.in", "LaserLock");
            mapping.Add("laserlok.o10", "LaserLock");
            mapping.Add("laserlok.011", "LaserLock");
            //mapping.Add("LASERLOK", "LaserLock"); // Directory name

            // MediaCloQ
            mapping.Add("sunncomm.ico", "MediaCloQ");

            // MediaMax CD-3
            mapping.Add("LaunchCd.exe", "MediaMax CD-3");

            // Origin
            mapping.Add("OriginSetup.exe", "Origin");

            // Protect DVD-Video
            //mapping.Add("VIDEO_TS", "Protect DVD-Video"); // Directory name
            // mapping.Add(".ifo", "Protect DVD-Video");

            // PSX LibCrypt - TODO: That's... not accurate
            mapping.Add(".cnf", "PSX LibCrypt");

            // SafeCast
            mapping.Add("cdac11ba.exe", "SafeCast");

            // SafeDisc
            mapping.Add("00000001.TMP", "SafeDisc 1-4");
            mapping.Add("CLCD16.DLL", "SafeDisc");
            mapping.Add("CLCD32.DLL", "SafeDisc");
            mapping.Add("CLOKSPL.EXE", "SafeDisc");
            mapping.Add("DPLAYERX.DLL", "SafeDisc");
            mapping.Add("drvmgt.dll", "SafeDisc 1-4");
            mapping.Add(".icd", "SafeDisc 1");
            mapping.Add(".016", "SafeDisc 1");
            mapping.Add(".256", "SafeDisc 1");

            // SafeDisc 2-4
            mapping.Add("00000002.TMP", "SafeDisc 2-4");
            mapping.Add("secdrv.sys", "SafeDisc 2-4");

            // SafeDisc Lite
            mapping.Add("00000001.LT1", "SafeDisc Lite");

            // SafeLock
            mapping.Add("SafeLock.dat", "SafeLock");
            mapping.Add("SafeLock.001", "SafeLock");
            mapping.Add("SafeLock.128", "SafeLock");

            // SecuROM
            mapping.Add("CMS16.DLL", "SecuROM");
            mapping.Add("CMS_95.DLL", "SecuROM");
            mapping.Add("CMS_NT.DLL", "SecuROM");
            mapping.Add("CMS32_95.DLL", "SecuROM");
            mapping.Add("CMS32_NT.DLL", "SecuROM");

            // SecuROM New
            mapping.Add("SINTF32.DLL", "SecuROM New");
            mapping.Add("SINTF16.DLL", "SecuROM New");
            mapping.Add("SINTFNT.DLL", "SecuROM New");

            // SmartE
            mapping.Add("00001.TMP", "SmartE");
            mapping.Add("00002.TMP", "SmartE");

            // SolidShield
            mapping.Add("dvm.dll", "SolidShield");
            mapping.Add("hc.dll", "SolidShield");
            mapping.Add("solidshield-cd.dll", "SolidShield");
            mapping.Add("c11prot.dll", "SolidShield");

            // Softlock
            mapping.Add("SOFTLOCKI.dat", "Softlock");
            mapping.Add("SOFTLOCKC.dat", "Softlock");

            // StarForce
            mapping.Add("protect.dll", "StarForce");
            mapping.Add("protect.exe", "StarForce");

            // Steam
            mapping.Add("SteamInstall.exe", "Steam");
            mapping.Add("SteamInstall.ini", "Steam");
            mapping.Add("SteamInstall.msi", "Steam");
            mapping.Add("SteamRetailInstaller.dmg", "Steam");
            mapping.Add("SteamSetup.exe", "Steam");

            // TAGES
            mapping.Add("Tages.dll", "TAGES");
            mapping.Add("tagesclient.exe", "TAGES");
            mapping.Add("TagesSetup.exe", "TAGES");
            mapping.Add("TagesSetup_x64.exe", "TAGES");
            mapping.Add("Wave.aif", "TAGES");

            // TZCopyProtector
            mapping.Add("_742893.016", "TZCopyProtector");

            // Uplay
            mapping.Add("UplayInstaller.exe", "Uplay");

            // VOB ProtectCD/DVD
            mapping.Add("VOB-PCD.KEY", "VOB ProtectCD/DVD");

            // Winlock
            mapping.Add("WinLock.PSX", "Winlock");

            // WTM CD Protect
            mapping.Add(".IMP", "WTM CD Protect");

            // WTM Copy Protection
            mapping.Add("imp.dat", "WTM Copy Protection");
            mapping.Add("wtmfiles.dat", "WTM Copy Protection");
            mapping.Add("Viewer.exe", "WTM Copy Protection");

            // XCP
            mapping.Add("XCP.DAT", "XCP");
            mapping.Add("ECDPlayerControl.ocx", "XCP");
            mapping.Add("go.exe", "XCP"); // Path.Combine("contents", "go.exe")

            return mapping;
        }

        /// <summary>
        /// Create a list of strings mapped to protections for when secondary strings and position doesn't matter
        /// </summary>
        private static Dictionary<string, string> CreateInternalProtectionMapping()
        {
            var mapping = new Dictionary<string, string>();

            // 3PLock
            mapping[".ldr"] = "3PLock";
            mapping[".ldt"] = "3PLock";
            // mapping["Y" + (char)0xC3 + "U" + (char)0x8B + (char)0xEC + (char)0x83 + (char)0xEC + "0SVW"] = "3PLock"; // YÃU‹ìƒì0SVW

            // ActiveMARK
            mapping["TMSAMVOF"] = "ActiveMARK";

            // ActiveMARK 5
            mapping[" " + (char)0xC2 + (char)0x16 + (char)0x00 + (char)0xA8 + (char)0xC1 + (char)0x16
                    + (char)0x00 + (char)0xB8 + (char)0xC1 + (char)0x16 + (char)0x00 + (char)0x86 + (char)0xC8 + (char)0x16 + (char)0x0
                    + (char)0x9A + (char)0xC1 + (char)0x16 + (char)0x00 + (char)0x10 + (char)0xC2 + (char)0x16 + (char)0x00] = "ActiveMARK 5";

            // Alpha-ROM
            mapping["SETTEC"] = "Alpha-ROM";

            // Armadillo
            mapping[".nicode" + (char)0x00] = "Armadillo";
            mapping["ARMDEBUG"] = "Armadillo";

            // CD-Cops
            mapping[".grand" + (char)0x00] = "CD-Cops";

            // CD-Lock
            mapping["2" + (char)0xF2 + (char)0x02 + (char)0x82 + (char)0xC3 + (char)0xBC + (char)0x0B
                    + "$" + (char)0x99 + (char)0xAD + "'C" + (char)0xE4 + (char)0x9D + "st"
                    + (char)0x99 + (char)0xFA + "2$" + (char)0x9D + ")4" + (char)0xFF + "t"] = "CD-Lock";

            // CDSHiELD SE
            mapping["~0017.tmp"] = "CDSHiELD SE";

            // Cenega ProtectDVD
            mapping[".cenega"] = "Cenega ProtectDVD";

            // Code Lock
            mapping["icd1" + (char)0x00] = "Code Lock";
            mapping["icd2" + (char)0x00] = "Code Lock";
            mapping["CODE-LOCK.OCX"] = "Code Lock";

            // CopyKiller
            mapping["Tom Commander"] = "CopyKiller";

            // EXE Stealth
            mapping["??[[__[[_" + (char)0x00 + "{{" + (char)0x0
                    + (char)0x00 + "{{" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x0
                    + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00 + "?;;??;;??"] = "EXE Stealth";

            // Games for Windows - Live
            mapping["xlive.dll"] = "Games for Windows - Live";

            // JoWooD X-Prot
            mapping["@HC09    "] = "JoWooD X-Prot v2";

            // Key-Lock (Dongle)
            mapping["KEY-LOCK COMMAND"] = "Key-Lock (Dongle)";

            // LaserLock
            mapping[":\\LASERLOK\\LASERLOK.IN" + (char)0x00 + "C:\\NOMOUSE.SP"] = "LaserLock 3";
            mapping["LASERLOK_INIT" + (char)0xC + "LASERLOK_RUN" + (char)0xE + "LASERLOK_CHECK"
                    + (char)0xF + "LASERLOK_CHECK2" + (char)0xF + "LASERLOK_CHECK3"] = "LaserLock 5";

            // Ring-Protech
            mapping[(char)0x00 + "Allocator" + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00] = "Ring-Protech";

            // SafeLock
            mapping["SafeLock"] = "SafeLock";

            // SecuROM
            mapping[".cms_t" + (char)0x00] = "SecuROM 1-3";
            mapping[".cms_d" + (char)0x00] = "SecuROM 1-3";

            // SmartE
            mapping["BITARTS"] = "SmartE";

            // SolidShield
            mapping["B" + (char)0x00 + "I" + (char)0x00 + "N" + (char)0x00 + (char)0x7 + (char)0x00 +
                    "I" + (char)0x00 + "D" + (char)0x00 + "R" + (char)0x00 + "_" + (char)0x00 +
                    "S" + (char)0x00 + "G" + (char)0x00 + "T" + (char)0x0] = "SolidShield";

            // SVK Protector
            mapping["?SVKP" + (char)0x00 + (char)0x00] = "SVK Protector";

            // Sysiphus / Sysiphus DVD
            mapping["V SUHPISYS"] = "Sysiphus"; // + Version
            mapping["V SUHPISYSDVD"] = "Sysiphus DVD"; // + Version

            // VOB ProtectCD/DVD
            mapping[".vob.pcd"] = "VOB ProtectCD";

            // WTM CD Protect
            mapping["WTM76545"] = "WTM CD Protect";

            // Xtreme-Protector
            mapping["XPROT   "] = "Xtreme-Protector";

            // CD Check
            //mapping["GetDriveType"] = "CD Check";
            //mapping["GetVolumeInformation"] = "CD Check";

            return mapping;
        }

        private string GetFileVersion(string file)
        {
            FileVersionInfo fvinfo = FileVersionInfo.GetVersionInfo(file);
            if (fvinfo.FileVersion == null)
                return "";
            if (fvinfo.FileVersion != "")
                return fvinfo.FileVersion.Replace(", ", ".");
            else
                return fvinfo.ProductVersion.Replace(", ", ".");
        }
    }
}
