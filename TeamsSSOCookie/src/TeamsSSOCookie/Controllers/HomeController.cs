using AuthExampleYT2.Helper;
using AuthExampleYT2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthExampleYT2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("denied")]
        public  IActionResult Denied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secured()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");
            //var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SecuredXL()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            //DelegateAuthenticationProvider provider = new DelegateAuthenticationProvider(
            //    async (requestMessage) =>
            //    {
            //        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", idToken);
            //    });

            //GraphServiceClient graphClient = new GraphServiceClient(provider);
            //User me = await graphClient.Me.Request().GetAsync();

            //var claim = new Claim("Profile Image", me.Photo.ToString());
            //var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            //claimsIdentity.AddClaim(claim);

            return View();
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("loginmicrosoft")]
        public IActionResult LoginExternal([FromRoute] string provider, [FromQuery] string returnUrl)
        {
            if (User != null && User.Identities.Any(identity => identity.IsAuthenticated))
            {
                return RedirectToAction("", "Home");
            }

            // By default the client will be redirect back to the URL that issued the challenge (/login?authtype=foo),
            // send them to the home page instead (/).
            returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
            var authenticationProperties = new AuthenticationProperties { RedirectUri = returnUrl };
            // authenticationProperties.SetParameter("prompt", "select_account");
            return new ChallengeResult("microsoft", authenticationProperties);
        }

        [HttpPost("login")]
        public IActionResult Validate(string username, string password, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            //if (username == "jeremy" && password == "dr3g0th")
            //{
            //    var claims = new List<Claim>();
            //    claims.Add(new Claim("username", username));
            //    claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
            //    claims.Add(new Claim(ClaimTypes.Name, "Jeremy Messer"));
            //    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            //    await HttpContext.SignInAsync(claimsPrincipal);
            //    return Redirect(returnUrl);
            //}
            TempData["Error"] = "Error. Username or password is invalid.";
            return View("login");
        }

        //[HttpPost("loginmicrosoft")]
        //public IActionResult ValidateX(string returnUrl)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;
        //    //if (username == "jeremy" && password == "dr3g0th")
        //    //{
        //    //    var claims = new List<Claim>();
        //    //    claims.Add(new Claim("username", username));
        //    //    claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
        //    //    claims.Add(new Claim(ClaimTypes.Name, "Jeremy Messer"));
        //    //    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //    //    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        //    //    await HttpContext.SignInAsync(claimsPrincipal);
        //    //    return Redirect(returnUrl);
        //    //}
        //    TempData["Error"] = "Error. Username or password is invalid.";
        //    return View("login");
        //}

        [HttpGet("loginteamssso")]
        public async Task<ActionResult> ValidateTeamsSSO([FromQuery] string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var httpContext = _httpContextAccessor.HttpContext;
            httpContext.Request.Headers.TryGetValue("Authorization", out StringValues assertion);
            var idToken = assertion.ToString().Split(" ")[1];
            httpContext.Request.Headers.TryGetValue("returnurl", out StringValues returnUrlFromHeader);
            returnUrl = returnUrlFromHeader.ToString();
            returnUrl = returnUrl ?? "/";

            var body = $"assertion={idToken}&requested_token_use=on_behalf_of&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&client_id={_configuration["AzureAd:ClientId"]}&client_secret={_configuration["AzureAd:AppSecret"]}&scope=https://graph.microsoft.com/User.Read";
            try
            {
                var client = _httpClientFactory.CreateClient("WebClient");
                string responseBody;
                using (var request = new HttpRequestMessage(HttpMethod.Post, _configuration["AzureAd:Instance"] +  "common" + _configuration["AzureAd:AuthUrl"]))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseBody);
                        }
                    }
                }

                var accessToken = JsonConvert.DeserializeObject<dynamic>(responseBody).access_token;

                DelegateAuthenticationProvider providerX = new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken.ToString());
                    });

                GraphServiceClient graphClient = new GraphServiceClient(providerX);
                User me = await graphClient.Me.Request().GetAsync();

                var claims = new List<Claim>();
                claims.Add(new Claim("username", me.Id));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, me.Id));
                claims.Add(new Claim(ClaimTypes.Name, me.GivenName + " " + me.Surname));
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                //HttpContext.Response.Cookies.Append(
                //     "name", "value",
                //     new CookieOptions() { SameSite = SameSiteMode.Lax });

                await HttpContext.SignInAsync("Cookies", claimsPrincipal);
                Response.StatusCode = (int)HttpStatusCode.OK;
                //return Redirect(returnUrl);
                return Json(new { accessToken = accessToken}); 
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error. " + ex.Message;
                ViewData["Error"] = "Error. " + ex.Message;

                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { errorMessage = "Error. " + ex.Message });
                //return null;
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [Authorize]
        [HttpGet("GetUserAccessToken")]
        public async Task<ActionResult<string>> GetUserAccessToken()
        {
            try
            {
                return await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
