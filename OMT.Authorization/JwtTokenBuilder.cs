using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OMT.Authorization
{
    public sealed class JwtTokenBuilder
    {
        private SecurityKey securityKey = null;
        private string issuer = "";
        private string audience = "";
        private Dictionary<string, string> claims = new Dictionary<string, string>();
        private int expiryInMinutes = 5;

        private readonly IOptions<JwtAuthSettings> _authSettings;


        public JwtTokenBuilder(IOptions<JwtAuthSettings> authSettings)
        {
            _authSettings = authSettings;

        }

        public JwtTokenBuilder AddClaims(Dictionary<string, string> claims)
        {
            foreach (var cl in claims)
            {
                this.claims.Add(cl.Key, cl.Value);
            }
            return this;
        }

        public JwtToken Build()
        {
            this.securityKey = JwtSecurityKey.Create(_authSettings.Value.SecretKey);
            this.issuer = _authSettings.Value.Issuer;
            this.audience = _authSettings.Value.Audience;
            this.expiryInMinutes = _authSettings.Value.ExpiryInMinutes;

            EnsureArguments();

            var claims = new List<Claim>
            {
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }
            .Union(this.claims.Select(item => new Claim(item.Key, item.Value)));

            var token = new JwtSecurityToken(
                              issuer: this.issuer,
                              audience: this.audience,
                              claims: claims,
                              expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                              signingCredentials: new SigningCredentials(
                                                        this.securityKey,
                                                        SecurityAlgorithms.HmacSha256));

            return new JwtToken(token);
        }

        public JwtTokenBuilder AddClaims(IEnumerable<Claim> claims)
        {
            this.claims.Clear();
            foreach (var cl in claims)
            {
                this.claims.Add(cl.Type, cl.Value);
            }
            return this;
        }

        #region " private "

        private void EnsureArguments()
        {
            if (this.securityKey == null)
                throw new ArgumentNullException("Security Key");

            if (string.IsNullOrEmpty(this.issuer))
                throw new ArgumentNullException("Issuer");

            if (string.IsNullOrEmpty(this.audience))
                throw new ArgumentNullException("Audience");
        }

        #endregion
    }

}
