using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DurableApp.OrdineCliente.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog;

namespace DurableApp.InviaMail
{
    public static class AttendiOrdineCliente
    {
        [FunctionName(Workflow.AttendiOrdineCliente)]
        public static async Task Run(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            [Table("ApprovaOrdineCliente")] CloudTable approvaOrdineTables,
            Microsoft.Extensions.Logging.ILogger logger)
        {
            logger.UseSerilog();
            Log.Information($"SubOrchestrator Instance: {context.InstanceId}");
            OrdiniAcquistoModel ordineAcquisto = context.GetInput<OrdiniAcquistoModel>();
            Log.Information($"Pending Order {ordineAcquisto.IdOrdine}");
            await SendMail(context, ordineAcquisto);

            string status = string.Empty;
            using (var timeoutCts = new CancellationTokenSource())
            {
                DateTime dueTime = context.CurrentUtcDateTime.AddHours(5);
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                Task<bool> approvalTask = context.WaitForExternalEvent<bool>(Workflow.EventoApprova);
                //Attendo un evento o un timer
                if (approvalTask == await Task.WhenAny(approvalTask, durableTimeout))
                {
                    timeoutCts.Cancel();
                    if (await approvalTask)
                    {
                        status = "Approvato";
                    }
                }
                else
                {
                    timeoutCts.Cancel();
                    status = "TempoScaduto";
                }
                Log.Warning(status);
            }
           
            var approvaOrdine = new ApprovaOrdineTable
            {
                PartitionKey = context.InstanceId,
                RowKey = "PendingApproval",
                OrdineId = ordineAcquisto.IdOrdine,
                IdConfirmation = context.InstanceId,
                Status = status
            };

            TableOperation insertOperation = TableOperation.InsertOrReplace(approvaOrdine);
            await approvaOrdineTables.ExecuteAsync(insertOperation);
        }

        /// <summary>
        /// Invia notifica ordine via Mail
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ordineAcquisto"></param>
        /// <returns></returns>
        private static async Task SendMail(DurableOrchestrationContext context, OrdiniAcquistoModel ordineAcquisto)
        {
            string mailInstance = await context.CallActivityWithRetryAsync<string>(Workflow.InviaMailOrdineCliente,
                new RetryOptions(TimeSpan.FromSeconds(5), 10),
                new OrdiniAcquistoModel
                {
                    IdConfirmation = context.InstanceId,
                    IdOrdine = ordineAcquisto.IdOrdine,
                    Articoli = ordineAcquisto.Articoli,
                    ClienteCorrente = ordineAcquisto.ClienteCorrente
                });
            Log.Information($"OrdineClienteManager: MailInstance {mailInstance}");
        }

        public class ApprovaOrdineTable : TableEntity, ITableEntity
        {
            public string OrdineId { get; set; }
            public string IdConfirmation { get; set; }
            public string Status { get; set; }
        }        
    }
}
