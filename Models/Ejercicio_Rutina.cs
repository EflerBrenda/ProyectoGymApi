using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class Ejercicio_Rutina
{
    public int Id { get; set; }
    public int RutinaId { get; set; }
    [ForeignKey(nameof(RutinaId))]
    public Rutina? Rutina { get; set; }
    public int EjercicioId { get; set; }
    [ForeignKey(nameof(EjercicioId))]
    public Ejercicio? Ejercicio { get; set; }

    public int cantidad { get; set; }

    public int repeticiones { get; set; }

    public int? Activo { get; set; }
}
