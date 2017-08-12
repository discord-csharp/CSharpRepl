using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.Linq;

namespace CSDiscordService.Eval.ResultModels
{
    public class OptionsObject
    {
        public OptionsObject() { }
        public OptionsObject(ScriptOptions options)
        {
            EmitDebugInformation = options.EmitDebugInformation;
            FileEncoding = options.FileEncoding;
            FilePath = options.FilePath;
            Imports = options.Imports.ToList();
            MetadataReferences = options.MetadataReferences.Select(a => new MetadataReferencesObject(a)).ToList();
        }

        public bool EmitDebugInformation { get; set; }
        public Encoding FileEncoding { get; set; }
        public string FilePath { get; set;  }
        public List<string> Imports { get; set; }
        public List<MetadataReferencesObject> MetadataReferences { get; set; } = new List<MetadataReferencesObject>();
            
    }
}