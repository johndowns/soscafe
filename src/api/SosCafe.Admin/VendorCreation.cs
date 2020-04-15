using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SosCafe.Admin.Models.Api;
using SosCafe.Admin.Models.Queue;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Web.Http;

namespace SosCafe.Admin
{
    public static class VendorCreation
    {
        [FunctionName("AddVendorToShopify")]
        public static async Task<IActionResult> AddVendorToShopify(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] AddVendorQueueModel addVendorModel,
            HttpRequest req,
            ILogger log)
        {
            // TODO send request to Shopify

            var responseObject = new AddVendorToShopifyResultApiModel() { ShopifyId = Guid.NewGuid().ToString() };
            return new OkObjectResult(responseObject);
        }

        private static string SendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
        private static string SendGridTemplateId = Environment.GetEnvironmentVariable("SendGridTemplateId");
        private static string SendGridEmailFromAddress = Environment.GetEnvironmentVariable("SendGridEmailFromAddress");
        private static string SendGridEmailFromName = Environment.GetEnvironmentVariable("SendGridEmailFromName");

        [FunctionName("SendVendorWelcomeEmail")]
        public static async Task<IActionResult> SendVendorWelcomeEmail(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route =  null)] AddVendorQueueModel addVendorModel,
           HttpRequest req,
           ILogger log)
        {
            // Initialize the SendGrid client.
            var client = new SendGridClient(SendGridApiKey);

            // Prepare the email message.
            var emailMessage = new SendGridMessage();
            emailMessage.SetFrom(new EmailAddress(SendGridEmailFromAddress, SendGridEmailFromName));
            emailMessage.AddTo(new EmailAddress(addVendorModel.EmailAddress, addVendorModel.ContactName));
            emailMessage.SetTemplateId(SendGridTemplateId);
            
            // Send the message.
            var response = await client.SendEmailAsync(emailMessage);
            log.LogInformation("Sent mail via SendGrid and received status code {SendGridStatusCode} and headers {SendGridHeaders}.", response.StatusCode, response.Headers.ToString());

            if ((int)response.StatusCode > 299)
            {
                log.LogError("Unable to send email.");
                return new InternalServerErrorResult();
            }            
            return new OkResult();
        }
    }
}
