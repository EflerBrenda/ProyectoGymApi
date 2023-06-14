using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class CambiarPlanUsuario
{
    public int? Id { get; set; }
    public int PlanId { get; set; }

}

