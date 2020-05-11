using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserCRUD.BusinessLayer;
using UserCRUD.Configurations;
using UserCRUD.Models;
using UserCRUD.ViewModels;
using ObjectResult = UserCRUD.ViewModels.ObjectResult;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace UserCRUD.Controllers
{
    [Authorize(Policy = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccountManagement _accountManagement;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountManagement accountManagement)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountManagement = accountManagement;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ObjectResult> Login([FromBody]LoginViewModel usuario)
        {
            ObjectDataResult<JWTToken> result = new ObjectDataResult<JWTToken>();
            bool credenciaisValidas = false;
            if (usuario != null && !string.IsNullOrWhiteSpace(usuario.UserName))
            {
                // Verifica a existência do usuário nas tabelas do
                // ASP.NET Core Identity
                var userIdentity = await _userManager
                    .FindByNameAsync(usuario.UserName);
                if (userIdentity != null)
                {
                    // Efetua o login com base no Id do usuário e sua senha
                    SignInResult resultadoLogin = _signInManager
                        .CheckPasswordSignInAsync(userIdentity, usuario.Password, false)
                        .Result;
                    credenciaisValidas = resultadoLogin.Succeeded;
                }
            }

            if (credenciaisValidas)
            {
                result.Success = true;
                result.Data = _accountManagement.GenerateToken(usuario.UserName);
            }
            else
            {
                result.Success = false;
                result.Message = "invalid credentials";
            }

            return result;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ObjectResult> CreateUser([FromBody]RegisterUserViewModel user )
        {
            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = true,
            };

            return await _accountManagement.CreateUser(applicationUser, user.Password);
        }
        
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<ObjectResult> UpdateUserPassword([FromBody]UpdatePasswordViewModel userToUpdate)
        {

            return await _accountManagement.UpdatePwd(User.Identity.Name, userToUpdate);
        }

        [AllowAnonymous]
        [HttpGet("GetUser")]
        public async Task<User> GetUserByUserName([FromQuery]string userName)
        {
            return await _accountManagement.GetUserByName(userName);
        }

        [HttpGet("TestToken")]
        public string TestToken()
        {
            return User.Identity.Name;
        }

        [AllowAnonymous]
        [HttpDelete("DeleteUser")]
        public async Task<ObjectResult> DeleteUser([FromQuery]string userName)
        {
            return await _accountManagement.DeleteUser(userName);

        }

      
    }
}