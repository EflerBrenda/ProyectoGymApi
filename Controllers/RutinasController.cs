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
        [HttpGet("ObtenerMiRutina")]
        public async Task<ActionResult<Rutina_Usuario>> ObtenerMiRutina(int id)
        {
            try
            {
                return Ok(contexto.Rutina_Usuario.Include(ur => ur.Rutina).Single(a => a.Activo == 1 && a.Alumno.Email == User.Identity.Name));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
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
        [HttpGet("ObtenerEjerciciosRutinas/{id}")]
        public async Task<ActionResult<Ejercicio_Rutina>> ObtenerEjerciciosRutinas(int id)
        {
            try
            {
                return Ok(await contexto.Ejercicio_Rutina.Include(r => r.Rutina).Include(e => e.Ejercicio).Where(a => a.Activo == 1 && a.RutinaId == id).ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("ObtenerRutinaEjerciciosUsuario")]
        public async Task<ActionResult<List<Ejercicio_Rutina>>> ObtenerRutinaEjerciciosUsuario()
        {
            try
            {
                Usuario usuario = await contexto.Usuario.SingleOrDefaultAsync(u => u.Email == User.Identity.Name);
                Rutina_Usuario rutina = await contexto.Rutina_Usuario.Include(r => r.Rutina).Include(a => a.Alumno).SingleOrDefaultAsync(u => u.AlumnoId == usuario.Id);
                if (rutina != null)
                {
                    List<Ejercicio_Rutina> erList = await contexto.Ejercicio_Rutina.Include(r => r.Rutina).Include(e => e.Ejercicio).Where(a => a.Activo == 1 && a.Rutina.Id == rutina.RutinaId).ToListAsync();
                    return Ok(erList);
                }
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("ObtenerCantDiasRutina")]
        public async Task<ActionResult<List<int>>> ObtenerCantDiasRutina()
        {
            try
            {
                var usuario = contexto.Usuario.Include(p => p.Plan).Single(u => u.Email == User.Identity.Name);
                var cdplan = usuario.Plan.Dias_mes / 4;
                List<int> lista = new List<int>();

                if (cdplan == 2)
                {
                    lista = obtenerLista(cdplan);
                }
                else if (cdplan == 3)
                {
                    lista = obtenerLista(cdplan);
                }
                else if (cdplan == 5)
                {
                    lista = obtenerLista(cdplan);
                }
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("NuevaRutina")]
        public async Task<IActionResult> Post([FromBody] Rutina nuevaRutina)
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
        [HttpPost("AsignarRutina")]
        public async Task<IActionResult> AsignarRutina([FromBody] Rutina_Usuario rutina)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Rutina_Usuario ru = null;
                    List<Rutina_Usuario> rutinas = await contexto.Rutina_Usuario.Where(r => r.AlumnoId == rutina.AlumnoId).ToListAsync();
                    if (rutinas.Count() > 0)
                    {
                        foreach (Rutina_Usuario r in rutinas)
                        {
                            if (r.Activo == 1)
                            {
                                ru = r;
                                break;
                            }
                        }
                        if (ru != null)
                        {
                            ru.RutinaId = rutina.RutinaId;
                            ru.Fecha_inicio_rutina = DateTime.Today;
                            ru.Fecha_fin_rutina = DateTime.Today.AddMonths(1);
                            ru.Activo = 1;
                            contexto.Rutina_Usuario.Update(ru);
                            await contexto.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        rutina.Fecha_inicio_rutina = DateTime.Today;
                        rutina.Fecha_fin_rutina = DateTime.Today.AddMonths(1);
                        rutina.Activo = 1;
                        contexto.Rutina_Usuario.Add(rutina);
                        await contexto.SaveChangesAsync();
                    }


                    return Ok(rutina);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("BajaRutina/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<Ejercicio_Rutina> ejercicioRutina = contexto.Ejercicio_Rutina.Where(er => er.RutinaId == id).ToList();
                    foreach (Ejercicio_Rutina er in ejercicioRutina)
                    {
                        er.Activo = 2;
                        contexto.Ejercicio_Rutina.Update(er);
                    }
                    var rutina = contexto.Rutina.Single(r => r.Id == id);
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
        public async Task<IActionResult> Put([FromBody] Rutina rutinaEditado)
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

        public List<int> obtenerLista(int dias)
        {
            int iteracion = 1;
            List<int> resultado = new List<int>();
            for (int i = 0; i < dias; i++)
            {
                resultado.Add(iteracion);
                iteracion++;
            }
            return (resultado);
        }
    }
}