using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamo.Business.Models;


public class EnergyDataBody
{
    [Required]
    public DateTime measurementDatetime { get; set; }


    public float? consumption { get; set; }

    public float? production { get; set; }

    [Required]
    public string houseIdentifier { get; set; }


}
