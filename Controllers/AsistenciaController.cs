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

namespace GymApi.Api
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class AsistenciaController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public AsistenciaController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }

        [HttpGet("ObtenerAsistenciasAlumnos")]
        public async Task<ActionResult<Asistencia>> ObtenerAsistencia()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(await contexto.Asistencia.Include(u => u.Usuario).Where(u => u.Usuario.Email == usuario).OrderByDescending(a => a.Fecha_asistencia).ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("NuevaAsistencia")]
        public async Task<ActionResult<Asistencia>> Post()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var email = User.Identity.Name;
                    var usuario = contexto.Usuario.Include(p => p.Plan).Single(u => u.Email == email);

                    List<Asistencia> asistencias = contexto.Asistencia.Include(u => u.Usuario).Where(u => u.Usuario.Id == usuario.Id).ToList();

                    foreach (var a in asistencias)
                    {
                        if (a.Fecha_asistencia == DateTime.Today)
                        {
                            return BadRequest("Ya pusiste presente.");
                        }
                    }

                    //int count= contexto.Asistencia.Include(u => u.Usuario).Where(u => u.Usuario.Email == email).Count();

                    /* if(count >= usuario.Plan.Dias_mes)
                     {
                         return BadRequest("Ya pusiste presente.");
                     }*/
                    var asistencia = new Asistencia();
                    asistencia.Fecha_asistencia = DateTime.Today;
                    asistencia.Hora_asistencia = DateTime.Now.ToShortTimeString();
                    asistencia.UsuarioId = usuario.Id;
                    contexto.Asistencia.Add(asistencia);
                    await contexto.SaveChangesAsync();
                    return Ok(asistencia);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /* [HttpGet("ObtenerCantidadAsistencia")]
         public async Task<ActionResult<int>> ObtenerCantidadAsistencia()
         {
             try
             {
                 if (ModelState.IsValid)
                 {
                     var email = User.Identity.Name;

                     var usuario= contexto.Usuario.Include(p => p.Plan).Single(u => u.Email == email);
                     int count= contexto.Asistencia.Include(u => u.Usuario).Where(u => u.Usuario.Email == email).Count();

                     if(count <= usuario.Plan.Dias_mes)
                     {

                     }

                     return Ok();
                 }
                 return BadRequest();
             }
             catch (Exception ex)
             {
                 return BadRequest(ex.Message);
             }
         }*/

    }
}