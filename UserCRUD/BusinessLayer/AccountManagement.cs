using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using UserCRUD.Configurations;
using UserCRUD.Models;
using UserCRUD.ViewModels;

namespace UserCRUD.BusinessLayer
{
    public class AccountManagement
    {
        private readonly TokenConfigurations _tokenConfigurations;
        private readonly SigningConfigurations _signingConfigurations;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountManagement(TokenConfigurations tokenConfigurations, SigningConfigurations signingConfigurations, UserManager<ApplicationUser> userManager)
        {
            _tokenConfigurations = tokenConfigurations;
            _signingConfigurations = signingConfigurations;
            _userManager = userManager;
        }


        public JWTToken GenerateToken(User usuario)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                    new GenericIdentity(usuario.UserName, "Login"),
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, usuario.UserName)
                    }
                );

            DateTime dataCriacao = DateTime.Now;
            DateTime dataExpiracao = dataCriacao +
                TimeSpan.FromSeconds(_tokenConfigurations.Seconds);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _tokenConfigurations.Issuer,
                Audience = _tokenConfigurations.Audience,
                SigningCredentials = _signingConfigurations.SigningCredentials,
                Subject = identity,
                NotBefore = dataCriacao,
                Expires = dataExpiracao
            });
            var token = handler.WriteToken(securityToken);

            return new JWTToken
            {
                Authenticated = true,
                Created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                Expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                AccessToken = token,
                Message = "OK"
            };
        }


        public async Task<bool> CreateUser(
            ApplicationUser user,
            string password)
        {
            try
            {
                if (_userManager.FindByNameAsync(user.UserName).Result == null)
                {
                    var resultado = await _userManager
                        .CreateAsync(user, password);

                    if (resultado.Succeeded)
                    {
                        IdentityResult x = await _userManager.AddToRoleAsync(user, Roles.BASE_ROLE);

                        
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public async Task<bool> UpdatePwd(string userName, string oldPassword, string newPassword)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);


            IdentityResult result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<User> GetUserByName(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if(user == null)
            {
                return null;
            }

            User usr = new User
            {
                Email = user.Email,
                UserName = user.UserName
            };

            return usr;
        }

        public async Task<bool> DeleteUser(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);

            if(user == null)
            {
                return false;
            }

            IdentityResult wasDeleted = await _userManager.DeleteAsync(user);

            return wasDeleted.Succeeded;
        }
    }
}
