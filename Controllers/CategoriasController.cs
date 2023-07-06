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
    public class CategoriasController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public CategoriasController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        [HttpGet("ObtenerCategorias")]
        public async Task<ActionResult<Categoria>> ObtenerCategorias()
        {
            try
            {
                return Ok(await contexto.Categoria.ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("ObtenerCategoriasDia/{diaaa}")]
        public async Task<ActionResult<Categoria>> ObtenerCategoriasDias(int diaaa)
        {
            try
            {
                var Categorias = await contexto.Categoria.FromSql($"SELECT DISTINCT (c.id),c.descripcion FROM categoria c INNER JOIN ejercicio e ON e.categoriaId= c.id INNER JOIN ejercicio_rutina er ON er.ejercicioid= e.id WHERE er.dia={diaaa}").ToListAsync();
                return Ok(Categorias);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("ObtenerCategoriasDiasRutina/{diaa}/{idd}")]
        public async Task<ActionResult<Categoria>> ObtenerCategoriasDiasRutina(int diaa, int idd)
        {
            try
            {
                var Categorias = await contexto.Categoria.FromSql($"SELECT DISTINCT (c.id),c.descripcion FROM categoria c INNER JOIN ejercicio e ON e.categoriaId= c.id INNER JOIN ejercicio_rutina er ON er.ejercicioid= e.id INNER JOIN rutina r ON er.rutinaid= r.id WHERE er.dia={diaa} && er.rutinaId ={idd} && r.activo=1 && er.activo=1").ToListAsync();
                return Ok(Categorias);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

    }
}