using System.ComponentModel.DataAnnotations;
namespace GymApi.Models;

public class Plan
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public Decimal Precio { get; set; }
    public int? Activo { get; set; }
}
