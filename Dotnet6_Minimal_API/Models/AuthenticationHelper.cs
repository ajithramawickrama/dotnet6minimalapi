using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
public class AuthenticationHelper
{

    Configuration _configuration;
    public AuthenticationHelper(Configuration configuration)
    {
        _configuration = configuration;
    }

    public string Login(LoginVM loginVM)
    {
        if (loginVM.Username == "admin" && loginVM.Password == "password")
        {
            return GenerateJWT();
        }
        else
            return null;
    }

    private string GenerateJWT()
    {
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));
        var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:ValidIssuer"],
        audience: _configuration["Jwt:ValidAudiance"],
        claims: null,
        expires: System.DateTime.Now.AddMinutes(120),
        signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}