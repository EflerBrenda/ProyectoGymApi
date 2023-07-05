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
        [HttpGet("ObtenerCategoriasDia/{dia}")]
        public async Task<ActionResult<Categoria>> ObtenerCategoriasDias(int dia)
        {
            try
            {
                //var ejercicioRutinas = contexto.Ejercicio_Rutina.Include(e => e.Ejercicio.Categoria).Where(er => er.dia == dia);
                var Categorias = contexto.Categoria.FromSql($"SELECT DISTINCT (c.id),c.descripcion FROM categoria c INNER JOIN ejercicio e ON e.categoriaId= c.id INNER JOIN ejercicio_rutina er ON er.ejercicioid= e.id WHERE er.dia={dia}");
                return Ok(Categorias);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

    }
}