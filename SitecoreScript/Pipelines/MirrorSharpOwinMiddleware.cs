using MirrorSharp;
using MirrorSharp.Owin;
using Sitecore.Script.Abstractions;
using Sitecore.Script.Owin.Pipelines.InitializeOwinMiddleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Xml;

namespace Sitecore.Script.Pipelines
{
    /// <summary>
    /// Direct processor that leverages OWIN startup in Sitecore 8.2 (This needs to be called differently from Sitecore 9+)
    /// Make sure to enable WebSocket Protocol on Windows Features:
    /// in the control panel window, navigate to Internet Information Services > World Wide Web Services > Application Development Features
    /// Then enable Websocket Protocol, to be able to work on IIS
    /// </summary>
    public class MirrorSharpOwinMiddleware
    {
        private static IScriptInitialConfiguration scriptConfiguration;

        public static List<string> DotNetDllFiles
        {
            get
            {
                return scriptConfiguration != null ? scriptConfiguration.DotNetReferenceFiles.ToList() : new List<string>();
            }
        }

        public void Process(InitializeOwinMiddlewareArgs args)
        {
            Sitecore.Diagnostics.Log.Info("[Sitecore.Script] OWIN.Startup: Loading assemblies references for MirrorSharp.", this);
            try
            {
                LoadScriptConfiguration();
                // Initialize mirrorsharp
                args.App.UseMirrorSharp(

                    new MirrorSharpOptions
                    {
                        SelfDebugEnabled = true,
                        IncludeExceptionDetails = true,
                        SetOptionsFromClient = new SetOptionsFromClientExtension(),
                        ExcludeDiagnosticIds = scriptConfiguration.ExcludedDiagnosticIds.ToList()
                    }
                    .SetupCSharp(c =>
                    {
                        c.MetadataReferences = c.MetadataReferences.Clear();
                        c.AddMetadataReferencesFromFiles(scriptConfiguration.DotNetReferenceFiles.ToArray());
                    })
                ) ;
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

        private void LoadScriptConfiguration()
        {
            var xmlNode = Sitecore.Configuration.Factory.GetConfigNode("sitecore-script/initialize");
            scriptConfiguration = Sitecore.Configuration.Factory.CreateObject<IScriptInitialConfiguration>(xmlNode);
            scriptConfiguration.LoadBinDllReferenceFiles();
        }
    }
}