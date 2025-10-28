using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

class Program
{
    static void Main()
    {
        var key = "qV9vTg1!P0r$7Y@3mK8#Z2^xL6&cN4*eH1)uB5+fS0=wR8";
        var issuer = "HairSalonAuth";
        var audience = "HairSalonUsers";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "tai"),
            new Claim(ClaimTypes.Role, "Customer")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            )
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        Console.WriteLine("JWT token:", jwt);
    }
}
