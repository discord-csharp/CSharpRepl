using Microsoft.Azure.WebJobs.Host;
using System.Diagnostics;

namespace CSDiscordFunctionTests
{
    class DummyWriter : TraceWriter
    {
        public DummyWriter(TraceLevel level) : base(level)
        {
        }

        public override void Trace(TraceEvent traceEvent)
        {
        }
    }
}
