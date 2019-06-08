using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiAngularTest.Models;
using ApiAngularTest.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApiAngularTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }


        #region actions

        [HttpPost, Route("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserViewModel.Create user)
        {
            if (ModelState.IsValid)
            {
                var newUser = new ApplicationUser
                {
                    UserName = user.Email,
                    Email = user.Email,
                    Name = user.Name
                };

                var result = await _userManager.CreateAsync(newUser, user.Password);

                if (result.Succeeded)
                {
                    //generate token =>
                    return await BuildToken(new UserViewModel.Login
                    {
                        Email = user.Email,
                        Password = user.Password
                    });
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] UserViewModel.Login login)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    userName: login.Email,
                    password: login.Password,
                    isPersistent: false,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    //generate token =>
                    return await BuildToken(login);
                }
                else
                {
                    return BadRequest("Invalid login attempt.");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        #endregion


        #region private-methods

        private async Task<IActionResult> BuildToken(UserViewModel.Login login)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, login.Email),
                new Claim("myValue", "my custom value"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_KEY"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(3);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: "localhost",
                audience: "localhost",
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var user = await _userManager.FindByEmailAsync(login.Email);

            if (user != null)
            {

                return Ok(new
                {
                    token = tokenString,
                    tokenExpiration = expiration,
                    user = new
                    {
                        user.Id,
                        user.Email,
                        user.Name,
                        user.UserName
                    }
                });
            }

            return BadRequest(String.Format(
                "User with email ({0}) not found.",
                login.Email
            ));
        }

        #endregion
    }
}