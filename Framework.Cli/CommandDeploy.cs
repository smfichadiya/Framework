﻿namespace Framework.Cli
{
    using System;
    using System.IO;

    public class CommandDeploy : CommandBase
    {
        public CommandDeploy(AppCliBase appCli)
            : base(appCli, "deploy", "Deploy app to Azure git")
        {

        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();
            string azureGitUrl = configCli.AzureGitUrl; // For example: "https://MyUsername:MyPassword@my22.scm.azurewebsites.net:443/my22.git"
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp2.0/publish/";

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");

            UtilCli.DotNet(folderName, "publish");
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");

            UtilCli.Start(folderNamePublish, "git", "init");
            UtilCli.Start(folderNamePublish, "git", "config user.email \"deploy@deploy.deploy\""); // Prevent: Error "Please tell me who you are". See also: http://www.thecreativedev.com/solution-github-please-tell-me-who-you-are-error/
            UtilCli.Start(folderNamePublish, "git", "config user.name \"Deploy\"");
            UtilCli.Start(folderNamePublish, "git", "remote add azure " + azureGitUrl);
            UtilCli.Start(folderNamePublish, "git", "fetch --all", isRedirectStdErr: true); // Another possibility is argument "-q" to do not write to stderr.
            UtilCli.Start(folderNamePublish, "git", "add .");
            UtilCli.Start(folderNamePublish, "git", "commit -m Deploy");
            UtilCli.Start(folderNamePublish, "git", "push azure master -f", isRedirectStdErr: true); // Do not write to stderr. Can be tested with "dotnet run -- deploy [AzureGitUrl] 2>Error.txt"
        }
    }
}