using Microsoft.CodeAnalysis;

namespace CSDiscordService.Eval.ResultModels
{
    public class MetadataReferencesObject
    {
        public MetadataReferencesObject() { }
        public MetadataReferencesObject(MetadataReference metadataReference)
        {
            Display = metadataReference.Display;
            Properties = new PropertiesObject(metadataReference.Properties);
        }

        public string Display { get; set; }
        public PropertiesObject Properties { get; set; }
    }
}