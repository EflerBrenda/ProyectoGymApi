using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class Anuncio
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public int? Activo { get; set; }

    [Display(Name = "Fecha anuncio")]
    public DateTime? Fecha_anuncio { get; set; }

    public int ProfesorId { get; set; }
    [ForeignKey(nameof(ProfesorId))]
    public Usuario? Profesor { get; set; }

}
