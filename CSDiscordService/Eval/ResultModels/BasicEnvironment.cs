using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSDiscordService.Eval.ResultModels
{
    public class BasicEnvironment
    {
        public string CommandLine { get; } = Environment.CommandLine;
        public string CurrentDirectory { get; set; } = Environment.CurrentDirectory;
        public int CurrentManagedThreadId { get; } = Environment.CurrentManagedThreadId;
        public int ExitCode { get; set; } = Environment.ExitCode;
        public bool HasShutdownStarted { get; } = Environment.HasShutdownStarted;
        public bool Is64BitOperatingSystem { get; } = Environment.Is64BitOperatingSystem;
        public bool Is64BitProcess { get; } = Environment.Is64BitProcess;
        public string MachineName { get; } = Environment.MachineName;
        public string NewLine { get; } = Environment.NewLine;
        public OperatingSystem OSVersion { get; } = Environment.OSVersion;
        public int ProcessorCount { get; } = Environment.ProcessorCount;
        public string StackTrace { get; } = Environment.StackTrace;
        public string SystemDirectory { get; } = Environment.SystemDirectory;
        public int SystemPageSize { get; } = Environment.SystemPageSize;
        public int TickCount { get; } = Environment.TickCount;
        public string UserDomainName { get; } = Environment.UserDomainName;
        public bool UserInteractive { get; } = Environment.UserInteractive;
        public string UserName { get; } = Environment.UserName;
        public Version Version { get; } = Environment.Version;
        public long WorkingSet { get; } = Environment.WorkingSet;

        // The last two are marked static to reproduce a real environment, where user and machine variables persists between executions.
        private Dictionary<string, string> _processEnv = new Dictionary<string, string>();
        private static Dictionary<string, string> _userEnv = new Dictionary<string, string>();
        private static Dictionary<string, string> _machineEnv = new Dictionary<string, string>();


        public void Exit(int exitCode) => throw new ExitException(exitCode);
        public void FailFast(string message, Exception exception = null) => throw new FailFastException(message, exception);
        public string[] GetCommandLineArgs() => Environment.GetCommandLineArgs();
        public string GetFolderPath(SpecialFolder folder, SpecialFolderOption option = SpecialFolderOption.None) => Environment.GetFolderPath((Environment.SpecialFolder)folder, (Environment.SpecialFolderOption)option);
        public string[] GetLogicalDrives => Environment.GetLogicalDrives();

        public string ExpandEnvironmentVariables(string name)
        {
            var env = (Dictionary<string, string>)GetEnvironmentVariables();
            var regex = new Regex("%([^%]+)%");
            var me = new MatchEvaluator(m => env.ContainsKey(m.Groups[1].Value) ? env[m.Groups[1].Value] : m.Value);

            return regex.Replace(name, me);
        }

        public void SetEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            if (target == EnvironmentVariableTarget.Process)
            {
                _processEnv[variable] = value;
            }
            else if (target == EnvironmentVariableTarget.User)
            {
                _userEnv[variable] = value;
            }
            else if (target == EnvironmentVariableTarget.Machine)
            {
                _machineEnv[variable] = value;
            }
        }
        
        public string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            var searchDictionary = GetEnvironmentVariables(target);

            return searchDictionary.Contains(variable) ? (string)searchDictionary[variable] : null;
        }

        public IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            Dictionary<string, string> searchDictionary = _machineEnv;
            if (target != EnvironmentVariableTarget.Machine)
            {
                foreach (var kvp in _userEnv)
                {
                    searchDictionary[kvp.Key] = kvp.Value;
                }
            }
            if (target == EnvironmentVariableTarget.Process)
            {
                foreach (var kvp in _processEnv)
                {
                    searchDictionary[kvp.Key] = kvp.Value;
                }
            }

            return searchDictionary;
        }


        public enum SpecialFolder
        {
            Desktop = Environment.SpecialFolder.Desktop,
            Programs = Environment.SpecialFolder.Programs,
            MyDocuments = Environment.SpecialFolder.MyDocuments,
            Favorites = Environment.SpecialFolder.Favorites,
            Startup = Environment.SpecialFolder.Startup,
            Recent = Environment.SpecialFolder.Recent,
            SendTo = Environment.SpecialFolder.SendTo,
            StartMenu = Environment.SpecialFolder.StartMenu,
            MyMusic = Environment.SpecialFolder.MyMusic,
            MyVideos = Environment.SpecialFolder.MyVideos,
            DesktopDirectory = Environment.SpecialFolder.DesktopDirectory,
            MyComputer = Environment.SpecialFolder.MyComputer,
            NetworkShortcuts = Environment.SpecialFolder.NetworkShortcuts,
            Fonts = Environment.SpecialFolder.Fonts,
            Templates = Environment.SpecialFolder.Templates,
            CommonStartMenu = Environment.SpecialFolder.CommonStartMenu,
            CommonPrograms = Environment.SpecialFolder.CommonPrograms,
            CommonStartup = Environment.SpecialFolder.CommonStartup,
            CommonDesktopDirectory = Environment.SpecialFolder.CommonDesktopDirectory,
            ApplicationData = Environment.SpecialFolder.ApplicationData,
            PrinterShortcuts = Environment.SpecialFolder.PrinterShortcuts,
            LocalApplicationData = Environment.SpecialFolder.LocalApplicationData,
            InternetCache = Environment.SpecialFolder.InternetCache,
            Cookies = Environment.SpecialFolder.Cookies,
            History = Environment.SpecialFolder.History,
            CommonApplicationData = Environment.SpecialFolder.CommonApplicationData,
            Windows = Environment.SpecialFolder.Windows,
            System = Environment.SpecialFolder.System,
            ProgramFiles = Environment.SpecialFolder.ProgramFiles,
            MyPictures = Environment.SpecialFolder.MyPictures,
            UserProfile = Environment.SpecialFolder.UserProfile,
            SystemX86 = Environment.SpecialFolder.SystemX86,
            ProgramFilesX86 = Environment.SpecialFolder.ProgramFilesX86,
            CommonProgramFiles = Environment.SpecialFolder.CommonProgramFiles,
            CommonProgramFilesX86 = Environment.SpecialFolder.CommonProgramFilesX86,
            CommonTemplates = Environment.SpecialFolder.CommonTemplates,
            CommonDocuments = Environment.SpecialFolder.CommonDocuments,
            CommonAdminTools = Environment.SpecialFolder.CommonAdminTools,
            AdminTools = Environment.SpecialFolder.AdminTools,
            CommonMusic = Environment.SpecialFolder.CommonMusic,
            CommonPictures = Environment.SpecialFolder.CommonPictures,
            CommonVideos = Environment.SpecialFolder.CommonVideos,
            Resources = Environment.SpecialFolder.Resources,
            LocalizedResources = Environment.SpecialFolder.LocalizedResources,
            CommonOemLinks = Environment.SpecialFolder.CommonOemLinks,
            CDBurning = Environment.SpecialFolder.CDBurning
        }

        public enum SpecialFolderOption
        {
            Create = Environment.SpecialFolderOption.Create,
            DoNotVerify = Environment.SpecialFolderOption.DoNotVerify,
            None = Environment.SpecialFolderOption.None
        }
    }
}
