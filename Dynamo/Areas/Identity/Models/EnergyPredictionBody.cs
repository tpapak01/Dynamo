using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Areas.Identity.Models;


public class EnergyPredictionBody
{
    [Required]
    public DateTime datetime {  get; set; }

    
    public float? forecasted_load_consumption { get; set; }

    public float? forecasted_pv { get; set; }

    [Required]
    public string houseId { get; set; }

    
}
