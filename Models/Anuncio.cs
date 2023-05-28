using System.ComponentModel.DataAnnotations;
namespace GymApi.Models;

public class Anuncio
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public int? Activo { get; set; }

    [Display(Name = "Fecha anuncio")]
    public DateTime? Fecha_anuncio { get; set; }

    [Display(Name = "Profesor")]
    public Usuario? Profesor { get; set; }

}
