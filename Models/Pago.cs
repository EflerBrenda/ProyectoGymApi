using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class Pago
{
    public int Id { get; set; }
    public String Descripcion { get; set; }
    public String Nro_Transaccion { get; set; }
    public DateTime Fecha_Pago { get; set; }
    public int UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }
}
