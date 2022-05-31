using AuthExampleYT2.Helper;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace AuthExampleYT2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
            services.AddHttpContextAccessor();

            //var multiSchemePolicy = new AuthorizationPolicyBuilder(
            //    CookieAuthenticationDefaults.AuthenticationScheme,
            //    JwtBearerDefaults.AuthenticationScheme)
            //.RequireAuthenticatedUser()
            //.Build();

            //services.AddAuthorization(o => o.DefaultPolicy = multiSchemePolicy);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie("Cookies", options =>
                {
                    //These three items are apparently needed by Teams to make a cookie.
                    //https://docs.microsoft.com/en-us/microsoftteams/platform/resources/samesite-cookie-update#samesite-cookie-attribute-2020-release
                    options.Cookie.SameSite = SameSiteMode.None;
                    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    //options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

                    options.LoginPath = "/login";
                    options.AccessDeniedPath = "/denied";

                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnSigningIn = async context =>
                        {
                            await Task.CompletedTask;
                        },
                        OnSignedIn = async context =>
                        {
                            await Task.CompletedTask;
                        },
                        OnValidatePrincipal = async context =>
                        {
                            await Task.CompletedTask;
                        }
                    };
                })
                //.AddOAuth("OAuth", options =>
                //{
                //    options.SaveTokens = true;
                //    options.AccessDeniedPath = "/denied";
                //    options.CallbackPath = "/Home/Secured";
                //    options.AuthorizationEndpoint = "https://login.microsoftonline.com/common";
                //    options.ClientId = "«ClientId»";
                //    options.ClientSecret = "«AppSecret»";
                //    options.TokenEndpoint = "api://«AppURI»/«ClientId»";

                //    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents()
                //    {
                //        OnCreatingTicket = async context =>
                //        {
                //            await Task.CompletedTask;
                //        },
                //        OnRedirectToAuthorizationEndpoint = async context =>
                //        {
                //            await Task.CompletedTask;
                //        },
                //        OnTicketReceived = async context =>
                //        {
                //            await Task.CompletedTask;
                //        }
                //    };
                //})
                .AddOpenIdConnect("microsoft", options =>
                {
                    options.Authority = "https://login.microsoftonline.com/common";
                    options.ClientId = "«ClientId»";
                    options.ClientSecret = "«AppSecret»";
                    options.CallbackPath = "/signin-oidc";
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.SaveTokens = true;
                    options.Prompt = "login";

                    options.Events = new OpenIdConnectEvents()
                    {
                        OnRedirectToIdentityProvider = async context =>
                        {
                            await Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            await Task.CompletedTask;
                        }
                    };
                })
                .AddJwtBearer("Bearer", options =>
                {
                    var azureAdOptions = new AzureADOptions();
                    Configuration.Bind("AzureAd", azureAdOptions);
                    //options.Authority = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/v2.0";
                    options.Authority = $"{azureAdOptions.Instance}common/v2.0";
                                        
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidAudiences = SSOAuthHelper.GetValidAudiences(Configuration),
                        ValidIssuers = SSOAuthHelper.GetValidIssuers(Configuration),
                        AudienceValidator = SSOAuthHelper.AudienceValidator
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
