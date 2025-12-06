using Flurl.Http;
using LegendaryExplorerCore.Compression;
using LegendaryExplorerCore.Gammtek.Extensions;
using LegendaryExplorerCore.Packages;
using ME3TweaksCore.Diagnostics.Modules;
using ME3TweaksCore.Diagnostics.Support;
using ME3TweaksCore.Helpers;
using ME3TweaksCore.Localization;
using ME3TweaksCore.Misc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace ME3TweaksCore.Diagnostics
{
    /// <summary>
    /// LogCollector is used to collect logs from the application and game for upload or viewing
    /// </summary>
    public class LogCollector
    {


        /// <summary>
        /// Gets a list of available log files in the log directory
        /// </summary>
        /// <param name="activeLogPath"></param>
        /// <returns></returns>
        public static List<LogItem> GetLogsList(string activeLogPath = null)
        {
            var logs = Directory.GetFiles(MCoreFilesystem.GetLogDir(), @"*.txt");
            return logs.Select(x => new LogItem(x)
            {
                IsActiveLog = x == activeLogPath
            }).ToList();
        }

        /// <summary>
        /// Collects an application log file and reopens the logger when complete
        /// </summary>
        /// <param name="logfile"></param>
        /// <returns></returns>
        public static string CollectLogs(string logfile)
        {
            MLog.Information(@"Shutting down logger to allow application to pull log file.");
            Log.CloseAndFlush();
            try
            {
                string log = File.ReadAllText(logfile);
                CreateLogger();
                return log;
            }
            catch (Exception e)
            {
                CreateLogger();
                MLog.Error(@"Could not read log file! " + e.Message);
                return null;
            }
        }
        /// <summary>
        /// ILogger creation delegate that is invoked when reopening the logger after collecting logs.
        /// </summary>
        internal static Func<ILogger> CreateLogger { get; set; }

        // Following is an example CreateLogger call that can be used in consuming applications.
        //        internal static void CreateLogger()
        //        {
        //            Log.Logger = new LoggerConfiguration().WriteTo.SizeRollingFile(Path.Combine(App.LogDir, @"modmanagerlog.txt"),
        //                                    retainedFileDurationLimit: TimeSpan.FromDays(14),
        //                                    fileSizeLimitBytes: 1024 * 1024 * 10) // 10MB  
        //#if DEBUG
        //                .WriteTo.Debug()
        //#endif
        //                .CreateLogger();
        //        }


        /// <summary>
        /// Collects the latest log file and reopens the logger when complete (if specified)
        /// </summary>
        /// <param name="logdir"></param>
        /// <param name="restartLogger"></param>
        /// <returns></returns>
        public static string CollectLatestLog(string logdir, bool restartLogger)
        {
            MLog.Information(@"Closing application log to allow application to read log file");
            Log.CloseAndFlush();
            var logFile = new DirectoryInfo(logdir)
                                             .GetFiles(@"*.txt")
                                             .OrderByDescending(f => f.LastWriteTime)
                                             .FirstOrDefault();
            string logText = null;
            if (logFile != null && File.Exists(logFile.FullName))
            {
                try
                {
                    logText = File.ReadAllText(logFile.FullName);
                }
                catch (Exception e)
                {
                    MLog.Fatal($@"UNABLE TO READ LOG FILE {logFile.FullName}: {e.Message}");
                }
            }

            if (restartLogger)
            {
                CreateLogger?.Invoke();
            }
            return logText;
        }

        /// <summary>
        /// Collects the game diagnostic information and places it into the given package
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static string PerformDiagnostic(LogUploadPackage package)
        {
            var diag = package.DiagnosticWriter;

            MLog.Information($@"Collecting diagnostics for target {package.DiagnosticTarget.TargetPath}");
            package.UpdateStatusCallback?.Invoke(LC.GetString(LC.string_preparingToCollectDiagnosticInfo));
            package.UpdateTaskbarProgressStateCallback?.Invoke(MTaskbarState.Indeterminate);

            #region Diagnostic setup and diag header
            package.UpdateStatusCallback?.Invoke(LC.GetString(LC.string_collectingGameInformation));

            MLog.Information(@"Beginning to build diagnostic output");

            diag.AddDiagLine(package.DiagnosticTarget.Game.ToGameNum().ToString(), LogSeverity.GAMEID);
            if (package.SelectedSaveFilePath != null && File.Exists(package.SelectedSaveFilePath))
            {
                // This will allow server to locate the save file that is uploaded and tell user what it is 
                diag.AddDiagLine(MUtilities.CalculateHash(package.SelectedSaveFilePath) + @"|" + Path.GetFileName(package.SelectedSaveFilePath), LogSeverity.SAVE_FILE_HASH_NAME);
            }
            diag.AddDiagLine($@"{MLibraryConsumer.GetHostingProcessname()} {MLibraryConsumer.GetAppVersion()} Game Diagnostic");
            diag.AddDiagLine($@"Build date: {MLibraryConsumer.GetSigningDate()}");
            diag.AddDiagLine($@"ME3TweaksCore version: {MLibraryConsumer.GetLibraryVersion()}");
            diag.AddDiagLine($@"Diagnostic for {package.DiagnosticTarget.Game.ToGameName()}");
            diag.AddDiagLine($@"Diagnostic generated at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            #endregion

            // List of modules to run for diagnostics, in the order they appear in the report.
            var modules = new List<DiagModuleBase>()
                {
                    new DiagGameInfo(),
                    new DiagSystemInfo(),
                    new DiagASIInfo(),
                    new DiagBasegameFiles(),
                    new DiagInstalledDLC(),
                    new DiagBTPVerify(),
                    new DiagTFCInfo(),
                    new DiagMEM(), // This also checks for extended in it
                    new DiagTOC(),
                    new DiagEventLog(),
                    new DiagME1Logs(),
                    new DiagME3Logger(),
                    new DiagExtended(),
                };

            foreach (var module in modules)
            {
                try
                {
                    module.RunModule(package);
                }
                catch (Exception ex)
                {
                    MLog.Error($@"An exception occurred running diagnostic module {module.GetType().Name}: {ex.Message}");
                    diag.AddDiagLine($@"An exception occurred running diagnostic module {module.GetType().Name}: {ex.Message}", LogSeverity.ERROR);
                }
                finally
                {
                    // Run cleanup, always
                    module.PostRunModule(package);
                }
            }

            // We have to strip any null terminators or it will bork it on the server log viewer
            return diag.GetDiagnosticText().Replace("\0", @""); // do not localize
        }

        /// <summary>
        /// Application session divider for logs. Should always be the very first line of a new session. Changing this will break the server side log viewer's ability to see different sessions.
        /// </summary>
        public static string SessionStartString { get; } = @"============================SESSION START============================";

        /// <summary>
        /// Submits the given diagnostic log package to the ME3Tweaks Log Viewing service.
        /// </summary>
        public static string SubmitDiagnosticLog(LogUploadPackage package)
        {
            StringBuilder logUploadText = new StringBuilder();
            if (package.DiagnosticTarget != null && !package.DiagnosticTarget.IsCustomOption && (package.DiagnosticTarget.Game.IsOTGame() || package.DiagnosticTarget.Game.IsLEGame()))
            {
                Debug.WriteLine(@"Selected game target: " + package.DiagnosticTarget.TargetPath);
                logUploadText.Append("[MODE]diagnostics\n"); //do not localize
                logUploadText.Append(LogCollector.PerformDiagnostic(package));
                logUploadText.Append("\n"); //do not localize
            }

            if (package.SelectedLog != null && package.SelectedLog.Selectable)
            {
                Debug.WriteLine(@"Selected log: " + package.SelectedLog.filepath);
                logUploadText.Append("[MODE]logs\n"); //do not localize
                logUploadText.AppendLine(LogCollector.CollectLogs(package.SelectedLog.filepath));
                logUploadText.Append("\n"); //do not localize
            }

            var logtext = logUploadText.ToString();
            package.UpdateStatusCallback?.Invoke(LC.GetString(LC.string_compressingForUpload));
            var lzmalog = LZMA.CompressToLZMAFile(Encoding.UTF8.GetBytes(logtext));
            try
            {
                //this doesn't need to technically be async, but library doesn't have non-async method.
                package.UpdateStatusCallback?.Invoke(LC.GetString(LC.string_uploadingToME3Tweaks));

                dynamic data = new ExpandoObject();
                IDictionary<string, object> dictionary = (IDictionary<string, object>)data;

                dictionary.Add(@"LogData", Convert.ToBase64String(lzmalog));
                if (package.Attachments != null)
                {
                    foreach (var attachment in package.Attachments)
                    {
                        // For save files this will be a filename ending with .pcsav
                        dictionary.Add(attachment.Key, Convert.ToBase64String(attachment.Value));
                    }
                }
                dictionary.Add(@"ToolName", MLibraryConsumer.GetHostingProcessname());
                dictionary.Add(@"ToolVersion", MLibraryConsumer.GetAppVersion());

                //10/22/2023 - Change to unified endpoint
                string responseString = @"https://me3tweaks.com/modmanager/logservice/shared/logupload".PostUrlEncodedAsync(dictionary)

                    //new
                    //{
                    //    LogData = Convert.ToBase64String(lzmalog),
                    //    Attachments = package.Attachments,
                    //    ToolName = MLibraryConsumer.GetHostingProcessname(),
                    //    ToolVersion = MLibraryConsumer.GetAppVersion()
                    //})
                    .ReceiveString().Result;
                Uri uriResult;
                bool result = Uri.TryCreate(responseString, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result)
                {
                    //should be valid URL.
                    MLog.Information(@"Result from server for log upload: " + responseString);
                    return responseString;
                }
                else
                {
                    MLog.Error(@"Error uploading log. The server responded with: " + responseString);
                    return LC.GetString(LC.string_interp_serverRejectedTheUpload, responseString);
                }
            }
            catch (AggregateException e)
            {
                Exception ex = e.InnerException;
                string exmessage = ex.Message;
                return LC.GetString(LC.string_interp_logWasUnableToUpload, exmessage);
            }
            catch (FlurlHttpTimeoutException)
            {
                // FlurlHttpTimeoutException derives from FlurlHttpException; catch here only
                // if you want to handle timeouts as a special case
                MLog.Error(@"Request timed out while uploading log.");
                return LC.GetString(LC.string_interp_requestTimedOutUploading);

            }
            catch (Exception ex)
            {
                // ex.Message contains rich details, including the URL, verb, response status,
                // and request and response bodies (if available)
                MLog.Exception(ex, @"Handled error uploading log: ");
                string exmessage = ex.Message;
                var index = exmessage.IndexOf(@"Request body:");
                if (index > 0)
                {
                    exmessage = exmessage.Substring(0, index);
                }

                return LC.GetString(LC.string_interp_logWasUnableToUpload, exmessage);
            }
        }
    }
}

