﻿namespace Framework.Cli
{
    using Framework.Cli.Command;
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class UtilCli
    {
        /// <summary>
        /// Run dotnet command.
        /// </summary>
        internal static void DotNet(string workingDirectory, string arguments, bool isWait = true)
        {
            Start(workingDirectory, "dotnet", arguments, isWait: isWait);
        }

        /// <summary>
        /// Run npm command.
        /// </summary>
        internal static void Npm(string workingDirectory, string arguments)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UtilCli.Start(workingDirectory, "cmd", "/c npm.cmd " + arguments);
            }
            else
            {
                UtilCli.Start(workingDirectory, "npm", arguments);
            }
        }

        /// <summary>
        /// Start script.
        /// </summary>
        /// <param name="isRedirectStdErr">If true, do not write to stderr. Use this flag if shell command is known to write info (mistakenly) to stderror.</param>
        internal static void Start(string workingDirectory, string fileName, string arguments, bool isWait = true, bool isRedirectStdErr = false)
        {
            string time = UtilFramework.DateTimeToString(DateTime.Now);
            UtilCli.ConsoleWriteLinePassword(string.Format("### {4} Process Begin (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.Green);

            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = workingDirectory;
            info.FileName = fileName;
            info.Arguments = arguments;
            if (isRedirectStdErr)
            {
                info.RedirectStandardError = true; // Do not write to stderr.
            }
            // info.UseShellExecute = true;
            var process = Process.Start(info);
            if (isWait)
            {
                process.WaitForExit();
                if (isRedirectStdErr)
                {
                    string errorText = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        UtilCli.ConsoleWriteLinePassword(string.Format("### {4} Process StdErr (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.DarkGreen); // Write stderr to stdout.
                        UtilCli.ConsoleWriteLinePassword(errorText, ConsoleColor.DarkGreen); // Log DarkGreen because it is not treated like an stderr output.
                    }
                }
                if (process.ExitCode != 0)
                {
                    throw new Exception("Script failed!");
                }
            }

            UtilCli.ConsoleWriteLinePassword(string.Format("### {4} Process End (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.DarkGreen);
        }

        /// <summary>
        /// Returns stdout of command.
        /// </summary>
        internal static string StartStdout(string workingDirectory, string fileName, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = workingDirectory;
            info.FileName = fileName;
            info.Arguments = arguments;
            info.RedirectStandardOutput = true; // Do not write to stdout.
            var process = Process.Start(info);
            process.WaitForExit();
            string result = process.StandardOutput.ReadToEnd();
            if (process.ExitCode != 0)
            {
                throw new Exception("Script failed!");
            }

            return result;
        }

        internal static void OpenWebBrowser(string url)
        {
            Start(null, "cmd", $"/c start {url}", isWait: false);
        }

        internal static void FolderNameDelete(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                foreach (FileInfo fileInfo in new DirectoryInfo(folderName).GetFiles("*.*", SearchOption.AllDirectories))
                {
                    fileInfo.Attributes = FileAttributes.Normal; // See also: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it/30673648
                }
                Directory.Delete(folderName, true);
            }
        }

        /// <summary>
        /// Adjustment for: If sequence of arguments is passed different than defined in CommandLineApplication values are wrong.
        /// </summary>
        /// <param name="commandBase"></param>
        /// <param name="commandArgument"></param>
        /// <returns></returns>
        private static CommandArgument ArgumentValue(CommandBase command, CommandArgument commandArgument)
        {
            CommandArgument result = null;
            foreach (CommandArgument item in command.Configuration.Arguments)
            {
                string name = item.Value;
                if (name?.IndexOf("=") != -1)
                {
                    name = name?.Substring(0, name.IndexOf("="));
                }
                if (name?.ToLower() == commandArgument.Name.ToLower())
                {
                    result = item;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true, if argument is used in command line.
        /// </summary>
        internal static bool ArgumentValueIsExist(CommandBase command, CommandArgument commandArgument)
        {
            commandArgument = ArgumentValue(command, commandArgument); // Sequence of passed arguments might be wrong.

            bool result = commandArgument != null && commandArgument.Value != null;
            return result;
        }

        /// <summary>
        /// Returns true, if value has been set. (Use Argument=null to set a value to null).
        /// </summary>
        /// <param name="value">Returns value.</param>
        internal static bool ArgumentValue(CommandBase command, CommandArgument commandArgument, out string value)
        {
            string name = commandArgument.Name;
            commandArgument = ArgumentValue(command, commandArgument); // Sequence of passed arguments might be wrong.

            bool isValue = false;
            string result = commandArgument.Value;
            UtilFramework.Assert(name.ToLower() == result.Substring(0, name.Length).ToLower());
            if (result.ToUpper().StartsWith(name.ToUpper()))
            {
                result = result.Substring(name.Length);
            }
            if (result.StartsWith("="))
            {
                result = result.Substring(1);
            }
            result = UtilFramework.StringNull(result);
            if (result != null)
            {
                isValue = true;
            }
            if (result?.ToLower() == "null") // User sets value to null.
            {
                result = null;
            }
            value = result;
            return isValue;
        }

        internal static void FolderCopy(string folderNameSource, string folderNameDest, string searchPattern, bool isAllDirectory)
        {
            var source = new DirectoryInfo(folderNameSource);
            var dest = new DirectoryInfo(folderNameDest);
            SearchOption searchOption = SearchOption.TopDirectoryOnly;
            if (isAllDirectory)
            {
                searchOption = SearchOption.AllDirectories;
            }
            foreach (FileInfo file in source.GetFiles(searchPattern, searchOption))
            {
                string fileNameSource = file.FullName;
                string fileNameDest = Path.Combine(dest.FullName, file.FullName.Substring(source.FullName.Length));
                FileCopy(fileNameSource, fileNameDest);
            }
        }

        /// <summary>
        /// Create folder if it does not yet exist.
        /// </summary>
        internal static void FolderCreate(string fileName)
        {
            string folderName = new FileInfo(fileName).DirectoryName;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
        }

        internal static void FileCopy(string fileNameSource, string fileNameDest)
        {
            FolderCreate(fileNameDest);
            File.Copy(fileNameSource, fileNameDest, true);
        }

        internal static void FolderDelete(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                foreach (FileInfo fileInfo in new DirectoryInfo(folderName).GetFiles("*.*", SearchOption.AllDirectories))
                {
                    fileInfo.Attributes = FileAttributes.Normal; // See also: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it/30673648
                }
                try
                {
                    Directory.Delete(folderName, true);
                }
                catch (IOException exception)
                {
                    throw new Exception(string.Format("Could not delete folder! Make sure server.ts and node.exe is not running. ({0})", folderName), exception);
                }
            }
            UtilFramework.Assert(!UtilCli.FolderNameExist(folderName));
        }

        internal static bool FolderNameExist(string folderName)
        {
            return Directory.Exists(folderName);
        }

        /// <summary>
        /// Returns git commit sha.
        /// </summary>
        internal static string GitCommit()
        {
            string result = "Commit";
            try
            {
                result = UtilCli.StartStdout(UtilFramework.FolderName, "git", "rev-parse --short HEAD");
                result = result.Replace("\n", "");
            }
            catch
            {
                // Silent exception
            }
            return result;
        }

        /// <summary>
        /// Tag version build.
        /// </summary>
        internal static void VersionBuild(Action build)
        {
            // Read UtilFramework.cs
            string fileNameServer = UtilFramework.FolderName + "Framework/Framework/UtilFramework.cs";
            string textServer = UtilFramework.FileLoad(fileNameServer);
            string fileNameClient = UtilFramework.FolderName + "Framework/Framework.Angular/application/src/app/data.service.ts";
            string textClient = UtilFramework.FileLoad(fileNameClient);

            string versionBuild = string.Format("Build (WorkplaceX={3}; Commit={0}; Pc={1}; Time={2} (UTC);)", UtilCli.GitCommit(), System.Environment.MachineName, UtilFramework.DateTimeToString(DateTime.Now.ToUniversalTime()), UtilFramework.Version);

            string findServer = "/* VersionBuild */"; // See also: method CommandBuild.BuildServer();
            string replaceServer = string.Format("                return \"{0}\"; /* VersionBuild */", versionBuild);
            string findClient = "/* VersionBuild */"; // See also: file data.service.ts
            string replaceClient = string.Format("  public VersionBuild: string = \"{0}\"; /* VersionBuild */", versionBuild);

            // Write UtilFramework.cs
            string textNewServer = UtilFramework.ReplaceLine(textServer, findServer, replaceServer);
            File.WriteAllText(fileNameServer, textNewServer);
            string textNewClient = UtilFramework.ReplaceLine(textClient, findClient, replaceClient);
            File.WriteAllText(fileNameClient, textNewClient);

            try
            {
                build();
            }
            finally
            {
                File.WriteAllText(fileNameServer, textServer); // Back to original text.
                File.WriteAllText(fileNameClient, textClient); // Back to original text.
            }
        }

        /// <summary>
        /// Returns password (ConnectionString or GitUrl) without sensitive data.
        /// </summary>
        /// <param name="password">For example ConnectionString or GitUrl.</param>
        private static string ConsoleWriteLinePasswordHide(string password)
        {
            return "[Password]"; // Remove password from ConnectionString or GitUrl.
        }

        /// <summary>
        /// Returns text without password. It replaces password with PasswordHide.
        /// </summary>
        private static string ConsoleWriteLinePasswordHide(string text, string password)
        {
            if (text != null && password?.Length > 0)
            {
                while (text.ToLower().IndexOf(password.ToLower()) >= 0)
                {
                    int indexStart = text.ToLower().IndexOf(password.ToLower());
                    int length = password.Length;
                    string passwordHide = ConsoleWriteLinePasswordHide(password);
                    text = text.Substring(0, indexStart) + passwordHide + text.Substring(indexStart + length);
                }
            }
            return text;
        }

        /// <summary>
        /// Write text which might contain sensitive data (ConnectionString and GitUrl) with this method to console.
        /// </summary>
        internal static void ConsoleWriteLinePassword(object value, ConsoleColor? color = null)
        {
            string text = string.Format("{0}", value);
            var configCli = ConfigCli.Load();
            text = ConsoleWriteLinePasswordHide(text, configCli.EnvironmentGet().ConnectionStringFramework);
            text = ConsoleWriteLinePasswordHide(text, configCli.EnvironmentGet().ConnectionStringApplication);
            text = ConsoleWriteLinePasswordHide(text, configCli.EnvironmentGet().DeployAzureGitUrl);
            Console.WriteLine(text, color);
        }

        /// <summary>
        /// Write to console in color.
        /// </summary>
        internal static void ConsoleWriteLineColor(object value, ConsoleColor? color)
        {
            if (color == null)
            {
                Console.WriteLine(value);
            }
            else
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                try
                {
                    Console.WriteLine(value);
                }
                finally
                {
                    Console.ForegroundColor = foregroundColor;
                }
            }
        }

        /// <summary>
        /// Write to stderr.
        /// </summary>
        internal static void ConsoleWriteLineError(object value)
        {
            using (TextWriter textWriter = Console.Error)
            {
                textWriter.WriteLine(value);
            }
        }

        /// <summary>
        /// Returns text escaped as CSharp code. Handles special characters.
        /// </summary>
        public static string EscapeCSharpString(string text)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(text), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
}
