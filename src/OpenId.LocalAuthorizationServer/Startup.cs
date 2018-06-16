namespace OpenId.LocalAuthorizationServer
{
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using AspNet.Security.OpenIdConnect.Primitives;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Owin;
    using Owin.Security.OpenIdConnect.Server;

    public sealed class Startup
    {
        public const string ConfigurationEndpoint = "/.well-known/openid-configuration";
        public const string AuthorizationEndpoint = "/connect/authorize";
        public const string CustomEndpoint = "/connect/custom";
        public const string IntrospectionEndpoint = "/connect/introspect";
        public const string LogoutEndpoint = "/connect/logout";
        public const string RevocationEndpoint = "/connect/revoke";
        public const string TokenEndpoint = "/connect/token";
        public const string UserinfoEndpoint = "/connect/userinfo";


        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                LoginPath = new PathString("/login"),
                LogoutPath = new PathString("/logout")
            });

            appBuilder.UseOpenIdConnectServer(options =>
            {
                options.AllowInsecureHttp = true;

                // Enable the tested endpoints.
                options.ConfigurationEndpointPath = new PathString(ConfigurationEndpoint);
                options.AuthorizationEndpointPath = new PathString(AuthorizationEndpoint);
                options.IntrospectionEndpointPath = new PathString(IntrospectionEndpoint);
                options.LogoutEndpointPath = new PathString(LogoutEndpoint);
                options.RevocationEndpointPath = new PathString(RevocationEndpoint);
                options.TokenEndpointPath = new PathString(TokenEndpoint);
                options.UserinfoEndpointPath = new PathString(UserinfoEndpoint);

                options.SigningCredentials.AddCertificate(
                    assembly: typeof(Startup).GetTypeInfo().Assembly,
                    resource: "OpenId.LocalAuthorizationServer.localhost.pfx",
                    password: "localhost");

                // Note: overriding the default data protection provider is not necessary for the tests to pass,
                // but is useful to ensure unnecessary keys are not persisted in testing environments, which also
                // helps make the unit tests run faster, as no registry or disk access is required in this case.
                options.DataProtectionProvider = new EphemeralDataProtectionProvider(new LoggerFactory());
            });

            appBuilder.Use((context, next) =>
            {
                if (context.Request.Path == new PathString("/invalid-signin"))
                {
                    var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationType);
                    identity.AddClaim(new Claim(OpenIdConnectConstants.Claims.Subject, "Bob le Bricoleur"));

                    context.Authentication.SignIn(identity);

                    return Task.CompletedTask;
                }

                else if (context.Request.Path == new PathString("/invalid-signout"))
                {
                    context.Authentication.SignOut(OpenIdConnectServerDefaults.AuthenticationType);

                    return Task.CompletedTask;
                }

                else if (context.Request.Path == new PathString("/invalid-challenge"))
                {
                    context.Authentication.Challenge(OpenIdConnectServerDefaults.AuthenticationType);

                    return Task.CompletedTask;
                }

                else if (context.Request.Path == new PathString("/invalid-authenticate"))
                {
                    return context.Authentication.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationType);
                }

                return next();
            });
        }
    }
}
