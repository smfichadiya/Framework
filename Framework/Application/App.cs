﻿namespace Framework.Application
{
    using Framework.Component;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class App
    {
        /// <summary>
        /// Gets or sets AppJson. This is the application root json component being transferred between server and client.
        /// </summary>
        public AppJson AppJson { get; internal set; }

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type[] TypeComponentInNamespaceList()
        {
            return (new Type[] {
                GetType(), // Namespace of running application.
                typeof(App), // Used for example for class Navigation.
            }).Distinct().ToArray(); // Enable serialization of components in App and AppConfig namespace.
        }

        protected virtual internal void Process()
        {
            AppJson.Version = UtilFramework.Version;
            AppJson.VersionBuild = UtilFramework.VersionBuild;
        }
    }

    public class AppSelector
    {
        public async Task<App> CreateApp(HttpContext context)
        {
            var result = CreateApp();
            string json = await UtilServer.StreamToString(context.Request.Body);
            if (json != null) // If post
            {
                result.AppJson = UtilJson.Deserialize<AppJson>(json, result.TypeComponentInNamespaceList());
            }
            else
            {
                result.AppJson = new AppJson();
            }
            result.Process();
            return result;
        }

        /// <summary>
        /// Override this method define App.
        /// </summary>
        protected virtual App CreateApp()
        {
            return new App(); // DefaultApp
        }
    }
}
