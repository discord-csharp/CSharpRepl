using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

namespace CSDiscordService
{
    public static class ScriptExtensions
    {
        private static FieldInfo _compilerField;
        private static FieldInfo _optionsField;
        private static MethodInfo _langVersionSetter;
        private static MethodInfo _specificLangVersionSetter;

        public static Script<TScript> WithLanguageVersion<TScript>(this Script<TScript> script, LanguageVersion version)
        {
            var desiredValue = new object[] { version };

            var compilerField = _compilerField = _compilerField ?? typeof(Script<>).GetField("Compiler", BindingFlags.NonPublic | BindingFlags.Instance);
            var compiler = compilerField.GetValue(script);

            var optionsField = _optionsField = _optionsField ?? compiler.GetType().GetField("s_defaultOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var options = optionsField.GetValue(compiler);

            var langVersionSetter = _langVersionSetter = _langVersionSetter ?? typeof(CSharpParseOptions).GetMethod($"set_{ nameof(CSharpParseOptions.LanguageVersion)}", BindingFlags.NonPublic | BindingFlags.Instance);
            langVersionSetter.Invoke(options, desiredValue);

            var specifiedLangVerisonSetter = _specificLangVersionSetter = _specificLangVersionSetter ?? typeof(CSharpParseOptions).GetMethod($"set_{nameof(CSharpParseOptions.SpecifiedLanguageVersion)}", BindingFlags.NonPublic | BindingFlags.Instance);
            specifiedLangVerisonSetter.Invoke(options, desiredValue);

            return script;
        }
    }
}
