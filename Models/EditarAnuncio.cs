using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class EditarAnuncio
{
    public int? Id { get; set; }
    public String Descripcion { get; set; }
    public int ProfesorId { get; set; }

}

