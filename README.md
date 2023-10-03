
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

1. Connectez-vous avec les identifiants admin/admin.
2. Créez un nouveau realm.
3. Ajoutez un client pour votre application ASP.NET Core.
4. Cochez OAuth 2.0 Device Authorization Grant
5. Valid redirect URLs https://localhost:7066/*
5. Configurez les rôles et les utilisateurs comme nécessaire.
(https://www.keycloak.org/getting-started/getting-started-docker)

## Configuration de ASP.NET Core

Installez le package NuGet pour OpenID Connect :

```bash
dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect
dotnet add package Swashbuckle.AspNetCore.Annotations
```

Dans `Startup.cs`, ajoutez la configuration suivante :

```csharp
// Startup.cs
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
{
    // Utilisation des valeurs par défaut pour les schémas
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme) // Schéma pour les cookies
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => // Schéma pour OpenID Connect
{
    options.Authority = "http://localhost:8080";
    options.ClientId = "your-client";
    options.ClientSecret = "your-client-secret";
    options.MetadataAddress = "http://localhost:8080/realms/myrealm/.well-known/openid-configuration";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.RequireHttpsMetadata = false;
});
var app = builder.Build();
```

## Exemple d'Utilisation

Créez des contrôleurs ou des actions qui utilisent l'authentification. Par exemple :

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


