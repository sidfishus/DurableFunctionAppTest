using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionAppTest
{
    public static class Function1
    {

        // This is the external public HTTP trigger function.
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Start the orchestration method and return a JSON so the caller can asynchronously check for updates.
            string instanceId = await starter.StartNewAsync("TaskSequence", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        // The orchestration method that orchestrates multiple tasks.
        [FunctionName("TaskSequence")]
        public static async Task<IList<string>> TaskSequence(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync("Task1", null);
            var outputs = new List<string>();

            // This becomes the output of the function app, accessible via a HTTP get call to the statusQueryGetUri
            outputs.Add("Output from Tasksequence");

            return outputs;
        }

        [FunctionName("Task1")]
        public static async Task Task1([ActivityTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            // A task that takes 30 seconds.
            await Task.Delay(1000 * 30);
        }
    }
}