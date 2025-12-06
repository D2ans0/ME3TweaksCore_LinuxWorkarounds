using AuthenticodeExaminer;
using LegendaryExplorerCore.Packages;
using ME3TweaksCore.Diagnostics.Support;
using ME3TweaksCore.GameFilesystem;
using ME3TweaksCore.Helpers;
using ME3TweaksCore.Helpers.ME1;
using ME3TweaksCore.Localization;
using ME3TweaksCore.Targets;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace ME3TweaksCore.Diagnostics.Modules
{
    /// <summary>
    /// Diagnostic module for collecting basic game information.
    /// </summary>
    internal class DiagGameInfo : DiagModuleBase
    {
        internal override void RunModule(LogUploadPackage package)
        {
            var diag = package.DiagnosticWriter;

            MLog.Information(@"Collecting basic game information");

            package.UpdateStatusCallback?.Invoke(LC.GetString(LC.string_collectingGameInformation));
            diag.AddDiagLine(@"Basic game information", LogSeverity.DIAGSECTION);
            diag.AddDiagLine($@"Game is installed at {package.DiagnosticTarget.TargetPath}");

            MLog.Information(@"Reloading target for most up to date information");
            package.DiagnosticTarget.ReloadGameTarget(false); //reload vars
            TextureModInstallationInfo avi = package.DiagnosticTarget.GetInstalledALOTInfo();

            string exePath = M3Directories.GetExecutablePath(package.DiagnosticTarget);
            if (File.Exists(exePath))
            {
                MLog.Information(@"Getting game version");
                var versInfo = FileVersionInfo.GetVersionInfo(exePath);
                diag.AddDiagLine($@"Version: {versInfo.FileMajorPart}.{versInfo.FileMinorPart}.{versInfo.FileBuildPart}.{versInfo.FilePrivatePart}");

                //Disk type
                string pathroot = Path.GetPathRoot(package.DiagnosticTarget.TargetPath);
                pathroot = pathroot.Substring(0, 1);
                if (pathroot == @"\")
                {
                    diag.AddDiagLine(@"Installation appears to be on a network drive (first character in path is \)", LogSeverity.WARN);
                }
                else
                {
                    if (MUtilities.IsWindows10OrNewer())
                    {
                        int backingType = GetPartitionDiskBackingType(pathroot);
                        string type = @"Unknown type";
                        switch (backingType)
                        {
                            case 3:
                                type = @"Hard disk drive";
                                break;
                            case 4:
                                type = @"Solid state drive";
                                break;
                            default:
                                type += @": " + backingType;
                                break;
                        }

                        diag.AddDiagLine(@"Installed on disk type: " + type);
                    }
                }

                if (package.DiagnosticTarget.Supported)
                {
                    diag.AddDiagLine($@"Game source: {package.DiagnosticTarget.GameSource}", LogSeverity.GOOD);
                }
                else
                {
                    diag.AddDiagLine($@"Game source: Unknown/Unsupported - {package.DiagnosticTarget.ExecutableHash}", LogSeverity.FATAL);
                }

                if (package.DiagnosticTarget.Game == MEGame.ME1)
                {
                    MLog.Information(@"Getting additional ME1 executable information");
                    var exeInfo = ME1ExecutableInfo.GetExecutableInfo(M3Directories.GetExecutablePath(package.DiagnosticTarget), false);
                    if (avi != null)
                    {
                        diag.AddDiagLine($@"Large Address Aware: {exeInfo.HasLAAApplied}", exeInfo.HasLAAApplied ? LogSeverity.GOOD : LogSeverity.FATAL);
                        diag.AddDiagLine($@"No-admin patched: {exeInfo.HasLAAApplied}", exeInfo.HasProductNameChanged ? LogSeverity.GOOD : LogSeverity.WARN);
                        diag.AddDiagLine($@"enableLocalPhysXCore patched: {exeInfo.HasPhysXCoreChanged}", exeInfo.HasLAAApplied ? LogSeverity.GOOD : LogSeverity.WARN);
                    }
                    else
                    {
                        diag.AddDiagLine($@"Large Address Aware: {exeInfo.HasLAAApplied}");
                        diag.AddDiagLine($@"No-admin patched: {exeInfo.HasLAAApplied}");
                        diag.AddDiagLine($@"enableLocalPhysXCore patched: {exeInfo.HasLAAApplied}");
                    }
                }

                //Executable signatures
                MLog.Information(@"Checking executable signature");

                var info = new FileInspector(exePath);
                var certOK = info.Validate();
                if (certOK == SignatureCheckResult.NoSignature)
                {
                    diag.AddDiagLine(@"This executable is not signed", LogSeverity.ERROR);
                }
                else
                {
                    if (certOK == SignatureCheckResult.BadDigest)
                    {
                        if (package.DiagnosticTarget.Game == MEGame.ME1 && versInfo.ProductName == @"Mass_Effect")
                        {
                            //Check if this Mass_Effect
                            diag.AddDiagLine(@"Signature check for this executable was skipped as MEM modified this exe");
                        }
                        else
                        {
                            diag.AddDiagLine(@"The signature for this executable is not valid. The executable has been modified", LogSeverity.ERROR);
                            diagPrintSignatures(info, package);
                        }
                    }
                    else
                    {
                        diag.AddDiagLine(@"Signature check for this executable: " + certOK.ToString());
                        diagPrintSignatures(info, package);
                    }
                }

                //BINK
                MLog.Information(@"Checking if Bink ASI loader is installed");

                if (package.DiagnosticTarget.IsBinkBypassInstalled())
                {
                    if (package.DiagnosticTarget.Game.IsOTGame())
                    {
                        diag.AddDiagLine(@"binkw32 ASI bypass is installed");
                    }
                    else
                    {
                        diag.AddDiagLine(@"bink2w64 ASI loader is installed");
                    }
                }
                else
                {
                    if (package.DiagnosticTarget.Game.IsOTGame())
                    {
                        diag.AddDiagLine(@"binkw32 ASI bypass is not installed. ASI mods, DLC mods, and modified DLC will not load", LogSeverity.WARN);
                    }
                    else
                    {
                        diag.AddDiagLine(@"bink2w64 ASI loader is not installed. ASI mods will not load", LogSeverity.WARN);
                    }
                }

                bool enhancedBinkInstalled = false;
                if (package.DiagnosticTarget.Game.IsLEGame() || package.DiagnosticTarget.Game == MEGame.LELauncher)
                {
                    // ME3Tweakscore 8.1.0 for LE: Enhanced bink2 encoder
                    enhancedBinkInstalled = package.DiagnosticTarget.IsEnhancedBinkInstalled();
                    if (enhancedBinkInstalled)
                    {
                        diag.AddDiagLine(@"Enhanced bink dll is installed", LogSeverity.GOOD);
                    }
                    else
                    {
                        diag.AddDiagLine(@"Standard bink dll is installed");
                    }
                }


                if (package.DiagnosticTarget.Game == MEGame.ME1)
                {
                    // Check for patched PhysX
                    if (ME1PhysXTools.IsPhysXLoaderPatchedLocalOnly(package.DiagnosticTarget))
                    {
                        diag.AddDiagLine(@"PhysXLoader.dll is patched to force local PhysXCore.dll", LogSeverity.GOOD);
                    }
                    else if (certOK == SignatureCheckResult.BadDigest)
                    {
                        diag.AddDiagLine(@"PhysXLoader.dll is not patched to force local PhysXCore.dll. Game may not boot", LogSeverity.WARN);
                    }
                    else if (certOK == SignatureCheckResult.Valid)
                    {
                        diag.AddDiagLine(@"PhysXLoader.dll is not patched, but executable is still signed", LogSeverity.GOOD);
                    }
                    else
                    {
                        diag.AddDiagLine(@"PhysXLoader.dll status could not be checked", LogSeverity.WARN);
                    }
                }

                package.DiagnosticTarget.PopulateExtras();
                if (package.DiagnosticTarget.ExtraFiles.Any())
                {
                    diag.AddDiagLine(@"Additional dll files found in game executable directory:", LogSeverity.WARN);
                    foreach (var extra in package.DiagnosticTarget.ExtraFiles)
                    {
                        diag.AddDiagLine(@" > " + extra.DisplayName);
                    }
                }
            }
        }


        private static string getSignerSubject(string subject)
        {
            // Get Common Name (CN)
            var props = StringStructParser.GetCommaSplitValues($"({subject})"); // do not localize
            return props[@"CN"];
        }

        private static void diagPrintSignatures(FileInspector info, LogUploadPackage package)
        {
            foreach (var sig in info.GetSignatures())
            {
                var signingTime = sig.TimestampSignatures.FirstOrDefault()?.TimestampDateTime?.UtcDateTime;
                package.DiagnosticWriter.AddDiagLine(@" > Signed on " + signingTime, LogSeverity.INFO);

                bool isFirst = true;
                foreach (var signChain in sig.AdditionalCertificates)
                {
                    try
                    {
                        package.DiagnosticWriter.AddDiagLine($@" >> {(isFirst ? @"Signed" : @"Countersigned")} by {getSignerSubject(signChain.Subject)}", LogSeverity.INFO); // do not localize
                    }
                    catch
                    {
                        package.DiagnosticWriter.AddDiagLine($@"  >> {(isFirst ? "Signed" : "Countersigned")} by " + signChain.Subject, LogSeverity.INFO); // do not localize
                    }

                    isFirst = false;
                }
            }
        }


        private static int GetPartitionDiskBackingType(string partitionLetter)
        {
            using (var partitionSearcher = new ManagementObjectSearcher(
                @"\\localhost\ROOT\Microsoft\Windows\Storage",
                $@"SELECT DiskNumber FROM MSFT_Partition WHERE DriveLetter='{partitionLetter}'"))
            {
                try
                {
                    var partition = partitionSearcher.Get().Cast<ManagementBaseObject>().Single();
                    using (var physicalDiskSearcher = new ManagementObjectSearcher(
                        @"\\localhost\ROOT\Microsoft\Windows\Storage",
                        $@"SELECT Size, Model, MediaType FROM MSFT_PhysicalDisk WHERE DeviceID='{partition[@"DiskNumber"]}'")) //do not localize
                    {
                        var physicalDisk = physicalDiskSearcher.Get().Cast<ManagementBaseObject>().Single();
                        return (UInt16)physicalDisk[@"MediaType"];
                    }
                }
                catch (Exception e)
                {
                    MLog.Warning($@"Error reading partition type on {partitionLetter}: {e.Message}. This may be an expected error due to how WMI works");
                    return -1;
                }
            }
        }
    }
}
