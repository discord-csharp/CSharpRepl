using System.Threading.Tasks;

namespace CSDiscordService.Eval
{
    public interface IDirectiveProcessor
    {
        bool CanProcessDirective(string directive);
        Task PreProcess(string directive, ScriptExecutionContext context);
    }
}
