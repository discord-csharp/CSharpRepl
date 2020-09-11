using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSDiscordService.Eval
{
    public class DefaultPreProcessorService : IPreProcessorService
    {
        private readonly IEnumerable<IDirectiveProcessor> _directives;

        public DefaultPreProcessorService(IEnumerable<IDirectiveProcessor> directives)
        {
            _directives = directives;
        }
        public async Task PreProcess(ScriptExecutionContext context, Action<string> logger)
        {
            var codeLines = context.Code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var remainingLines = new List<string>();

            foreach(var line in codeLines)
            {
                var lineHandled = false;
                foreach(var directive in _directives)
                {
                    if (lineHandled)
                    {
                        continue;
                    }

                    var canProcess = directive.CanProcessDirective(line);
                    if(!canProcess)
                    {
                        continue;
                    }

                    lineHandled = true;
                    await directive.PreProcess(line, context, logger);
                }

                if(!lineHandled)
                {
                    remainingLines.Add(line);
                }
            }

            context.Code = string.Join(Environment.NewLine, remainingLines);
        }
    }
}
