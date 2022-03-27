using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebStoreAPI
{
    public class JwtAuthenticationOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public int TokenMinuteLifetime { get; set; }
        public SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));
    }
}