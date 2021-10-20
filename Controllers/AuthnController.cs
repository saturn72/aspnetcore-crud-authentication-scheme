using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace aspnetcore_crud_authentication_scheme.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthnController : ControllerBase
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly OpenIdConnectOptionsFactory _factory;

        public AuthnController(
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            OpenIdConnectOptionsFactory factory)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _factory = factory;
        }
        [HttpGet]
        public async Task<object> GetAll()
        {
            var allScehmes = await _authenticationSchemeProvider.GetAllSchemesAsync();
            return allScehmes.Select(s => new
            {
                s.Name,
                s.DisplayName,
                handlerName = s.HandlerType.Name,
            });
        }
        [HttpPost]
        public async Task<bool> Post()
        {
            //adds oidc2 scheme
            var toAdd = new AuthenticationScheme("oidc2", "oidc2", typeof(OpenIdConnectHandler));
            if (!_authenticationSchemeProvider.TryAddScheme(toAdd))
                return false;
            var a = new Action<OpenIdConnectOptions>(options =>
            {
                options.Authority = "https://demo.identityserver.io/";
                options.ClientId = "c-id2";
            });

            _factory.AddOption("oidc2", a);

            //clear cache
            var allSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
            var services = Request.HttpContext.RequestServices;
            var c = services.GetService<IOptionsMonitorCache<OpenIdConnectOptions>>();
            foreach (var s in allSchemes)
                c.TryRemove(s.Name);
            return true;
        }
    }
}
