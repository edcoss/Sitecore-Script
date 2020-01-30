using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Sitecore.Data.Items;
using Sitecore.Script.Helpers;

public class ScriptSourceResolver : SourceReferenceResolver
{
    private readonly Dictionary<string, string> _sitecoreScripts = new Dictionary<string, string>();
    private readonly SourceFileResolver _fileBasedResolver;

    /// <summary>
    /// 
    /// </summary>
    public ScriptSourceResolver() : this(ImmutableArray.Create(new string[0]), AppDomain.CurrentDomain.BaseDirectory)
    {
        // For .Net Core use AppContext.BaseDirectory for second constructor, instead of AppDomain
    }

    public ScriptSourceResolver(ImmutableArray<string> searchPaths, string baseDirectory)
    {
        _fileBasedResolver = new SourceFileResolver(searchPaths, baseDirectory);
    }

    private string GetScriptPath(string path)
    {
        return path.ToLowerInvariant().Contains(Settings.ScriptLibraryPath.ToLowerInvariant()) ? path : Settings.ScriptLibraryPath + path;
    }

    public override string NormalizePath(string path, string baseScriptPath)
    {
        var fullItemPath = GetScriptPath(path);
        var item = GetItem(path);
        if (item == null) return _fileBasedResolver.NormalizePath(path, baseScriptPath);

        return fullItemPath;
    }

    public override Stream OpenRead(string resolvedPath)
    {
        var item = GetItem(resolvedPath);
        if (item == null) return _fileBasedResolver.OpenRead(resolvedPath);

        if (_sitecoreScripts.ContainsKey(resolvedPath.ToLowerInvariant()))
        {
            var storedScript = _sitecoreScripts[resolvedPath.ToLowerInvariant()];
            return new MemoryStream(Encoding.UTF8.GetBytes(storedScript));
        }

        return Stream.Null;
    }

    public override string ResolveReference(string path, string baseFilePath)
    {
        var fullItemPath = GetScriptPath(path);
        var item = GetItem(fullItemPath);
        if (item == null) item = GetItem(path);
        if (item == null) return _fileBasedResolver.ResolveReference(path, baseFilePath);

        var scriptCode = item["code"];
        
        if (!string.IsNullOrWhiteSpace(scriptCode))
        {
            var key = fullItemPath.ToLowerInvariant();
            if (!_sitecoreScripts.ContainsKey(key))
            {
                _sitecoreScripts.Add(key, scriptCode);
            }
            else
            {
                _sitecoreScripts[key] = scriptCode;
            }
        }
        
        return fullItemPath;
    }

    private static Item GetItem(string input)
    {
        var database = Sitecore.Configuration.Factory.GetDatabase(Settings.ContextDatabase);
        var item = database.GetItem(input);
        return item;
    }

    protected bool Equals(ScriptSourceResolver other)
    {
        return Equals(_sitecoreScripts, other._sitecoreScripts) && Equals(_fileBasedResolver, other._fileBasedResolver);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ScriptSourceResolver)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = 37;
            hashCode = (hashCode * 397) ^ (_sitecoreScripts?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (_fileBasedResolver?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}