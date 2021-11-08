using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace ADFTimeoutPOC
{
    public class TestActivity
    {
        public const string FunctionName = nameof(TestActivity);

        private const int ActivityTimeout = 5000;
        
        private const int ActivityDuration = 7000;
        
        [FunctionName(FunctionName)]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            await this.RunWithTimeout(this.RunInternal);
        }

        private async Task<string> RunInternal(CancellationToken cancellationToken)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds <= ActivityDuration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"*** CANCELLATION REQUESTED ***");
                    return "Cancelled";
                }
                
                await Task.Delay(1000);
            }

            return "Completed";
        }
        
        private async Task<TResult> RunWithTimeout<TResult>(Func<CancellationToken, Task<TResult>> action)
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource(ActivityTimeout))
            {
                TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
                CancellationToken token = tokenSource.Token;

                token.Register(() =>
                {
                    taskCompletionSource.TrySetCanceled();
                });

                var task = action.Invoke(token);
                var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);

                return await completedTask;
            }
        }
    }
}