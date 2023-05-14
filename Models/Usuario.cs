using System.ComponentModel.DataAnnotations;
namespace GymApi.Models;

public class Usuario
{
    public int Id { get; set; }
    public String Nombre { get; set; }
    public String Apellido { get; set; }
    public String Telefono { get; set; }
    public String Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
    public int PlanId { get; set; }
    public DateTime fecha_plan_inicio { get; set; }
    public DateTime fecha_plan_fin { get; set; }
    public int Activo { get; set; }
    public int RolId { get; set; }
}
