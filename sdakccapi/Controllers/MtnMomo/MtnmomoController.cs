using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sdakccapi.Dtos.MtnMomoDto;
using sdakccapi.StaticDetails;
using System.Text;

namespace sdakccapi.Controllers.MtnMomo
{
    [Route("api/[controller]")]
    [ApiController]
    public class MtnmomoController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly MtnMomoConfig _mtnConfig;
        private static MtnTokenObj _mtnTokenObj;

        public MtnmomoController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
            _mtnConfig = JsonConvert.DeserializeObject<MtnMomoConfig>(_configuration.GetSection("MtnMomo").Value)??new MtnMomoConfig();
            if (_httpClient.BaseAddress == null) _httpClient.BaseAddress = new Uri(_mtnConfig.Url);
           
        }

        [NonAction]
        public async Task<ActionResult> RequestToPay([FromBody] RequestToPayDto model)
        {
            // Set the API endpoint and authorization header
            string endpoint = "collection/v1_0/requesttopay";
            
            //Check or get User Token
            if(statics.MtnTokenObj is not null && statics.MtnTokenObj.ExpiryTime<DateTime.Now)
            {
                _mtnTokenObj = statics.MtnTokenObj;
            }           
            else
            {
                _mtnTokenObj = await CreateSessionToken(_mtnConfig.ApiKey, _mtnConfig.ApiUser);
                if (_mtnTokenObj is null) return BadRequest("Error while Authenticating with MtnMomo");
            }
            string authHeader = $"Bearer {_mtnTokenObj.Token}";

            string transactionID = Guid.NewGuid().ToString();

            // Set the authorization header
            _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            _httpClient.DefaultRequestHeaders.Add("X-Reference-Id", transactionID);
            _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", _mtnConfig.TargetEnvironment);
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _mtnConfig.SecondaryKey);
            _httpClient.DefaultRequestHeaders.Add("X-Callback-Url", model.CallBackUrl);

            // Set the request body
            var requestBody = new
            {
                amount = model.Amount,
                currency = model.Currency,
                externalId = model.ExternalId,
                payer = model.Payer,
                payeeNote = model.PayeeNote,
                payerMessage = model.PayerMessage
            };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the POST request
            var response = await _httpClient.PostAsync(endpoint, content);

            // Check the response status code
            if (response.IsSuccessStatusCode)
            {
                // Return the response body
                return Ok(new {requestID = transactionID });
            }
            else
            {
                // Return an error
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }

        [NonAction]
        public async Task<ActionResult> CheckPaymentStatus(string paymentID)
        {
            // Set the API endpoint and authorization header
            string endpoint = $"collection/v1_0/requesttopay/{paymentID}";

            

            string transactionID = Guid.NewGuid().ToString();

            // Set the authorization header
            
            _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", _mtnConfig.TargetEnvironment);
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _mtnConfig.SecondaryKey);


            // Send the GET request
            var response = await _httpClient.GetAsync(endpoint);

            // Check the response status code
            if (response.IsSuccessStatusCode)
            {
                // Return the response body
                return Ok(JsonConvert.DeserializeObject<RequestToPayDto>(await response.Content.ReadAsStringAsync()));
            }
            else
            {
                // Return an error
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
        #region SandBox Provisioning Methods

        [NonAction]
        private async Task<string> CreateApiUser()
        {
            string endpoint = "v1_0/apiuser";
            string apiUserID = Guid.NewGuid().ToString();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Reference-Id", apiUserID);
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _mtnConfig.SecondaryKey);

            var requestBody = new
            {
                // Set the request body parameters
                providerCallbackHost = "https://webhook.site/b0f50d5e-4031-464c-9eb0-a31c6aefc2b7"
            };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                return apiUserID;
            }
            return "";
        }

        [NonAction]
        private async Task<string> CreateApiKey(string apiUserID)
        {
            string endpoint = $"v1_0/apiuser/${apiUserID}/apikey";


            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _mtnConfig.SecondaryKey);



            var content = new StringContent(null, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var retunedObj = await response.Content.ReadAsStringAsync();
                string apiKey = JsonConvert.DeserializeObject<JToken>(retunedObj)?["apiKey"]?.ToString() ?? "";
                return apiKey;
            }
            return "";
        }

        #endregion
        private async Task<MtnTokenObj> CreateSessionToken(string apiKey, string apiUserID)
        {
            string endpoint = $"/collection/token/";


            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _mtnConfig.SecondaryKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode($"{apiUserID}:{apiKey}")}");



            var content = new StringContent(null, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var retunedObj = await response.Content.ReadAsStringAsync();
                string accessToken = JsonConvert.DeserializeObject<JToken>(retunedObj)?["access_token"]?.ToString() ?? "";
                int expiryTime = int.Parse(JsonConvert.DeserializeObject<JToken>(retunedObj)?["expires_in"]?.ToString()??"0");
                var tokenObj = new MtnTokenObj()
                {
                    Token = accessToken,
                    ExpiryTime = DateTime.Now.AddSeconds(expiryTime).AddMinutes(-10)
                };
                statics.MtnTokenObj = tokenObj;
                return tokenObj;
            }
            return null;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText??"");
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
