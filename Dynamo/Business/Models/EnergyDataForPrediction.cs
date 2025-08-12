using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamo.Business.Models;


public class EnergyDataForPrediction
{
    
    public DateTime measurementDatetime { get; set; }


    public float? consumption { get; set; }

    public float? production { get; set; }

    public string houseId { get; set; }



}
