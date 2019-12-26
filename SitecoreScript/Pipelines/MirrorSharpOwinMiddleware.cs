using MirrorSharp;
using MirrorSharp.Owin;
using Sitecore.Owin.Pipelines.InitializeOwinMiddleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Sitecore.Script.Pipelines
{
    /// <summary>
    /// Processor that leverages OWIN startup in Sitecore
    /// Make sure to enable WebSocket Protocol on Windows Features:
    /// in the control panel window, navigate to Internet Information Services > World Wide Web Services > Application Development Features
    /// Then enable Websocket Protocol, to be able to work on IIS
    /// </summary>
    public class MirrorSharpOwinMiddleware
    {
        public void Process(InitializeOwinMiddlewareArgs args)
        {
            AddBinDllReferenceFiles();
            Sitecore.Diagnostics.Log.Info("[Sitecore.Script] OWIN.Startup: Loading assemblies references for MirrorSharp.", this);
            try
            {
                // Initialize mirrorsharp
                args.App.UseMirrorSharp(

                    new MirrorSharpOptions
                    {
                        SelfDebugEnabled = true,
                        IncludeExceptionDetails = true,
                        SetOptionsFromClient = new SetOptionsFromClientExtension()
                    }
                    .SetupCSharp(c =>
                    {
                        c.MetadataReferences = c.MetadataReferences.Clear();
                        c.AddMetadataReferencesFromFiles(dotNetDllFiles.ToArray());
                    })
                );
                RouteTable.Routes.MapOwinPath("mirrorsharp", "/mirrorsharp");

                // This is a another alternative to initialize Middleware code:

                //RouteTable.Routes.MapOwinRoute("mirrorsharp", app => app.UseMirrorSharp(
                //    new MirrorSharpOptions
                //    {
                //        SelfDebugEnabled = true,
                //        IncludeExceptionDetails = true,
                //        SetOptionsFromClient = new SetOptionsFromClientExtension()
                //    }
                //    .SetupCSharp(c =>
                //    {
                //        c.MetadataReferences = c.MetadataReferences.Clear();
                //        c.AddMetadataReferencesFromFiles(dotNetDllFiles.ToArray());
                //    })
                //));
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Error on MirrorSharpOwinMiddleware Initialize pipeline.", ex, this);
            }
            Sitecore.Diagnostics.Log.Info("[Sitecore.Script] OWIN.Startup: Assembly references loaded for MirrorSharp.", this);
        }

        private static List<string> dotNetDllFiles = new List<string>();
        private static List<string> excludedDllFiles = new List<string>();

        public static List<string> ExcludedDllFiles
        {
            get
            {
                return excludedDllFiles;
            }
        }

        public static List<string> DotNetDllFiles
        {
            get
            {
                return dotNetDllFiles;
            }
        }

        private void AddBinDllReferenceFiles()
        {
            var binPath = System.Web.HttpRuntime.BinDirectory;
            AddAllAssembliesInPath(binPath);
        }

        private string[] GetAllDllsFromFolder(string path)
        {
            var dllReferences = new List<string>();

            foreach (var filePath in System.IO.Directory.GetFiles(path))
            {
                var fileName = filePath.Remove(0, path.Length + 1);
                if (filePath.EndsWith(".dll") && !excludedDllFiles.Any(name => fileName.ToLowerInvariant() == name.ToLowerInvariant()))
                {
                    dllReferences.Add(filePath);
                }
            }
            return dllReferences.ToArray();
        }


        public void AddAssembly(string assemblyPath)
        {
            if (!string.IsNullOrEmpty(assemblyPath) && !dotNetDllFiles.Contains(assemblyPath))
            {
                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] OWIN.Startup, adding reference: " + assemblyPath, this);
                dotNetDllFiles.Add(assemblyPath);
            }
        }

        public void ExcludeFile(string filename)
        {
            if (!string.IsNullOrEmpty(filename) && !excludedDllFiles.Contains(filename))
            {
                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] OWIN.Startup, excluding reference: " + filename, this);
                excludedDllFiles.Add(filename);
            }
        }

        public void AddAllAssembliesInPath(string path)
        {
            foreach (var assemblyDllFile in GetAllDllsFromFolder(path))
            {
                if (!string.IsNullOrEmpty(assemblyDllFile) && !dotNetDllFiles.Contains(assemblyDllFile))
                {
                    Sitecore.Diagnostics.Log.Info("[Sitecore.Script] OWIN.Startup, adding reference: " + assemblyDllFile, this);
                    dotNetDllFiles.Add(assemblyDllFile);
                }
            }
        }
    }
}