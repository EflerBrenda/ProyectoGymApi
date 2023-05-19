using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApi.Models;

public class CambiarPassword
{

    [Required, DataType(DataType.Password)]
    public string PasswordActual { get; set; }

    [Required, DataType(DataType.Password)]
    public string PasswordNueva { get; set; }

}

