using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamo.Business.Models;


public class EnergyDataFromPrediction
{
    
    public DateTime Datetime { get; set; }


    public float? Forecasted_Load_Consumption { get; set; }

    public float? Forecasted_PV { get; set; }

    public string House_ID { get; set; }

    public float? Reliability_PV_24h { get; set; }

    public float? Reliability_Load_24h { get; set; }

}
