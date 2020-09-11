using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CSDiscordService.Eval
{
    public class NugetDirectiveProcessor : IDirectiveProcessor, IDisposable
    {
        private readonly SourceCacheContext cache = new SourceCacheContext();
        private bool disposedValue;

        public bool CanProcessDirective(string directive)
        {
            return directive != null && directive.StartsWith("#nuget");
        }

        public async Task PreProcess(string directive, ScriptExecutionContext context)
        {
            var nugetDirective = NugetPreProcessorDirective.Parse(directive);
            string frameworkName = Assembly.GetEntryAssembly()!.GetCustomAttributes(true)
              .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
              .Select(x => x.FrameworkName)
              .FirstOrDefault()!;
            NuGetFramework framework = frameworkName == null
              ? NuGetFramework.AnyFramework
              : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());

            using var cache = new SourceCacheContext();
            var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
            var repository = Repository.Factory.GetCoreV3(nugetDirective.FeedUrl, FeedType.HttpV3);
            var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>();
            var searchMetadata = await packageMetadataResource.GetMetadataAsync(
                nugetDirective.PackageId,
                includePrerelease: false,
                includeUnlisted: false,
                cache,
                NullLogger.Instance,
                CancellationToken.None);

            if (!searchMetadata.Any())
            {
                throw new NuGetResolverException($"Unable to resolve nuget package with id {nugetDirective.PackageId}");
            }

            var latest = searchMetadata.OrderByDescending(a => a.Identity.Version).FirstOrDefault();

            if (latest is null)
            {
                throw new NuGetResolverException($"Unable to resolve nuget package with id {nugetDirective.PackageId}");
            }

            var packageId = latest.Identity;
            var dependencyResource = await repository.GetResourceAsync<DependencyInfoResource>();

            await GetPackageDependencies(
                packageId,
                framework,
                cache,
                repository,
                dependencyResource,
                availablePackages);

            var resolverContext = new PackageResolverContext(
                DependencyBehavior.Lowest,
                new[] { nugetDirective.PackageId },
                Enumerable.Empty<string>(),
                Enumerable.Empty<PackageReference>(),
                Enumerable.Empty<PackageIdentity>(),
                availablePackages,
                new[] { repository.PackageSource },
                NullLogger.Instance);

            var resolver = new PackageResolver();
            var toInstall = resolver.Resolve(resolverContext, CancellationToken.None)
                .Select(a => availablePackages.Single(b => PackageIdentityComparer.Default.Equals(b, a)));


            var pathResolver = new PackagePathResolver(Path.Combine(Path.GetTempPath(), "packages"));
            var extractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3,
                XmlDocFileSaveMode.None,
                ClientPolicyContext.GetClientPolicy(Settings.LoadDefaultSettings(null),
                    NullLogger.Instance),
                NullLogger.Instance);
            var libraries = new List<string>();
            var frameworkReducer = new FrameworkReducer();
            var downloadResource = await repository.GetResourceAsync<DownloadResource>(CancellationToken.None);
            foreach (var package in toInstall)
            {
                libraries.AddRange(await Install(downloadResource, package, pathResolver, extractionContext, frameworkReducer, framework));
            }

            foreach (var path in libraries)
            {
                var assembly = Assembly.LoadFile(path);
                if (context.TryAddReferenceAssembly(assembly))
                {
                    foreach (var ns in assembly.GetTypes().Select(a => a.Namespace).Distinct())
                    {
                        context.AddImport(ns);
                    }
                }
            }
        }

        private async Task GetPackageDependencies(PackageIdentity package,
            NuGetFramework framework,
            SourceCacheContext cacheContext,
            SourceRepository repository,
            DependencyInfoResource dependencyInfoResource,
            ISet<SourcePackageDependencyInfo> availablePackages)
        {
            if (availablePackages.Contains(package)) return;

            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                package,
                framework, 
                cacheContext, 
                NullLogger.Instance, 
                CancellationToken.None);

            if (dependencyInfo == null)
            {
                return;
            }

            availablePackages.Add(dependencyInfo);
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                await GetPackageDependencies(
                    new PackageIdentity(
                        dependency.Id, 
                        dependency.VersionRange.MinVersion),
                    framework,
                    cacheContext,
                    repository, 
                    dependencyInfoResource,
                    availablePackages);
            }
        }

        private async Task<IEnumerable<string>> Install(
            DownloadResource downloadResource,
            PackageIdentity package,
            PackagePathResolver pathResolver,
            PackageExtractionContext extractionContext,
            FrameworkReducer reducer,
            NuGetFramework framework)
        {
            var packageResult = await downloadResource.GetDownloadResourceResultAsync(
                package,
                new PackageDownloadContext(cache),
                ".",
                NullLogger.Instance,
                CancellationToken.None);
            await PackageExtractor.ExtractPackageAsync(
                packageResult.PackageSource,
                packageResult.PackageStream,
                pathResolver, extractionContext,
                CancellationToken.None);

            var libItems = await packageResult.PackageReader.GetLibItemsAsync(CancellationToken.None);
            var nearest = reducer.GetNearest(framework, libItems.Select(a => a.TargetFramework));
            var selected = libItems.Where(a => a.TargetFramework.Equals(nearest)).SelectMany(a => a.Items);
            return selected.Where(a => Path.GetExtension(a) == ".dll")
                .Select(a => Path.Combine(pathResolver.GetInstalledPath(package), a));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cache.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
