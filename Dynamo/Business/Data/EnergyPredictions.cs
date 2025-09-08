using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamo.Business.Data;
public class EnergyPredictions
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    [Required]
    public DateTime predictionDatetime { get; set; }


    public float? consumption { get; set; }

    public float? production { get; set; }

    public float? reliabilityScoreProd { get; set; }

    public float? reliabilityScoreCons { get; set; }

    [Required]
    [ForeignKey("Houses")]
    public int houseId { get; set; }
    public virtual Houses Houses { get; set; }


}
