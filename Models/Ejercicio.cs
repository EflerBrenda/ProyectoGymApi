using System.ComponentModel.DataAnnotations;
namespace GymApi.Models;

public class Ejercicio
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public String Explicacion { get; set; }
    public int? Activo { get; set; }

}
