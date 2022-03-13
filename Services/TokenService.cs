using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EstudosAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace EstudosAPI.Services
{
    public static class TokenService //clase static => nao precisa dar um new object{}
    {
        public static string GenerateToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler(); //tokenHandler => gera o token
            var key = Encoding.ASCII.GetBytes(Settings.Key); //pega os bytes da chave
            var tokenDescriptor = new SecurityTokenDescriptor //descrição do que tem dentro do token
            {
                Subject = new ClaimsIdentity(new Claim[]       //ClaimsIdentity => trabalhar com identidade
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()) 
                }),
                Expires = DateTime.UtcNow.AddHours(1), //tempo de experição do token
                //gerando as credentials usando os bytes da chave
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor); //crio o token com as infos do tokenDescriptor
            return tokenHandler.WriteToken(token); //gera a string do token
        }
    }
}