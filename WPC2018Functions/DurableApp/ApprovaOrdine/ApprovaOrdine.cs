using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Serilog;

namespace DurableApp
{
    public static class ApprovaOrdine
    {
        [FunctionName("ApprovaOrdine")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ApprovaOrdine")]HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient starter,
            Microsoft.Extensions.Logging.ILogger logger)
        {
            logger.UseSerilog();

            string ordineId = req.GetQueryParameterDictionary().First(x => x.Key == "ordineId").Value;
            Log.Information($"Approva Ordine {ordineId}");

            await starter.RaiseEventAsync(ordineId, Workflow.EventoApprova, true);
            return new AcceptedResult();
        }
    }
}
