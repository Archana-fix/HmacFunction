using System;
using System.IO;
using System.Threading.Tasks;
using AzureHmac.Model;
using HMacToken;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureHmac.Controller
{
    public class HmacFunction
    {
        private readonly ILogger _logger;

        public HmacFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HmacFunction>();
        }

        [Function("GenerateHMAC")]
        public async Task<HttpResponseData> GenerateHMAC(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "generate-hmac")] HttpRequestData req)
        {
            _logger.LogInformation("GenerateHMAC function processed a request from main");
            HttpResponseData response;
            try
            {
                // Read and deserialize the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                HmacRequest? request = JsonConvert.DeserializeObject<HmacRequest>(requestBody);

                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request), "Request body is invalid or empty.");
                }

                // Generate HMAC token
                HMacTokenClass hmacToken = new HMacTokenClass();
                string hmac = hmacToken.IdeationmHmac(request);

                // Create response
                response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { Hmac = hmac });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating HMAC token");
                response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new { Error = ex.Message });
            }

            return response;
        }
    }
}
