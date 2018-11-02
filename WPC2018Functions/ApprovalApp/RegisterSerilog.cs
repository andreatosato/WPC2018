using Serilog;
using Serilog.Sinks.ILogger;

namespace ApprovalApp
{
    public static class RegisterSerilog
    {
        public static void UseSerilog(this Microsoft.Extensions.Logging.ILogger logger)
        {
            // Registrazione del tracewriter su Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ILogger(logger)
                .CreateLogger();
        }
    }
}
