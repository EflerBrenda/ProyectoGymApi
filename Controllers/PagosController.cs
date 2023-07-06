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
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using MercadoPago.Client.PaymentMethod;
using MercadoPago.Http;
using MercadoPago;
using System.ComponentModel;

namespace GymApi.Api
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public PagosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        // GET: api/<controller>
        [HttpGet("GenerarLinkPago")]
        public async Task<ActionResult<PagoUrl>> Get()
        {
            try
            {
                var email = User.Identity.Name;
                var usuario = contexto.Usuario.Include(p => p.Plan).Single(u => u.Email == email);
                IDictionary<string, object> metadata = new Dictionary<string, object>();
                metadata.Add("userid", usuario.Id);

                var request = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>{
                        new PreferenceItemRequest{
                            Title = usuario.Plan.Descripcion, //Producto a cobrar
                            Quantity = 1,
                            CurrencyId = "ARS",
                            UnitPrice = usuario.Plan.Precio,
                        }
                    },
                    Metadata = metadata,
                    // NotificationUrl = "https://793d-170-150-8-5.ngrok-free.app/api/pagos/NotificacionMP"
                };
                var client = new PreferenceClient();
                Preference preference = await client.CreateAsync(request);
                PagoUrl url = new PagoUrl();
                url.URL = preference.InitPoint;
                return Ok(url);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("NotificacionMP")]
        [AllowAnonymous]

        public async Task<IActionResult> NotificacionMP([FromBody] PagoMP body)
        {
            try
            {


                if (body.Action.Equals("payment.created") && body.Type.Equals("payment"))
                {
                    MercadoPago.Client.Payment.PaymentClient pago = new MercadoPago.Client.Payment.PaymentClient();
                    MercadoPago.Resource.Payment.Payment pagoMercado = pago.Get(body.Data.Id);

                    if (pagoMercado.Status.Equals("approved") && pagoMercado.StatusDetail.Equals("accredited"))
                    {

                        Pago p = new Pago();
                        p.UsuarioId = int.Parse(pagoMercado.Metadata["userid"].ToString());
                        p.Descripcion = "Cuota del mes";
                        p.Fecha_Pago = DateTime.Today;
                        p.Nro_Transaccion = body.Data.Id.ToString();
                        contexto.Pago.Add(p);
                        await contexto.SaveChangesAsync();
                        Console.WriteLine("Pago OK");

                    }
                }



                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok();
            }
        }
        [HttpGet("ObtenerPagos")]
        public async Task<ActionResult<List<Pago>>> ObtenerPagos()
        {
            var usuario = User.Identity.Name;
            return Ok(await contexto.Pago.Where(u => u.Usuario.Email == usuario).ToListAsync());
        }

    }
}