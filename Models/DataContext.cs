using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GymApi.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Anuncio> Anuncio { get; set; }
        public DbSet<Ejercicio> Ejercicio { get; set; }
        public DbSet<Pago> Pago { get; set; }
        public DbSet<Plan> Plan { get; set; }
        public DbSet<Rutina> Rutina { get; set; }
        public DbSet<Rutina_Usuario> Rutina_Usuario { get; set; }


    }
}