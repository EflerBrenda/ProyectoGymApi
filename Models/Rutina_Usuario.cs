using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class Rutina_Usuario
{
    public int Id { get; set; }
    public int RutinaId { get; set; }
    [ForeignKey(nameof(RutinaId))]
    public Rutina? Rutina { get; set; }
    public int ProfesorId { get; set; }
    [ForeignKey(nameof(ProfesorId))]
    public Usuario? Profesor { get; set; }

    public int AlumnoId { get; set; }
    [ForeignKey(nameof(AlumnoId))]
    public Usuario? Alumno { get; set; }

    public DateTime? Fecha_inicio_rutina { get; set; }

    public DateTime Fecha_fin_rutina { get; set; }

    public int? Activo { get; set; }
}
