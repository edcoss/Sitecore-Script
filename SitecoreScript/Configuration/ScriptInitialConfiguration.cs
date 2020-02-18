using Sitecore.Script.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public void LoadBinDllReferenceFiles()
        {
            var binPath = System.Web.HttpRuntime.BinDirectory;
            AddAllAssembliesInPath(binPath);
        }

        public void AddAssembly(string assemblyPath)
        {
            if (!string.IsNullOrEmpty(assemblyPath) && !dotnetReferenceFiles.Contains(assemblyPath))
            {
                Sitecore.Diagnostics.Log.Info("[Sitecore.Script] Initialization, adding reference: " + assemblyPath, this);
                dotnetReferenceFiles.Add(assemblyPath);
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
                AddAssembly(assemblyDllFile);
            }
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