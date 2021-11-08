using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ADFTimeoutPOC
{
    public class TestApi
    {
        public const string FunctionName = nameof(TestApi);
            
        [FunctionName(FunctionName)]
        public async Task<IActionResult> ExecuteAsync(
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            var instanceId = Guid.NewGuid().ToString();
            await orchestrationClient.StartNewAsync(TestOrchestrator.FunctionName, instanceId);
            return new ObjectResult(new {instanceId})
            {
                StatusCode = (int)HttpStatusCode.Accepted,
            };
        }
    }
}