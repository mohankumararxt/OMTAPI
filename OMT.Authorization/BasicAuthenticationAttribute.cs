using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using OMT.DataService;


namespace OMT.Authorization
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IOptions<BasicAuthCredential> _basicAuthCredential;
        private readonly IConfiguration _configuration;
        public BasicAuthenticationHandler(
            IOptions<BasicAuthCredential> basicAuthCredential,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _basicAuthCredential = basicAuthCredential;
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            var authorizationHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Basic "))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            var encodedCredentials = authorizationHeader.Substring("Basic ".Length).Trim();
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var credentials = decodedCredentials.Split(':');

            if (credentials.Length != 2)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Basic Authentication credentials"));
            }

            var username = credentials[0];
            var password = credentials[1];

            // Validate the username and password (this is where you can check in-memory cache or other storage)
            if (ValidateCredentials(username, password))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, "Basic");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Basic");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));
        }

        private bool ValidateCredentials(string username, string password)
        {
            string _username = _basicAuthCredential.Value.username;
            string _password = _basicAuthCredential.Value.password;
            //IConfigurationSection _username = _configuration.GetSection("BasicAuthCredential:username");
            //IConfigurationSection _password = _configuration.GetSection("BasicAuthCredential:password");
            return username.Equals( _username) && password.Equals( _password); // Example
        }
    }


}
