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
    public class AnunciosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public AnunciosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }

        [HttpGet("ObtenerAnuncios")]
        public async Task<ActionResult<Anuncio>> ObtenerAnuncios()
        {
            try
            {
                return Ok(await contexto.Anuncio.Include(u => u.Profesor).Where(a => a.Activo == 1).OrderByDescending(a => a.Fecha_anuncio).ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("NuevoAnuncio")]
        public async Task<IActionResult> Post([FromBody] Anuncio a)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var anuncio = new Anuncio();
                    anuncio.Descripcion = a.Descripcion;
                    anuncio.Profesor = contexto.Usuario.Single(p => p.Id == a.ProfesorId);
                    anuncio.Activo = 1;
                    anuncio.Fecha_anuncio = DateTime.Today;
                    contexto.Anuncio.Add(anuncio);
                    await contexto.SaveChangesAsync();
                    return Ok(anuncio);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("BajaAnuncio/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var anuncio = contexto.Anuncio.Single(u => u.Id == id);
                    anuncio.Activo = 2;
                    contexto.Anuncio.Update(anuncio);
                    await contexto.SaveChangesAsync();
                    return Ok(anuncio);

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("EditarAnuncio")]
        public async Task<IActionResult> Put([FromBody] Anuncio anuncioEditado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var anuncioActual = contexto.Anuncio.Single(u => u.Id == anuncioEditado.Id);
                    anuncioActual.Descripcion = anuncioEditado.Descripcion;
                    //anuncioActual.Profesor = contexto.Usuario.Single(p => p.Id == anuncioEditado.ProfesorId);
                    contexto.Anuncio.Update(anuncioActual);
                    await contexto.SaveChangesAsync();
                    return Ok(anuncioActual);

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