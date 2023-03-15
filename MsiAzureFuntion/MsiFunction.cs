using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Identity.Client;

namespace MsiAzureFuntion
{
    public static class MsiFunction
    {
        [FunctionName("MsiFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string? userAssignedId = req.Query["userAssignedId"];
            string responseMessage;
            log.LogInformation("Get token from MSAL for managed identity.");
            try
            {
                IManagedIdentityApplication mi = string.IsNullOrEmpty(userAssignedId) ? 
                    ManagedIdentityApplicationBuilder.Create()
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .WithExperimentalFeatures()
                    .Build() : 
                    ManagedIdentityApplicationBuilder.Create(userAssignedId)
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .WithExperimentalFeatures()
                    .Build();

                var result = await mi.AcquireTokenForManagedIdentity("https://management.azure.com").ExecuteAsync().ConfigureAwait(false);

                responseMessage = "Access token received. Token Source: " + result.AuthenticationResultMetadata.TokenSource;
                
            }
            catch (MsalException ex)
            {
                responseMessage = ex.ToJsonString();
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message + ex.Source + ex.StackTrace;
            }

            return new OkObjectResult(responseMessage);
        }
    }
}
