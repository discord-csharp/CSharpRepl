using ICSharpCode.Decompiler.Util;
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
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return directive != null && directive.StartsWith("#nuget");
        }

        public async Task PreProcess(string directive, ScriptExecutionContext context, Action<string> logger)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            var actionLogger = new NugetLogger(logger);
            var nugetDirective = NugetPreProcessorDirective.Parse(directive);
            string frameworkName = typeof(NugetDirectiveProcessor).Assembly.GetCustomAttributes(true)
              .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
              .Select(x => x.FrameworkName)
              .FirstOrDefault()!;
            NuGetFramework framework = frameworkName == null
              ? NuGetFramework.AnyFramework
              : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());

            using var cache = new SourceCacheContext();
            var packagesPath = Path.Combine(Path.GetTempPath(), "packages");

            await CreateEmptyNugetConfig(packagesPath, nugetDirective.FeedUrl);

            var settings = Settings.LoadImmutableSettingsGivenConfigPaths(new[] { Path.Combine(packagesPath, "empty.config")  }, new SettingsLoadingContext());
            
            var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
            #pragma warning disable CS0618 // Type or member is obsolete
            var repositoryProvider = new SourceRepositoryProvider(settings, Repository.Provider.GetCoreV3());
            #pragma warning restore CS0618 // Type or member is obsolete

            var repository = repositoryProvider.GetRepositories().FirstOrDefault();
            var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>(CancellationToken.None);
            var searchMetadata = await packageMetadataResource.GetMetadataAsync(
                nugetDirective.PackageId,
                includePrerelease: false,
                includeUnlisted: false,
                cache,
                actionLogger,
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
                availablePackages, actionLogger);

            var resolverContext = new PackageResolverContext(
                DependencyBehavior.Lowest,
                new[] { nugetDirective.PackageId },
                Enumerable.Empty<string>(),
                Enumerable.Empty<PackageReference>(),
                Enumerable.Empty<PackageIdentity>(),
                availablePackages,
                new[] { repository.PackageSource },
                actionLogger);

            var resolver = new PackageResolver();
            var toInstall = resolver.Resolve(resolverContext, CancellationToken.None)
                .Select(a => availablePackages.Single(b => PackageIdentityComparer.Default.Equals(b, a)));

            var pathResolver = new PackagePathResolver(packagesPath);
            var extractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3,
                XmlDocFileSaveMode.None,
                ClientPolicyContext.GetClientPolicy(settings, actionLogger),
                actionLogger);
            var libraries = new List<string>();
            var frameworkReducer = new FrameworkReducer();
            var downloadResource = await repository.GetResourceAsync<DownloadResource>(CancellationToken.None);
            foreach (var package in toInstall)
            {
                libraries.AddRange(await Install(downloadResource, package, pathResolver, extractionContext, frameworkReducer, framework, packagesPath, actionLogger));
            }

            foreach (var path in libraries)
            {
                var asm = Assembly.LoadFile(path);
                if (context.References.Add(asm))
                {
                    foreach (var type in asm.GetTypes())
                    {
                        if (type.IsPublic && type.Namespace is not null)
                            context.Imports.Add(type.Namespace);
                    }
                }
            }
        }

        private async Task CreateEmptyNugetConfig(string packagesPath, string feedUrl)
        {
            var filename = "empty.config";
            var fullPath = Path.Combine(packagesPath, filename);
            Directory.CreateDirectory(packagesPath);
            
            if(!File.Exists(fullPath))
            {
                await File.WriteAllTextAsync(fullPath, 
                    $"""
                        <?xml version="1.0" encoding="utf-8"?>
                        <configuration>
                            <config>
                                <add key="repositoryPath" value="{packagesPath}" />
                                <add key="globalPackagesFolder" value="{packagesPath}" />
                            </config>
                            <packageSources>
                                <add key="Feed" value="{feedUrl}" />
                            </packageSources>
                        </configuration>
                        """
                    );
            }
        }

        private async Task GetPackageDependencies(PackageIdentity package,
            NuGetFramework framework,
            SourceCacheContext cacheContext,
            SourceRepository repository,
            DependencyInfoResource dependencyInfoResource,
            ISet<SourcePackageDependencyInfo> availablePackages,
            ILogger logger)
        {
            if (availablePackages.Contains(package)) return;

            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                package,
                framework,
                cacheContext,
                logger,
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
                    availablePackages, logger);
            }
        }

        private async Task<IEnumerable<string>> Install(
            DownloadResource downloadResource,
            PackageIdentity package,
            PackagePathResolver pathResolver,
            PackageExtractionContext extractionContext,
            FrameworkReducer reducer,
            NuGetFramework framework,
            string packagesPath,
            ILogger logger)
        {
            var packageResult = await downloadResource.GetDownloadResourceResultAsync(
                package,
                new PackageDownloadContext(cache),
                packagesPath,
                logger,
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
        
        public void Dispose()
        {
            cache.Dispose();
            disposedValue = true;
        }

        private class NugetLogger : ILogger
        {
            private readonly Action<string> _logger;

            public NugetLogger(Action<string> logger)
            {
                _logger = logger;
            }
            public void Log(LogLevel level, string data)
            {
                _logger($"{level}: {data}");
            }

            public void Log(ILogMessage message)
            {
                _logger($"{message.Level}: {message.Message}");
            }

            public Task LogAsync(LogLevel level, string data)
            {
                _logger($"{level}: {data}");
                return Task.CompletedTask;
            }

            public Task LogAsync(ILogMessage message)
            {
                _logger($"{message.Level}: {message.Message}");
                return Task.CompletedTask;
            }

            public void LogDebug(string data)
            {
                Log(LogLevel.Debug, data);
            }

            public void LogError(string data)
            {
                Log(LogLevel.Error, data);
            }

            public void LogInformation(string data)
            {
                Log(LogLevel.Information, data);
            }

            public void LogInformationSummary(string data)
            {
                Log(LogLevel.Information, data);
            }

            public void LogMinimal(string data)
            {
                Log(LogLevel.Minimal, data);
            }

            public void LogVerbose(string data)
            {
                Log(LogLevel.Debug, data);
            }

            public void LogWarning(string data)
            {
                Log(LogLevel.Warning, data);
            }
        }

    }
}
