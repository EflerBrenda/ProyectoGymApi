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
    public class EjerciciosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public EjerciciosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        [HttpGet("ObtenerEjercicios")]
        public async Task<ActionResult<Ejercicio>> ObtenerEjercicios()
        {
            try
            {
                return Ok(await contexto.Ejercicio.Where(a => a.Activo == 1).ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("EjerciciosPorCategorias/{id}")]
        public async Task<ActionResult<Ejercicio>> EjerciciosPorCategorias(int id)
        {
            try
            {
                return Ok(await contexto.Ejercicio.Include(c => c.Categoria).Where(e => e.CategoriaId == id && e.Activo == 1).ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("NuevoEjercicio")]
        public async Task<IActionResult> Post([FromBody] Ejercicio ejercicio)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ejercicio.Activo = 1;
                    contexto.Ejercicio.Add(ejercicio);
                    await contexto.SaveChangesAsync();
                    return Ok(ejercicio);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("BajaEjercicio/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ejercicio = contexto.Ejercicio.Single(e => e.Id == id);
                    ejercicio.Activo = 2;
                    contexto.Ejercicio.Update(ejercicio);
                    await contexto.SaveChangesAsync();
                    return Ok(ejercicio);

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("EditarEjercicio")]
        public async Task<IActionResult> Put([FromBody] Ejercicio ejercicioEditado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ejercicioActual = contexto.Ejercicio.Single(e => e.Id == ejercicioEditado.Id);
                    ejercicioActual.Descripcion = ejercicioEditado.Descripcion;
                    ejercicioActual.Explicacion = ejercicioEditado.Explicacion;
                    ejercicioActual.CategoriaId = ejercicioEditado.CategoriaId;
                    contexto.Ejercicio.Update(ejercicioActual);
                    await contexto.SaveChangesAsync();
                    return Ok(ejercicioActual);

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