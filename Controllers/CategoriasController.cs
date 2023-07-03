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
        /*[HttpGet("CantidadCategorias")]
        public async Task<ActionResult> CantidadCategorias()
        {
            try
            {
                return Ok(contexto.Categoria.Count());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }*/

    }
}