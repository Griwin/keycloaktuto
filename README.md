
---

# Intégration de Keycloak avec ASP.NET Core

Ce guide a pour but de vous montrer comment intégrer Keycloak avec une application ASP.NET Core pour gérer l'authentification et l'autorisation.

## Prérequis

- Docker
- .NET SDK
- Un IDE (comme Visual Studio ou VS Code)

## Installation de Keycloak via Docker

Pour créer et lancer le conteneur Keycloak :

```bash
docker run -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:22.0.3 start-dev
```

## Configuration de Keycloak

Une fois le conteneur lancé, accédez à l'interface web de Keycloak à l'adresse : [http://localhost:8080/](http://localhost:8080/)
(https://www.keycloak.org/getting-started/getting-started-docker)
1. Connectez-vous avec les identifiants admin/admin.
2. Créez un nouveau realm.
3. Ajoutez un client pour votre application ASP.NET Core.
4. Cochez OAuth 2.0 Device Authorization Grant
5. Activez Client Authentication
6. Valid redirect URLs https://localhost:7066/*
7. Valid redirect URLs https://localhost:7066/signin-oidc
8. Récupérer le client secret dans credentials
9. Créer un user et dans crédential set password non temporary


## Configuration de ASP.NET Core

Installez le package NuGet pour OpenID Connect :

```bash
dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect
dotnet add package Swashbuckle.AspNetCore.Annotations
```

Dans `Startup.cs`, ajoutez la configuration suivante :

```csharp
// Startup.cs
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

public void ConfigureServices(IServiceCollection services)
{
    
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "cookie";
        options.DefaultSignInScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("cookie")
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "http://localhost:8080/auth/realms/your-realm";
        options.ClientId = "your-client-id";
        options.ClientSecret = "your-client-secret";
        options.ResponseType = "code";

    });
}
```

## Exemple d'Utilisation

Créez controller qui utilisent l'authentification. Par exemple :

```csharp
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



    }
```

## Résumé

Keycloak agit comme un serveur d'authentification et d'autorisation, gérant les utilisateurs et les rôles pour votre application ASP.NET Core.


