using System;

namespace CSDiscordService.Eval
{
    public class NugetPreProcessorDirective
    {
        internal const string Prefix = "#nuget";

        public string PackageId { get; }
        public string FeedUrl { get; }

        private NugetPreProcessorDirective(string packageId, string feedUrl)
        {
            PackageId = packageId;
            FeedUrl = feedUrl;
        }

        public static NugetPreProcessorDirective Parse(string directive)
        {
            if(directive is null)
            {
                throw new ArgumentNullException(nameof(directive));
            }

            if(!directive.StartsWith(Prefix))
            {
                throw new ArgumentException("Directive is not a valid nuget pre-processor directive.", nameof(directive));
            }

            var parts = directive.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length < 2)
            {
                throw new ArgumentException("#nuget directive must contain a package id");
            }

            var pkgId = parts[1];
            var feedUrl = "https://api.nuget.org/v3/index.json";
            if(parts.Length == 3)
            {
                feedUrl = parts[2];
            }

            return new NugetPreProcessorDirective(pkgId, feedUrl);
        }
    }
}
