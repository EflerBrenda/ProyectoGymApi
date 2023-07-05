using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GymApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MercadoPago.Config;



var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Usuarios/Login";
                    options.LogoutPath = "/Usuarios/Logout";
                    options.AccessDeniedPath = "/Home/Restringido";
                })
                 .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["TokenAuthentication:Issuer"],
                        ValidAudience = configuration["TokenAuthentication:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(
                            configuration["TokenAuthentication:SecretKey"])),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {

                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/API/Usuarios/token"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
                options.AddPolicy("Empleado", policy => policy.RequireRole("Empleado"));
            });

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
    serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps());
});

var serverVersion = new MySqlServerVersion(new Version(8, 0, 29));
builder.Services.AddDbContext<DataContext>(
    options => options.UseMySql(
        configuration["ConnectionStrings:DefaultConnection"],
        ServerVersion.AutoDetect(configuration["ConnectionStrings:DefaultConnection"])

    )
);


builder.Services.AddControllersWithViews();

var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

MercadoPagoConfig.AccessToken = "TEST-8311172502624981-070420-dbe63d907edfe56a90e4f362680d1907-1414468349";

app.Run();
