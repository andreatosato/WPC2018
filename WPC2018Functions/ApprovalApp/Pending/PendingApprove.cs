using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Serilog;

namespace ApprovalApp
{
    public static class PendingApprove
    {
        [FunctionName("PendingApprove")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Pending")]HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient starter,
            Microsoft.Extensions.Logging.ILogger logger)
        {
            // Registrazione del tracewriter su Serilog
            logger.UseSerilog();

            string instanceId = await starter.StartNewAsync("BudgetApproval", null);
            // Verifica completamento lavoro...
            Log.Information($"Inizio Orchestratore con ID = '{instanceId}'.");
            var res = starter.CreateCheckStatusResponse(req, instanceId);
            return res;
        }
    }
}
