using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamo.Business.Data;


public class HouseAliases
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public string? ElectiAlias { get; set; }

    public string? MeasurementsAlias { get; set; }

    public string? PredictionsAlias { get; set; }

    [Required]
    [ForeignKey("Houses")]
    public int houseId { get; set; }
    public virtual Houses Houses { get; set; }
}
