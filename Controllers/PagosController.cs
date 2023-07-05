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
        public async Task<ActionResult<String>> Get()
        {
            try
            {
                var usuario = User.Identity.Name;
                IDictionary<string, object> metadata = new Dictionary<string, object>();
                metadata.Add("userid", usuario);

                var request = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>{
                        new PreferenceItemRequest{
                            Title = "cuota mes julio", //Producto a cobrar
                            Quantity = 1,
                            CurrencyId = "ARS",
                            UnitPrice = 100,
                        }
                    },
                    Metadata = metadata,
                    // NotificationUrl = "https://793d-170-150-8-5.ngrok-free.app/api/pagos/NotificacionMP"
                };
                var client = new PreferenceClient();
                Preference preference = await client.CreateAsync(request);
                return Ok(preference.InitPoint);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("NotificacionMP")]
        [AllowAnonymous]
        //public async Task<IActionResult> NotificacionMP([FromQuery] String type, [FromQuery] String id)
        public async Task<IActionResult> NotificacionMP([FromBody] PagoMP body)//[FromBody] Object body
        {
            try
            {
                /* String type = HttpContext.Request.Query["type"];
                 String id = HttpContext.Request.Query["id"];
                 String topic = HttpContext.Request.Query["topic"];
                Console.WriteLine("body:" + body.Data.Id);
                Console.WriteLine("body:" + body.Type);
                   foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(pagoMercado))
                    {
                        string name = descriptor.Name;
                        object value = descriptor.GetValue(pagoMercado);
                        Console.WriteLine("{0}={1}", name, value);
                    }
                */

                if (body.Action.Equals("payment.created") && body.Type.Equals("payment"))
                {
                    MercadoPago.Client.Payment.PaymentClient pago = new MercadoPago.Client.Payment.PaymentClient();
                    MercadoPago.Resource.Payment.Payment pagoMercado = pago.Get(body.Data.Id);

                    if (pagoMercado.Status.Equals("approved") && pagoMercado.StatusDetail.Equals("accredited"))
                    {
                        //
                        //Guardar en la base de datos
                        Console.WriteLine(body.Data.Id);
                        Console.WriteLine(pagoMercado.Metadata["userid"].ToString());
                        Console.WriteLine(body.Type);
                    }
                }

                //Console.WriteLine(id);
                /*if (!String.IsNullOrEmpty(id))
                {
                    //Payment pago= new PreferencePaymentTypeRequest();
                    MercadoPago.Client.Payment.PaymentClient pago = new MercadoPago.Client.Payment.PaymentClient();
                    MercadoPago.Resource.Payment.Payment pagoMercado = pago.Get(1313716952);
                    Console.WriteLine(pagoMercado.Status);
                }
                if (type == "payment" || topic == "payment")
                {

                    //Console.WriteLine(pagoMercado.Status);
                    Console.WriteLine("--------------------------------------");
                    Console.WriteLine("--------------------------------------");
                    Console.WriteLine(type);
                    Console.WriteLine("--------------------------------------");
                    Console.WriteLine(topic);
                    //Payment pago= new PreferencePaymentTypeRequest();
                    // MercadoPago.Client.Payment.PaymentClient pago = new MercadoPago.Client.Payment.PaymentClient();
                    // MercadoPago.Resource.Payment.Payment pagoMercado = pago.Get(long.Parse(id));
                    // Console.WriteLine(pagoMercado);
                }*/

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok();
            }
        }
    }
}