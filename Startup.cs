using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace aspnetcore_crud_authentication_scheme
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

            services.AddSingleton<OpenIdConnectOptionsFactory>();
            services.AddSingleton<IOptionsFactory<OpenIdConnectOptions>>(sp => sp.GetService<OpenIdConnectOptionsFactory>());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "aspnetcore_crud_authentication_scheme", Version = "v1" });
            });
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "oidc";
                options.DefaultChallengeScheme = "oidc";
            })
                //add some authentication at startup
                .AddOpenIdConnect("oidc1", options =>
                 {
                     options.Authority = "https://demo.identityserver.io/";
                     options.ClientId = "c-id1";
                     options.SignInScheme = "oidc";
                 });

            /*********************************************************************************************************************/
            //the code snippet below is required for authn middleware and is not required for the add/remove scheme functionality
            /*********************************************************************************************************************/
            authenticationBuilder.AddPolicyScheme("oidc",
            "AuthenticationSelector",
            options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (!context.Request.Headers.TryGetValue(HeaderNames.Authorization, out var allAuthnHeaders) || StringValues.IsNullOrEmpty(allAuthnHeaders))
                        return "";
                    var header = allAuthnHeaders.First();
                    var spaceIndex = header.IndexOf(' ');
                    return header[..spaceIndex];
                };
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "aspnetcore_crud_authentication_scheme v1"));
            }

            app.UseHttpsRedirection();
            // app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
