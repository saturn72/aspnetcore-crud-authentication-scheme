using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aspnetcore_crud_authentication_scheme.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ProtectedController : ControllerBase
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public ProtectedController(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var s = await _authenticationSchemeProvider.GetRequestHandlerSchemesAsync();
            var currentSchemes = s.Select(d => d.DisplayName).ToArray();
            return "your schemes are : " + String.Join(", ", currentSchemes);
        }
    }
}