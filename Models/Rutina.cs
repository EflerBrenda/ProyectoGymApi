using System.ComponentModel.DataAnnotations;
namespace GymApi.Models;

public class Rutina
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public int? Activo { get; set; }
}
