using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;

namespace CSDiscordService.Eval.ResultModels
{
    public class PropertiesObject
    {
        public PropertiesObject() { }
        public PropertiesObject(MetadataReferenceProperties properties)
        {
            Aliases = properties.Aliases.ToList();
            EmbedInteropTypes = properties.EmbedInteropTypes;
            Kind = properties.Kind;
        }

        public List<string> Aliases { get; set; }
        public bool EmbedInteropTypes { get; set; }
        public MetadataImageKind Kind { get; set; }
    }
}