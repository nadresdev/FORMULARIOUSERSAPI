using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using FORMULARIOAPI.Context;
using FORMULARIOAPI.Models;
using FORMULARIOAPI.Helpers;
using FORMULARIOAPI.Models.Dto;

namespace FORMULARIOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        public UserController(AppDbContext context)
        {
            _authContext = context;
        }

        [HttpPost("autenticacion")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Users
                .FirstOrDefaultAsync(x => x.Username == userObj.Username);

            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado!" });

            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new { Message = "Contraseña incorrecta" });
            }

            user.Token = CreateJwt(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("registro")] 
        public async Task<IActionResult> AddUserB([FromBody] LoginDTO userObj)
        {
             DateTime hoy = DateTime.Now;


            Personas persona = new Personas();
            User user = new User();
            persona.Nombres = userObj.Nombre;
            persona.Email = userObj.Email;
            persona.Role = userObj.Role;
            persona.Apellidos = userObj.Apellidos;
            persona.FechaCreacion = hoy;
            user.FechaCreacion = hoy;
            persona.NroIdentificacion = userObj.NroIdentificacion;
            persona.TipoIdentificacion = userObj.TipoIdentificacion;
            persona.NombresApellidos = userObj.Nombre + " " + userObj.Apellidos;
            persona.IdentificacionConcatenada = userObj.TipoIdentificacion + " " + userObj.NroIdentificacion;

            user.Username = userObj.Username;  
            user.Password = userObj.Password;

            
            user.Token = userObj.Token;
            user.RefreshToken = userObj.RefreshToken;
           
            if (userObj == null)
                return BadRequest(new { Message = "No se recibió Usuario" });


            if (await CheckEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "El Email ya existente" });


            if (await CheckUsernameExistAsync(userObj.Username))
                return BadRequest(new { Message = "El Nombre de usuario ya existe" });

            var passMessage = CheckPasswordStrength(userObj.Password);
            if (!string.IsNullOrEmpty(passMessage))
                return BadRequest(new { Message = passMessage.ToString() });

            user.Password = PasswordHasher.HashPassword(user.Password);
            persona.Role = "Admin";
            user.Token = "";
            await _authContext.AddAsync(user);
            await _authContext.SaveChangesAsync();
            await _authContext.AddAsync(persona);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Status = 200,
                Message = "Usuario agregado!"
            });
        }


        private Task<bool> CheckEmailExistAsync(string? email)
            => _authContext.Personas.AnyAsync(x => x.Email == email);

        private Task<bool> CheckUsernameExistAsync(string? username)
            => _authContext.Users.AnyAsync(x => x.Username == username);

        private static string CheckPasswordStrength(string pass)
        {
            StringBuilder sb = new StringBuilder();
            if (pass.Length < 9)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(pass, "[a-z]") && Regex.IsMatch(pass, "[A-Z]") && Regex.IsMatch(pass, "[0-9]")))
                sb.Append("Password should be AlphaNumeric" + Environment.NewLine);
            if (!Regex.IsMatch(pass, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password should contain special charcter" + Environment.NewLine);
            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("muymuymuysecreta....yut");
            var identity = new ClaimsIdentity(new Claim[]
            {
           
                new Claim(ClaimTypes.Name,$"{user.Username}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(10),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Users
                .Any(a => a.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {//
            var key = Encoding.ASCII.GetBytes("muymuymuysecreta....yut");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token invalido");
            return principal;

        }

       
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Personas>> GetAllUsers ()// oBTIENE LISTA PERSONAS
        {
            return Ok(await _authContext.Personas.ToListAsync());
        }

        [HttpPost("refrescar")]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                return BadRequest("Error en la petición desde cliente");
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Petición no valida");
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();
            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            });
        }
    }

}
