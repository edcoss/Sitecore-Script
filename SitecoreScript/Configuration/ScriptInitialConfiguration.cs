using Sitecore.Script.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Sitecore.Script.Configuration
{
    public class ScriptInitialConfiguration : IScriptInitialConfiguration
    {
        public ScriptInitialConfiguration() { }

        private readonly List<string> dotnetReferenceFiles = new List<string>();
        private readonly List<string> excludedDllFiles = new List<string>();
        private readonly List<string> excludedDiagnosticIds = new List<string>();

        public IEnumerable<string> DotNetReferenceFiles => dotnetReferenceFiles;

        public IEnumerable<string> ExcludedFiles => excludedDllFiles;

        public IEnumerable<string> ExcludedDiagnosticIds => excludedDiagnosticIds;

        public void LoadBinDllReferenceFiles(XmlNode node)
        {
            var binPath = System.Web.HttpRuntime.BinDirectory;
            AddAllAssembliesInPath(binPath);
        }

        public void AddAssembly(string assemblyPath)
        {
            if (File.Exists(assemblyPath))
            {
                FileInfo file = new FileInfo(assemblyPath);
                if (!dotnetReferenceFiles.Any(reference => new FileInfo(reference).Name.ToLowerInvariant() == file.Name.ToLowerInvariant()))
                {
                    Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, adding direct reference: " + assemblyPath, this);
                    dotnetReferenceFiles.Add(assemblyPath);
                }
                else
                {
                    Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, DLL is already added: " + assemblyPath, this);
                }
            }
            else
            {
                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, reference not found: " + assemblyPath, this);
            }
        }

        public void ExcludeFile(string filename)
        {
            if (!string.IsNullOrEmpty(filename) && !excludedDllFiles.Contains(filename))
            {
                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, excluding reference: " + filename, this);
                excludedDllFiles.Add(filename);
            }
        }

        public void AddAllAssembliesInPath(string path)
        {
            foreach (var assemblyDllFile in GetAllDllsFromFolder(path))
            {
                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, adding reference: " + assemblyDllFile, this);
                dotnetReferenceFiles.Add(assemblyDllFile);
            }
        }

        private string[] GetAllDllsFromFolder(string path)
        {
            var dllReferences = new List<string>();

            if (Directory.Exists(path))
            {
                foreach (var filePath in System.IO.Directory.GetFiles(path))
                {
                    FileInfo file = new FileInfo(filePath);
                    if (filePath.ToLowerInvariant().EndsWith(".dll"))
                    {
                        if (!excludedDllFiles.Any(name => file.Name.ToLowerInvariant() == name.ToLowerInvariant()))
                        {
                            if (!dotnetReferenceFiles.Any(reference => new FileInfo(reference).Name.ToLowerInvariant() == file.Name.ToLowerInvariant()))
                            {
                                dllReferences.Add(filePath);
                            }
                            else
                            {
                                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, DLL is already added: " + filePath, this);
                            }
                        }
                        else
                        {
                            Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, skipping excluded file: " + filePath, this);
                        }
                    }
                    else
                    {
                        Sitecore.Diagnostics.Log.Debug("[Sitecore.Script] Initialization, skipping file: " + filePath, this);
                    }
                }
            }
            else
            {
                Sitecore.Diagnostics.Log.Debug("[Sitecore.Script] Initialization, directory path not found: " + path, this);
            }
            return dllReferences.ToArray();
        }

        public void ExcludeDiagnosticId(string diagnosticId)
        {
            if (!string.IsNullOrWhiteSpace(diagnosticId))
            {
                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, excluding diagnostic Id: " + diagnosticId, this);
                excludedDiagnosticIds.Add(diagnosticId);
            }
        }
    }
}