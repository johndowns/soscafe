using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SosCafe.Admin
{
    public static class KeepAlive
    {
        [FunctionName("KeepAlive")]
        public static void Run([TimerTrigger("0 */4 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Keep-alive function executed at: {DateTime.Now}");
        }
    }
}
