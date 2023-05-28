using GymApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;

namespace InmobiliariaEfler.Api
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public UsuariosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        // GET: api/<controller>
        [HttpGet("ObtenerPerfil")]
        public async Task<ActionResult<Usuario>> ObtenerPerfil()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(await contexto.Usuario.Where(u => u.Email == usuario).Select(u => new
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Telefono = u.Telefono
                }).SingleOrDefaultAsync()
                );

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("ObtenerProfesores")]
        public async Task<ActionResult<Usuario>> ObtenerProfesores()
        {
            try
            {
                return Ok(await contexto.Usuario.Where(u => u.Activo == 1 && u.RolId == 2).Select(u => new
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Telefono = u.Telefono
                }).ToListAsync()
                );

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("ObtenerAlumnos")]
        public async Task<ActionResult<Usuario>> ObtenerAlumnos()
        {
            try
            {
                return Ok(await contexto.Usuario.Where(u => u.Activo == 1 && u.RolId == 3).Select(u => new
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Telefono = u.Telefono
                }).ToListAsync()
                );

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] UsuarioLogin usuarioLogin)
        {
            try
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: usuarioLogin.Password,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));

                var u = await contexto.Usuario.FirstOrDefaultAsync(x => x.Email == usuarioLogin.Email);
                if (u == null || u.Password != hashed)
                {
                    return BadRequest("Nombre de usuario o clave incorrecta");
                }
                else
                {
                    var key = new SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                    var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, u.Email),
                        new Claim("FullName", u.Nombre + " " + u.Apellido),
                        //new Claim(ClaimTypes.Role, "Profesor"),
                    };

                    var token = new JwtSecurityToken(
                        issuer: config["TokenAuthentication:Issuer"],
                        audience: config["TokenAuthentication:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(60),
                        signingCredentials: credenciales
                    );
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("NuevoUsuario")]
        public async Task<IActionResult> Post([FromForm] Usuario usuario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: usuario.Password,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                    usuario.Password = hashed;
                    contexto.Usuario.Add(usuario);
                    await contexto.SaveChangesAsync();
                    return Ok(usuario);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("BajaUsuario")]
        public async Task<IActionResult> Delete([FromForm] int idUsuario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuario = contexto.Usuario.Single(u => u.Id == idUsuario);
                    usuario.Fecha_plan_fin = DateTime.Today;
                    usuario.Activo = 2;
                    contexto.Usuario.Update(usuario);
                    await contexto.SaveChangesAsync();
                    return Ok(usuario);

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("EditarUsuario")]
        public async Task<IActionResult> Put([FromForm] Usuario usuarioEditado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuarioActual = contexto.Usuario.Single(u => u.Id == usuarioEditado.Id);
                    usuarioActual.Nombre = usuarioEditado.Nombre;
                    usuarioActual.Apellido = usuarioEditado.Apellido;
                    usuarioActual.Telefono = usuarioEditado.Telefono;
                    usuarioActual.Email = usuarioEditado.Email;
                    contexto.Usuario.Update(usuarioActual);
                    await contexto.SaveChangesAsync();
                    return Ok(usuarioActual);

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("cambiarPassword")]
        public async Task<IActionResult> cambiarPassword([FromForm] CambiarPassword usuario)
        {
            try
            {
                string hashedPassVieja = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                   password: usuario.PasswordActual,
                   salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                   prf: KeyDerivationPrf.HMACSHA1,
                   iterationCount: 1000,
                   numBytesRequested: 256 / 8));
                string hashedPassNueva = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                       password: usuario.PasswordNueva,
                       salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                       prf: KeyDerivationPrf.HMACSHA1,
                       iterationCount: 1000,
                       numBytesRequested: 256 / 8));


                var usuarioCambioPassword = await contexto.Usuario.SingleOrDefaultAsync(x => x.Email == User.Identity.Name);
                string PassVieja = usuarioCambioPassword.Password;

                if (PassVieja == hashedPassVieja)
                {

                    usuarioCambioPassword.Password = hashedPassNueva;
                    contexto.Usuario.Update(usuarioCambioPassword);
                    await contexto.SaveChangesAsync();
                    return Ok(usuarioCambioPassword);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}