using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
namespace keycloakdavid.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        [AllowAnonymous]
        [HttpGet("login")]
        [SwaggerOperation(Summary = "Initiate login via Keycloak")]
        [SwaggerResponse(302, "Redirects to Keycloak login page")]
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("logout")]
        [SwaggerOperation(Summary = "Logout from the application and Keycloak")]
        [SwaggerResponse(302, "Redirects to the home page after logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        [HttpGet("profile")]
        [SwaggerOperation(Summary = "Retrieve the authenticated user's profile")]
        [SwaggerResponse(200, "Returns the user's profile", typeof(UserProfile))]
        public IActionResult Profile()
        {
            return Ok(new UserProfile
            {
                Name = User.Identity.Name,
                Claims = User.Claims.ToDictionary(c => c.Type, c => c.Value)
            });
        }

        [HttpGet("isauthenticated")]
        [SwaggerOperation(Summary = "Check if the user is authenticated")]
        [SwaggerResponse(200, "Returns a message if the user is authenticated", typeof(string))]
        public IActionResult IsAuthenticated()
        {
            // Si cette ligne est atteinte, l'utilisateur est authentifié grâce à l'attribut [Authorize]
            return Ok(new { Message = "Tu es connecté" });
        }
    }

    public class UserProfile
    {
        public string Name { get; set; }
        public Dictionary<string, string> Claims { get; set; }
    }
}


