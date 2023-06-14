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
    public class RutinasController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public RutinasController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        [HttpGet("ObtenerRutinas")]
        public async Task<ActionResult<Rutina>> ObtenerRutinas()
        {
            try
            {
                return Ok(await contexto.Rutina.Where(a => a.Activo == 1).ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("NuevaRutina")]
        public async Task<IActionResult> Post([FromForm] Rutina nuevaRutina)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var rutina = nuevaRutina;
                    rutina.Activo = 1;
                    contexto.Rutina.Add(rutina);
                    await contexto.SaveChangesAsync();
                    return Ok(rutina);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("BajaRutina")]
        public async Task<IActionResult> Delete([FromForm] int idRutina)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var rutina = contexto.Rutina.Single(r => r.Id == idRutina);
                    rutina.Activo = 2;
                    contexto.Rutina.Update(rutina);
                    await contexto.SaveChangesAsync();
                    return Ok(rutina);

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("EditarRutina")]
        public async Task<IActionResult> Put([FromForm] Rutina rutinaEditado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var rutinaActual = contexto.Rutina.Single(r => r.Id == rutinaEditado.Id);
                    rutinaActual.Descripcion = rutinaEditado.Descripcion;
                    contexto.Rutina.Update(rutinaActual);
                    await contexto.SaveChangesAsync();
                    return Ok(rutinaActual);

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