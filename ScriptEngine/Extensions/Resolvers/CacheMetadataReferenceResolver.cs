using Microsoft.CodeAnalysis;
using ScriptSharp.ScriptEngine.Extensions.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Extensions.Resolvers
{
    public class CacheMetadataReferenceResolver : MetadataReferenceResolver
    {
        internal readonly RelativePathResolver PathResolver;
        private readonly Func<string, MetadataReferenceProperties, PortableExecutableReference> _fileReferenceProvider;

        internal CacheMetadataReferenceResolver(ImmutableArray<string> searchPaths, string baseDirectory)
        {
            this.PathResolver = new RelativePathResolver(searchPaths, baseDirectory);
            _fileReferenceProvider = new Func<string, MetadataReferenceProperties, PortableExecutableReference>((path, properties) => MetadataReference.CreateFromFile(path, properties));
        }

        public override int GetHashCode()
        {
            return Hash.Combine(1, 0);
        }

        public bool Equals(CacheMetadataReferenceResolver other)
        {
            return ReferenceEquals(this, other);
        }

        public override bool Equals(object other)
        {
            return Equals(other as CacheMetadataReferenceResolver);
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            if (PathUtilities.IsFilePath(reference))
            {
                if (PathResolver != null)
                {
                    string resolvedPath = PathResolver.ResolvePath(reference, baseFilePath);
                    if (resolvedPath != null)
                    {
                        ObjectCache cache = MemoryCache.Default;
                        object cachedObj = cache.Get(resolvedPath);
                        ImmutableArray<PortableExecutableReference> referenceObj = ImmutableArray<PortableExecutableReference>.Empty;
                        if (cachedObj == null)
                        {
                            var policy = new CacheItemPolicy();
                            policy.SlidingExpiration = new TimeSpan(1, 0, 0);
                            referenceObj = ImmutableArray.Create(_fileReferenceProvider(resolvedPath, properties));
                            cache.Set(reference, referenceObj, policy);
                        }
                        else
                        {
                            referenceObj = (ImmutableArray<PortableExecutableReference>)cachedObj;
                        }
                        return referenceObj;
                    }
                }
            }

            return ImmutableArray<PortableExecutableReference>.Empty;
        }        
    }
}
