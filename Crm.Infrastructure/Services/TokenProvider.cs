using System.Security.Claims;
using System.Text;
using System.Text.Unicode;
using Crm.Domain.Entities;
using Crm.Infrastucture.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Crm.Infrastucture.Services;

public class TokenProvider(IOptions<JwtOptions> options)
{
    private readonly IOptions<JwtOptions> _jwtOptions = options;

    public string Create(User user)
    {
    var symetricSecuriteKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Key));
    var signingCredentials = new SigningCredentials(symetricSecuriteKey, SecurityAlgorithms.HmacSha256);
    var tokenDescriptor = new SecurityTokenDescriptor                                                                                                                          
    {                                                                                                                                                                          
        Subject = new ClaimsIdentity(                                                                                                                                          
        [                                                                                                                                                                      
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),                                                                                                        
            new Claim(JwtRegisteredClaimNames.Email, user.Email),                                                                                                              
        ]),                                                                                                                                                                    
        Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.ExpirationInMinutes),                                                                                                                                                         
        Issuer = _jwtOptions.Value.Issuer,                                                                                                                                                          
        Audience = _jwtOptions.Value.Audience,                                                                                                                                                        
        SigningCredentials = signingCredentials                                                                                                                                               
    };     
    var handler = new JsonWebTokenHandler();                                                                                                                                   
    return handler.CreateToken(tokenDescriptor);                                                                                                                               

    }
}