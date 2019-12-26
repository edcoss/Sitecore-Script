using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using MirrorSharp.Advanced;
using MirrorSharp;
using MirrorSharp.Owin;
using Owin;
using System.Collections.Generic;
using System.Text;
using System.IO;

// Uncomment when using non-Sitecore websites
[assembly: OwinStartup(typeof(Sitecore.Script.Startup))]

namespace Sitecore.Script
{
    /// <summary>
    /// This class is only used when running on a non-Sitecore website. And also, for testing purposes.
    /// This StartUp implementation requires to update web.config OWIN startup attribute, as well as the removal of the PrincipalPermission
    /// attribute on some of the ObjectDebugger.aspx.cs methods.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Collection of DLLs that will not get loaded into mirrorSharp.
        /// For testing purposes and for non-Sitecore sites, add any non-managed assembly into this list
        /// to be excluded when loading managed dlls for XML documentation
        /// </summary>
        private readonly List<string> _doNotIncludeDllPerDefault = new List<string>()
        {
            "msvcp140.dll",
            "vcruntime140.dll",
        };

        /// <summary>
        /// Collection of .Net assemblies, located in Windows > Microsoft.NET > Framework64 > v4.0.30319
        /// These dlls are considered to load them up as references for Roslyn and XML documentation in mirrorSharp
        /// </summary>
        private readonly List<string> dotnetLibraries = new List<string>()
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework64\\v4.0.30319\\mscorlib.dll",
            Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework64\\v4.0.30319\\System.dll",
            Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Data.dll",
            Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Web.dll",
            Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Xml.dll",
            Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework64\\v4.0.30319\\System.Xml.Linq.dll"
        };

        public void Configuration(IAppBuilder app)
        {
            //Sitecore.Diagnostics.Log.Info("Sitecore.Script => OWIN.Startup: Loading assemblies references for MirrorSharp.", this);
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            var path = System.Web.HttpRuntime.BinDirectory;

            app.UseMirrorSharp(
                new MirrorSharpOptions
                {
                    SelfDebugEnabled = true,
                    IncludeExceptionDetails = true,
                    SetOptionsFromClient = new SetOptionsFromClientExtension()
                }
                .SetupCSharp(c =>
                {
                    c.MetadataReferences = c.MetadataReferences.Clear();
                    c.AddMetadataReferencesFromFiles(dotnetLibraries.ToArray());
                    c.AddMetadataReferencesFromFiles(GetAllDllsFromFolder(path));
                })
            );            
        }

        private string[] GetAllDllsFromFolder(string path)
        {
            var dllReferences = new List<string>();

            foreach (var filePath in Directory.GetFiles(path))
            {
                var fileName = filePath.Remove(0, path.Length + 1);
                if (filePath.EndsWith(".dll") && !_doNotIncludeDllPerDefault.Contains(fileName))
                {
                    dllReferences.Add(filePath);
                }
            }
            return dllReferences.ToArray();
        }
    }
}
