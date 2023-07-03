using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class Asistencia
{
    public int Id { get; set; }
    public DateTime? Fecha_asistencia { get; set; }
    public String? Hora_asistencia { get; set; }
    public int UsuarioId { get; set; }
    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }
}
