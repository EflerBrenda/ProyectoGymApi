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
    public class PlanesController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public PlanesController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        [HttpGet("ObtenerPlanes")]
        public async Task<ActionResult<Plan>> ObtenerPlanes()
        {
            try
            {
                return Ok(await contexto.Plan.Where(p => p.Activo == 1).ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("NuevoPlan")]
        public async Task<IActionResult> Post([FromBody] Plan plan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    plan.Activo = 1;
                    contexto.Plan.Add(plan);
                    await contexto.SaveChangesAsync();
                    return Ok(plan);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("BajaPlan/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var plan = contexto.Plan.Single(p => p.Id == id);
                    plan.Activo = 2;
                    contexto.Plan.Update(plan);
                    await contexto.SaveChangesAsync();
                    return Ok(plan);

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("EditarPlan")]
        public async Task<IActionResult> Put([FromBody] Plan planEditado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var planActual = contexto.Plan.Single(u => u.Id == planEditado.Id);
                    planActual.Descripcion = planEditado.Descripcion;
                    planActual.Precio = planEditado.Precio;
                    planActual.Dias_mes = planEditado.Dias_mes;
                    contexto.Plan.Update(planActual);
                    await contexto.SaveChangesAsync();
                    return Ok(planActual);

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