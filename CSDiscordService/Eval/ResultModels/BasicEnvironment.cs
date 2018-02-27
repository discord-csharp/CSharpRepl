using System;
using System.Collections;

namespace CSDiscordService.Eval.ResultModels
{
    public class BasicEnvironment
    {
        public string CommandLine { get; } = @"C:\thing.exe --do-stuff";
        public string CurrentDirectory { get; set; } = @"C:\";
        public int CurrentManagedThreadId { get; } = Environment.CurrentManagedThreadId;
        public int ExitCode { get; set; }
        public bool HasShutdownStarted { get; } = false;
        public bool Is64BitOperatingSystem { get; } = Environment.Is64BitOperatingSystem;
        public bool Is64BitProcess { get; } = Environment.Is64BitProcess;
        public string MachineName { get; } = "SOMEONE-ELSES-MACHINE";
        public string NewLine { get; } = Environment.NewLine;
        public OperatingSystem OSVersion { get; } = Environment.OSVersion;
        public int ProcessorCount { get; } = Environment.ProcessorCount;
        public string StackTrace { get; } = string.Empty;
        public string SystemDirectory { get; } = Environment.SystemDirectory;
        public int SystemPageSize { get; } = Environment.SystemPageSize;
        public int TickCount { get; } = Environment.TickCount;
        public string UserDomainName { get; } = "SOMEONE-ELSES-MACHINE";
        public bool UserInteractive { get; } = false;
        public string UserName { get; } = "SomeoneElse";
        public Version Version { get; } = Environment.Version;
        public long WorkingSet { get; } = Environment.WorkingSet;

        private const string AccessDenied = "Usage of this API is prohibited";

        public void Exit(int exitCode) => throw new MethodAccessException(AccessDenied);
        public string ExpandEnvironmentVariables(string name) => throw new MethodAccessException(AccessDenied);
        public void FailFast(string message) => throw new Exception(message);
        public void FailFast(string message, Exception exception) => throw exception;
        public string[] GetCommandLineArgs() => CommandLine.Split(' ');
        public string GetEnvironmentVariable(string variable) => throw new MethodAccessException(AccessDenied);
        public string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target) => throw new MethodAccessException(AccessDenied);
        public IDictionary GetEnvironmentVariables() => throw new MethodAccessException(AccessDenied);
        public IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target) => throw new MethodAccessException(AccessDenied);
        public string GetFolderPath(SpecialFolder folder) => Environment.GetFolderPath((Environment.SpecialFolder)folder);
        public string GetFolderPath(SpecialFolder folder, SpecialFolderOption option) => Environment.GetFolderPath((Environment.SpecialFolder)folder, (Environment.SpecialFolderOption)option);
        public string[] GetLogicalDrives => Environment.GetLogicalDrives();
        public void SetEnvironmentVariable(string variable, string value) => throw new MethodAccessException(AccessDenied);
        public void SetEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target) => throw new MethodAccessException(AccessDenied);

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
