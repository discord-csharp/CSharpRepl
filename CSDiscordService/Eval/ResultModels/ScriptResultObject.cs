using System;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.Linq;

namespace CSDiscordService.Eval.ResultModels
{
    public class ScriptResultObject
    {
        public ScriptResultObject() { }
        public ScriptResultObject(ScriptState<object> state) => FromState(state);

        public static ScriptResultObject FromState(ScriptState<object> state)
        {
            state = state ?? throw new ArgumentNullException(nameof(state));
            var result = new ScriptResultObject
            {
                Exception = state.Exception,
                ReturnValue = state.ReturnValue,
                Variables = state.Variables.Select(a => new VariableObject(a)).ToList(),
                Script = new ScriptObject(state.Script)
            };
            return result;
        }

       public Exception Exception { get; set; }
       public object ReturnValue { get; set; }
       public List<VariableObject> Variables { get; set; } = new List<VariableObject>();
       public ScriptObject Script { get; set; }
    }
}