using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;


using Microsoft.Azure.Devices.Provisioning.Service;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Company.Function
{
    public static class registerDeviceDPS
    {
        [FunctionName("registerDeviceDPS")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["deviceid"];

        
        string primaryKey = "";
         var credential = new ChainedTokenCredential(
                new ManagedIdentityCredential(),
             new VisualStudioCodeCredential());
             var client = new SecretClient(new Uri("https://deviceonboardingkeyvault.vault.azure.net/"), credential);
             var secret = client.GetSecret("dpsconnectionstring");

         using (var provisioningServiceClient = ProvisioningServiceClient.CreateFromConnectionString(secret.Value.Value)){
             var attestation = new SymmetricKeyAttestation("","");
             var ie = new IndividualEnrollment(name,attestation);
             var enrollment = await provisioningServiceClient.CreateOrUpdateIndividualEnrollmentAsync(ie);
             var keys = enrollment.Attestation as SymmetricKeyAttestation;
             primaryKey = keys.PrimaryKey;
             log.LogInformation(enrollment.IotHubHostName);
         }

            return new OkObjectResult(primaryKey);
        }
    }

}
