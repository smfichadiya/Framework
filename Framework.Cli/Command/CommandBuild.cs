﻿namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Cli build command.
    /// </summary>
    internal class CommandBuild : CommandBase
    {
        public CommandBuild(AppCli appCli)
            : base(appCli, "build", "Build Angular client and ASP.NET Core server")
        {

        }

        internal CommandOption OptionClientOnly;

        protected internal override void Register(CommandLineApplication configuration)
        {
            OptionClientOnly = configuration.Option("-c|--client", "Build angular client only.", CommandOptionType.NoValue);
        }

        /// <summary>
        /// Copy folder Application.Website/Shared/CustomComponent/
        /// </summary>
        private static void BuildAngularInit()
        {
            // Delete folder Application.Website/
            string folderNameApplicationWebSite = UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website/";
            UtilCli.FolderDelete(folderNameApplicationWebSite);

            // Copy folder CustomComponent/
            string folderNameSource = UtilFramework.FolderName + "Application.Website/Shared/CustomComponent/";
            string folderNameDest = UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website/Shared/CustomComponent/";
            UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);

            // Copy empty index.html file
            UtilCli.FileCopy(UtilFramework.FolderName + "Framework/Framework.Angular/application/src/index.html", UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website/Default/index.html");

            // Ensure folder exists now
            UtilFramework.Assert(Directory.Exists(folderNameApplicationWebSite));
        }

        /// <summary>
        /// Build Framework/Framework.Angular/application/.
        /// </summary>
        private static void BuildAngular()
        {
            // Build SSR
            {
                string folderName = UtilFramework.FolderName + "Framework/Framework.Angular/application/";
                UtilCli.Npm(folderName, "install --loglevel error"); // Angular install. --loglevel error prevent writing to STDERR "npm WARN optional SKIPPING OPTIONAL DEPENDENCY"
                UtilCli.Npm(folderName, "run build:ssr", isRedirectStdErr: true); // Build Server-side Rendering (SSR) to folder Framework/Framework.Angular/application/server/dist/ // TODO Bug report Angular build writes to stderr. Repo steps: Delete node_modules and run npm install and then run build:ssr.
            }

            // Copy output dist folder
            {
                string folderNameSource = UtilFramework.FolderName + "Framework/Framework.Angular/application/dist/application/";
                string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/Framework.Angular/";

                // Copy folder
                UtilCli.FolderDelete(folderNameDest);
                UtilFramework.Assert(!Directory.Exists(folderNameDest));
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                UtilFramework.Assert(Directory.Exists(folderNameDest));
            }
        }

        /// <summary>
        /// Copy ConfigServer.json to publish folder.
        /// </summary>
        internal static void ConfigServerPublish()
        {
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp3.1/publish/";

            string fileNameSource = UtilFramework.FolderName + "ConfigServer.json";
            string fileNameDest = folderNamePublish + "ConfigServer.json";
            UtilCli.FileCopy(fileNameSource, fileNameDest);
        }

        private static void BuildServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp3.1/publish/";

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");
            UtilCli.DotNet(folderName, "publish"); // Use publish instead to build.
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");

            ConfigServerPublish();
        }

        /// <summary>
        /// Execute "npm run build" command.
        /// </summary>
        private static void BuildWebsiteNpm(ConfigCliWebsite website)
        {
            string folderNameNpmBuild = UtilFramework.FolderNameParse(website.FolderNameNpmBuild);
            if (UtilFramework.StringNull(folderNameNpmBuild) != null)
            {
                string folderName = UtilFramework.FolderName + folderNameNpmBuild;
                UtilCli.Npm(folderName, "install --loglevel error"); // --loglevel error prevent writing to STDERR "npm WARN optional SKIPPING OPTIONAL DEPENDENCY"
                UtilCli.Npm(folderName, "run build");
            }
        }

        /// <summary>
        /// Build all layout Websites. For example: "Application.Website/LayoutDefault"
        /// </summary>
        private void BuildWebsite()
        {
            var configCli = ConfigCli.Load();

            // Ensure FolderNameNpmBuild is defined once only in ConfigCli.json.
            ConfigCliWebsite configCliWebsite = configCli.WebsiteList.GroupBy(item => item.FolderNameNpmBuild.ToLower()).Where(group => group.Count() > 1).FirstOrDefault()?.FirstOrDefault();
            UtilFramework.Assert(configCliWebsite == null, string.Format("ConfigCli.json Website defined more than once. Use DomainNameList instead! (FolderNameNpmBuild={0})", configCliWebsite?.FolderNameNpmBuild));

            // Delete folder Application.Server/Framework/Application.Website/
            string folderNameApplicationWebsite = UtilFramework.FolderName + "Application.Server/Framework/Application.Website/";
            UtilCli.FolderDelete(folderNameApplicationWebsite);

            foreach (var website in configCli.WebsiteList)
            {
                Console.WriteLine(string.Format("### Build Website (Begin) - {0}", website.DomainNameListToString()));
                
                // Delete dist folder
                string folderNameDist = UtilFramework.FolderNameParse(website.FolderNameDist);
                UtilFramework.Assert(folderNameDist != null);
                UtilCli.FolderDelete(folderNameDist);

                // npm run build
                BuildWebsiteNpm(website);
                string folderNameServer = UtilFramework.FolderNameParse(website.FolderNameServerGet(configCli));
                UtilFramework.Assert(folderNameServer != null, "FolderNameServer can not be null!");
                UtilFramework.Assert(folderNameServer.StartsWith("Application.Server/Framework/Application.Website/"), "FolderNameServer has to start with 'Application.Server/Framework/Application.Website/'!");

                // Copy dist folder
                string folderNameSource = UtilFramework.FolderName + folderNameDist;
                string folderNameDest = UtilFramework.FolderName + folderNameServer;
                if (!UtilCli.FolderNameExist(folderNameSource))
                {
                    throw new Exception(string.Format("Folder does not exist! ({0})", folderNameDest));
                }
                UtilCli.FolderDelete(folderNameDest);
                UtilFramework.Assert(!UtilCli.FolderNameExist(folderNameDest));
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                UtilFramework.Assert(UtilCli.FolderNameExist(folderNameDest));

                Console.WriteLine(string.Format("### Build Website (End) - {0}", website.DomainNameListToString()));
            }
        }

        /// <summary>
        /// Clone external git repo and call prebuild script.
        /// </summary>
        private static void ExternalGit()
        {
            var configCli = ConfigCli.Load();

            // Clone repo
            var externalGit = UtilFramework.StringNull(configCli.ExternalGit);
            if (externalGit != null)
            {
                string externalFolderName = UtilFramework.FolderName + "ExternalGit/";
                if (!UtilCli.FolderNameExist(externalFolderName))
                {
                    Console.WriteLine("Git Clone ExternalGit");
                    UtilCli.FolderCreate(externalFolderName);
                    UtilCli.Start(externalFolderName, "git", "clone --recursive -q" + " " + externalGit); // --recursive clone also submodule Framework -q do not write to stderr on linux
                }
            }
        }

        /// <summary>
        /// Run method AppCli.ExternalPrebuild(); on ExternalGit/ProjectName/
        /// </summary>
        private static void ExternalPrebuild()
        {
            var configCli = ConfigCli.Load();

            // External git url
            var externalGit = UtilFramework.StringNull(configCli.ExternalGit);

            // Call external cli command (prebuild script)
            var externalProjectName = UtilFramework.StringNull(configCli.ExternalProjectName);

            if (externalGit != null && externalProjectName != null)
            {
                string folderName = UtilFramework.FolderName + "ExternalGit/" + externalProjectName + "/" + "Application.Cli";
                UtilCli.DotNet(folderName, "run -- external");
            }
        }

        protected internal override void Execute()
        {
            // Clone external repo
            ExternalGit();

            // Copy folder Application.Website/Shared/CustomComponent/
            BuildAngularInit();

            // Run cli external command. Override for example custom components.
            ExternalPrebuild();

            // Build layout Website(s) (npm) includes for example Bootstrap
            BuildWebsite(); // Has to be before dotnet publish! It will copy site to publish/Framework/Application.Website/

            UtilCli.VersionBuild(() => {
                // Build Angular client (npm)
                BuildAngular();

                if (OptionClientOnly.OptionGet() == false)
                {
                    // Build .NET Core server (dotnet)
                    BuildServer();
                }
            });
        }
    }
}
