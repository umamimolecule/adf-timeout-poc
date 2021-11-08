using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace ADFTimeoutPOC
{
    public class TestOrchestrator
    {
        public const string FunctionName = nameof(TestOrchestrator);

        private readonly RetryOptions retryOptions;

        public TestOrchestrator()
        {
            this.retryOptions = new RetryOptions(TimeSpan.FromSeconds(1), 1)
            {
                BackoffCoefficient = 2,
                Handle = this.Handle,
            };
        }
        
        [FunctionName(FunctionName)]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                await context.CallActivityWithRetryAsync(TestActivity.FunctionName, this.retryOptions, null);
            }
            catch (Exception e)
            {
                if (e.InnerException is TaskCanceledException)
                {
                    // Have exhausted all retries
                    Console.WriteLine("*** Orchestration ended due to timeout ***");
                    return;
                }

                throw;
            }
        }

        private bool Handle(Exception exception)
        {
            if (exception.InnerException is TaskCanceledException)
            {
                return true;
            }

            return false;
        }
    }
}