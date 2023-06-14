using System.ComponentModel.DataAnnotations;
namespace GymApi.Models;

public class Pago
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public String Nro_Transaccion { get; set; }
    public DateTime Fecha_Pago { get; set; }

    [Display(Name = "Alumno")]
    public Usuario? Alumno { get; set; }
}
