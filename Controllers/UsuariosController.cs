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
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;


namespace GymApi.Api
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

        [HttpGet("ObtenerPerfil")]
        public async Task<ActionResult<Usuario>> ObtenerPerfil()
        {
            try
            {
                var usuario = User.Identity.Name;
                /* return Ok(await contexto.Usuario.Where(u => u.Email == usuario).Select(u => new
                 {
                     Id = u.Id,
                     Nombre = u.Nombre,
                     Apellido = u.Apellido,
                     Email = u.Email,
                     Telefono = u.Telefono,
                     Plan = u.Plan
                 }).SingleOrDefaultAsync()
                 );*/
                return Ok(contexto.Usuario.Include(p => p.Plan).Single(u => u.Email == usuario));

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
                    Telefono = u.Telefono,
                    Plan = u.Plan
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
                        expires: DateTime.Now.AddMinutes(120),
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
        [HttpPost("NuevoAlumno")]
        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: usuario.Apellido.ToLower() + "123",
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                    usuario.Password = hashed;
                    usuario.Activo = 1;
                    usuario.Fecha_plan_inicio = DateTime.Today;
                    usuario.Fecha_plan_fin = DateTime.Today.AddMonths(1);
                    usuario.Plan = contexto.Plan.Single(p => p.Id == usuario.PlanId);
                    usuario.RolId = 3;
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
        [HttpDelete("BajaAlumno/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuario = contexto.Usuario.Single(u => u.Id == id);
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
        [HttpPut("EditarAlumno")]
        public async Task<IActionResult> Put([FromBody] Usuario usuarioEditado)
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
                    usuarioActual.PlanId = usuarioEditado.PlanId;
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
        [HttpPut("CambiarPlanUsuario")]
        public async Task<IActionResult> Put([FromForm] CambiarPlanUsuario cambioPlan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuarioActual = contexto.Usuario.Single(u => u.Id == cambioPlan.Id);
                    usuarioActual.Plan = contexto.Plan.Single(p => p.Id == cambioPlan.PlanId);
                    usuarioActual.Fecha_plan_inicio = DateTime.Today;
                    usuarioActual.Fecha_plan_fin = DateTime.Today.AddMonths(1);
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
        [HttpPost("HabilitarAlumnoRutina")]
        public async Task<IActionResult> Post([FromForm] Rutina_Usuario habilitarRutina)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    habilitarRutina.Alumno = contexto.Usuario.Single(x => x.Id == habilitarRutina.AlumnoId);
                    habilitarRutina.Rutina = contexto.Rutina.Single(x => x.Id == habilitarRutina.RutinaId);
                    habilitarRutina.Activo = 1;
                    habilitarRutina.Fecha_inicio_rutina = DateTime.Today;
                    contexto.Rutina_Usuario.Add(habilitarRutina);
                    await contexto.SaveChangesAsync();
                    return Ok(habilitarRutina);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("DeshabilitarAlumnoRutina")]
        public async Task<IActionResult> Put([FromForm] int IdRutina)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var deshabilitarRutina = contexto.Rutina_Usuario.Single(x => x.Id == IdRutina);
                    deshabilitarRutina.Activo = 2;
                    contexto.Rutina_Usuario.Update(deshabilitarRutina);
                    await contexto.SaveChangesAsync();
                    return Ok(deshabilitarRutina);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("ObtenerRutinaAlumno")]
        public async Task<ActionResult<Rutina>> ObtenerRutina()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(await contexto.Rutina_Usuario.Include(u => u.Alumno).Where(u => u.Alumno.Email == usuario && u.Activo == 1).ToListAsync()
                );

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("token")]
        public async Task<ActionResult> token()
        {
            try
            {

                var perfil = new
                {
                    Email = User.Identity.Name,
                    Nombre = User.Claims.First(x => x.Type == "FullName").Value,
                    Rol = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value
                };

                Random rand = new Random(Environment.TickCount);
                string randomChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
                string nuevaClave = "";
                for (int i = 0; i < 8; i++)
                {
                    nuevaClave += randomChars[rand.Next(0, randomChars.Length)];
                }

                String nuevaClaveSin = nuevaClave;

                nuevaClave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: nuevaClave,
                            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                            prf: KeyDerivationPrf.HMACSHA1,
                            iterationCount: 1000,
                            numBytesRequested: 256 / 8));


                Usuario original = await contexto.Usuario.AsNoTracking().FirstOrDefaultAsync(x => x.Email == perfil.Email);
                original.Password = nuevaClave;
                contexto.Usuario.Update(original);
                await contexto.SaveChangesAsync();

                var message = new MimeKit.MimeMessage();
                message.To.Add(new MailboxAddress(perfil.Nombre, "eflerbrenda@gmail.com"));
                message.From.Add(new MailboxAddress("Gym App", "eflerbrenda@gmail.com"));
                message.Subject = "Gym App";
                message.Body = new TextPart("html")
                {
                    Text = @$"<h1>Hola {perfil.Nombre}!</h1>
					<p> Su nueva contraseña es: <b>{nuevaClaveSin}</b></p><br>
                    <p> Adios!</p>",
                };

                message.Headers.Add("Encabezado", "Valor");
                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.ServerCertificateValidationCallback = (object sender,
                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
                { return true; };
                client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
                client.Authenticate(config["SMTPUser"], config["SMTPPass"]);
                //client.Authenticate("ulp.api.net@gmail.com", "ktitieuikmuzcuup");

                await client.SendAsync(message);

                return Ok("Listo! ya se envio la nueva contraseña a su e-mail.");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("PedidoEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByEmail([FromForm] string email)
        {
            try
            {
                var feature = HttpContext.Features.Get<IHttpConnectionFeature>();
                var LocalPort = feature?.LocalPort.ToString();
                var ipv4 = HttpContext.Connection.LocalIpAddress.MapToIPv4().ToString();
                var ipConexion = "http://" + ipv4 + ":" + LocalPort + "/";

                var entidad = await contexto.Usuario.FirstOrDefaultAsync(x => x.Email == email);
                var key = new SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, entidad.Email),
                        new Claim("FullName", entidad.Nombre + " " + entidad.Apellido),
                        new Claim("id", entidad.Id + " " ),
                        new Claim(ClaimTypes.Role, "Usuario"),

                    };

                var token = new JwtSecurityToken(
                    issuer: config["TokenAuthentication:Issuer"],
                    audience: config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(600),
                    signingCredentials: credenciales
                );
                var to = new JwtSecurityTokenHandler().WriteToken(token);

                var direccion = ipConexion + "API/Usuarios/token?access_token=" + to;
                try
                {


                    var message = new MimeKit.MimeMessage();
                    message.To.Add(new MailboxAddress(entidad.Nombre, "eflerbrenda@gmail.com"));
                    message.From.Add(new MailboxAddress("Gym App", "eflerbrenda@gmail.com"));
                    message.Subject = "Gym App";
                    message.Body = new TextPart("html")


                    {
                        Text = @$"<h1>Hola {entidad.Nombre}!</h1>
					<p>Si usted solicito el cambio de contraseña,<a href={direccion} >presione aquí para reestablecerla.</a> </p><br><p> Si no lo hizo, desestime este e-mail.</p>",
                    };

                    message.Headers.Add("Encabezado", "Valor");
                    MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                    client.ServerCertificateValidationCallback = (object sender,
                    System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                    System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
                    { return true; };
                    client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
                    client.Authenticate(config["SMTPUser"], config["SMTPPass"]);

                    await client.SendAsync(message);


                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                return entidad != null ? Ok(entidad) : NotFound();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}