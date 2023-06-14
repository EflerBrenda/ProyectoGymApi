using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class Usuario
{
    public int Id { get; set; }
    public String Nombre { get; set; }
    public String Apellido { get; set; }
    public String Telefono { get; set; }
    public String Email { get; set; }

    //[Required, DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "Plan contratado")]
    public int PlanId { get; set; }

    [ForeignKey(nameof(PlanId))]
    public Plan? Plan { get; set; }

    [Display(Name = "Fecha de inicio")]
    public DateTime? Fecha_plan_inicio { get; set; }

    [Display(Name = "Fecha de fin")]
    public DateTime? Fecha_plan_fin { get; set; }
    public int? Activo { get; set; }

    [Display(Name = "Rol")]
    public int? RolId { get; set; }
}
