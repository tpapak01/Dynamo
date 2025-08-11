using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamo.Data;


public class EnergyMeasurements
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    [Required]
    public DateTime measurementDatetime {  get; set; }

    
    public float? consumption { get; set; }

    public float? production { get; set; }

    [Required]
    [ForeignKey("Houses")]
    public int houseId { get; set; }
    public virtual Houses Houses { get; set; }


}
