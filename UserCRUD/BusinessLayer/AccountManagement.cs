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
    public interface IAccountManagement
    {
        Task<ObjectResult> CreateUser(ApplicationUser user, string password);
        Task<ObjectResult> DeleteUser(string userName);
        JWTToken GenerateToken(string usuario);
        Task<User> GetUserByName(string userName);
        Task<ObjectResult> UpdatePwd(string userName, UpdatePasswordViewModel userToUpdate);
    }

    public class AccountManagement : IAccountManagement
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


        public JWTToken GenerateToken(string usuario)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                    new GenericIdentity(usuario, "Login"),
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
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


        public async Task<ObjectResult> CreateUser(
            ApplicationUser user,
            string password)
        {
            ObjectDataResult<IdentityResult> result = new ObjectDataResult<IdentityResult>();

            try
            {
                if (_userManager.FindByNameAsync(user.UserName).Result == null)
                {
                    var resultado = await _userManager
                        .CreateAsync(user, password);

                    if (resultado.Succeeded)
                    {
                        IdentityResult x = await _userManager.AddToRoleAsync(user, Roles.BASE_ROLE);

                        result.Success = x.Succeeded;
                        if (x.Succeeded)
                        {
                            result.Data = resultado;
                        }
                        else
                        {
                            result.Message = x.Errors.Select(x => x.Description).FirstOrDefault();
                        }
                    }
                    else
                    {
                        result.Success = resultado.Succeeded;
                        result.Message = resultado.Errors.Select(x => x.Description).FirstOrDefault();
                    }
                }

                result.Success = false;
                result.Message = "user already exists";
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Message = e.Message;
            }

            return result;


        }

        public async Task<ObjectResult> UpdatePwd(string userName, UpdatePasswordViewModel userToUpdate)
        {

            ObjectDataResult<IdentityResult> result = new ObjectDataResult<IdentityResult>();
            ApplicationUser user = await _userManager.FindByNameAsync(userName);


            IdentityResult idRes = await _userManager.ChangePasswordAsync(user, userToUpdate.OldPassword, userToUpdate.NewPassword);

            result.Success = idRes.Succeeded;
            result.Message = idRes.Errors.Select(x => x.Description).FirstOrDefault();
            result.Data = idRes.Succeeded ? idRes : null;

            return result;
        }

        public async Task<User> GetUserByName(string userName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
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

        public async Task<ObjectResult> DeleteUser(string userName)
        {
            ObjectDataResult<IdentityResult> result = new ObjectDataResult<IdentityResult>();

            ApplicationUser user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                result.Success = false;
                result.Message = "user not found";

                return result;
            }

            IdentityResult wasDeleted = await _userManager.DeleteAsync(user);

            result.Success = wasDeleted.Succeeded;
            result.Message = wasDeleted.Errors.Select(x => x.Description).FirstOrDefault();
            result.Data = wasDeleted.Succeeded ? wasDeleted : null;

            return result;
        }
    }
}
