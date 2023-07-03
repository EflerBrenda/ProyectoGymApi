using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class Ejercicio
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public String Explicacion { get; set; }
    public int CategoriaId { get; set; }
    [ForeignKey(nameof(CategoriaId))]
    public Categoria? Categoria { get; set; }
    public int? Activo { get; set; }

}
