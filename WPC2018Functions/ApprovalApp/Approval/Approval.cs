using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalApp
{
    public static class Approval
    {
        [FunctionName("Approval")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Approval")]HttpRequestMessage req,
             [OrchestrationClient] DurableOrchestrationClient starter,
             Microsoft.Extensions.Logging.ILogger logger)
        {
            logger.UseSerilog();

            var payload = await req.Content.ReadAsAsync<Payload>();
            await starter.RaiseEventAsync(payload.InstanceId, "Approve", payload.Result);

            return new AcceptedResult();
        }

        public class Payload
        {
            public string InstanceId { get; set; }
            public bool Result { get; set; }
        }
    }
}
