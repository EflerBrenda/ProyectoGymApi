using System.ComponentModel.DataAnnotations;
namespace GymApi.Models;

public class Pago
{
    public int Id { get; set; }
    public String Nombre { get; set; }
    public String Apellido { get; set; }
    public String Telefono { get; set; }
    public String Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
    //public string Avatar { get; set; }
}
