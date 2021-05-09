﻿using CleanApp.Core.Entities;
using CleanApp.Core.Entities.Auth;
using CleanApp.Core.Enumerations;
using CleanApp.Core.Responses;
using CleanApp.Core.Services.Auth;
using CleanApp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CleanApp.Api.Auth.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPassworService _passwordService;
        public TokenController(IConfiguration configuration, IAuthenticationService authenticationService, IPassworService passworService)
        {
            _configuration = configuration;
            _authenticationService = authenticationService;
            _passwordService = passworService;
        }

        /// <summary>
        /// Obtener un token mediante credenciales
        /// </summary>
        /// <param name="login">Usuario y contraseña</param>
        /// <returns>Token</returns>
        [HttpPost(Name = nameof(GenerateToken))]
        [ProducesResponseType(typeof(ApiResponse<Token>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateToken(UserLogin login)
        {
            var userValid = await ValidateUser(login);

            var token = GenerateToken(userValid);

            var response = new ApiResponse<Token>(token);

            return Ok(response);
        }

        /// <summary>
        /// Validar token para iniciar sesión
        /// </summary>
        /// <param name="token">Token para validar</param>
        /// <returns></returns>
        [HttpPost("ValidateToken", Name = nameof(ValidateToken))]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public IActionResult ValidateToken(string token)
        {
            var response = new ApiResponse<bool>(false);

            var _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));
            var issuer = _configuration["Authentication:Issuer"];
            var audience = _configuration["Authentication:Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = _symmetricSecurityKey
                }, out SecurityToken securityToken);

                response.Data = true;
            }
            catch
            {
                response.Data = false;
            }


            return Ok(response);
        }

        #region Private methods

        private async Task<Authentication> ValidateUser(UserLogin login)
        {
            var user = await _authenticationService.GetLoginByCredentials(login);

            _passwordService.Check(user.UserPassword, login.Password);

            return user;
        }

        private Token GenerateToken(Authentication authentication)
        {
            //Header
            var _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));
            var signingCredentials = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(signingCredentials);

            //Claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, authentication.UserLogin),
                new Claim(ClaimTypes.Name, authentication.UserName),
                new Claim(ClaimTypes.Role, authentication.UserRole.ToString()),
            };

            //Payload
            var payload = new JwtPayload
                (
                    _configuration["Authentication:Issuer"],
                    _configuration["Authentication:Audience"],
                    claims,
                    DateTime.Now,
                    authentication.UserRole == RoleType.Administrator ? DateTime.Now.AddMinutes(_configuration.GetValue<double>("Authentication:AdminTokenLife")) : DateTime.Now.AddMinutes(_configuration.GetValue<double>("Authentication:TokenLife"))
                ); ;

            var jwtToken = new JwtSecurityToken(header, payload);

            Token token = new Token()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                CreatedIn = jwtToken.ValidFrom,
                ExpiresIn = jwtToken.ValidTo
            };

            return token;
        }

        #endregion
    }
}
