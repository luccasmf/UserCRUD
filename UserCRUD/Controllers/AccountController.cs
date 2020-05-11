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
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace UserCRUD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AccountManagement _accountManagement;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AccountManagement accountManagement)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountManagement = accountManagement;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public object Login([FromBody]User usuario)
        {
            bool credenciaisValidas = false;
            if (usuario != null && !string.IsNullOrWhiteSpace(usuario.UserName))
            {
                // Verifica a existência do usuário nas tabelas do
                // ASP.NET Core Identity
                var userIdentity = _userManager
                    .FindByNameAsync(usuario.UserName).Result;
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
                return _accountManagement.GenerateToken(usuario);
            }

            return null;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<bool> CreateUser([FromBody]RegisterUser user )
        {
            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = true,
            };

            return await _accountManagement.CreateUser(applicationUser, user.Password);
        }
    }
}