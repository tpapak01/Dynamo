using Dynamo.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Areas.Identity.Models
{
    public class MyAPIs
    {
        private static Dictionary<Type, PropertyInfo> _IdLookup = new();

        //private static ILogger Logger;

        internal static void Initialize<D, C>(ILogger logger)
            where D : DbContext
            where C : class
        {

            //Logger = logger;

            var theType = typeof(C);
            var idProp = theType.GetProperty("id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? theType.GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));

            if (idProp != null)
            {
                _IdLookup.Add(theType, idProp);
            }

        }

        internal static void ReceiveEnergyMeasurements(IEndpointRouteBuilder app, string url)
        {
            //Logger.LogInformation($"Created API: HTTP POST\t{url}");

            app.MapPost(url, async ([FromServices] DynamoContext db, IValidator<List<EnergyDataBody>> validator, [FromBody] List<EnergyDataBody> energyDataBodyList) =>
            {
                var validationResult = await validator.ValidateAsync(energyDataBodyList);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                foreach (EnergyDataBody energyDataBody in energyDataBodyList)
                {
                    HouseAliases houseAlias = new HouseAliases();
                    houseAlias.MeasurementsAlias = energyDataBody.houseIdentifier;

                    List<HouseAliases> houseAliases = await db.HouseAliases
                        .Where(h => h.MeasurementsAlias == houseAlias.MeasurementsAlias)
                        .AsNoTracking() //fast fast
                        .ToListAsync();

                    EnergyMeasurements energyMeasurement = new EnergyMeasurements();
                    energyMeasurement.production = energyDataBody.production;
                    energyMeasurement.consumption = energyDataBody.consumption;
                    energyMeasurement.measurementDatetime = energyDataBody.measurementDatetime;
                    energyMeasurement.houseId = houseAliases[0].id;

                    db.Add(energyMeasurement);
                }
                await db.SaveChangesAsync();
                return Results.Created($"{url}", energyDataBodyList);
            })
            .WithName("energydata")
            .WithOpenApi();

        }

        internal static void SendEnergyPredictions(IEndpointRouteBuilder app, string url)
        {
            //Logger.LogInformation($"Created API: HTTP POST\t{url}");

            app.MapGet(url, async ([FromServices] DynamoContext db) =>
            {
                List<EnergyPredictionBody> energyPredictionBodyList = new List<EnergyPredictionBody> ();
                List<Houses> houses = await db.Houses
                         .AsNoTracking() //fast fast
                         .ToListAsync();
                foreach (Houses house in houses)
                {
                    List<EnergyPredictions> housePredictions = await db.EnergyPredictions
                         .Where(e => e.houseId == house.id && e.predictionDatetime.DayOfYear == DateTime.Today.DayOfYear)
                         .OrderByDescending(x => x.predictionDatetime)
                         .AsNoTracking() //fast fast
                         .ToListAsync();

                    List<HouseAliases> houseAliases = await db.HouseAliases
                        .Where(ha => ha.houseId == house.id && ha.ElectiAlias != null)
                        .AsNoTracking() //fast fast
                        .ToListAsync();
                    String electiAlias = houseAliases.FirstOrDefault().ElectiAlias;

                    foreach (EnergyPredictions prediction in housePredictions)
                    {
                        EnergyPredictionBody energyPredictionBody = new EnergyPredictionBody();
                        energyPredictionBody.forecasted_pv = prediction.production;
                        energyPredictionBody.forecasted_load_consumption = prediction.consumption;
                        energyPredictionBody.datetime = prediction.predictionDatetime;
                        energyPredictionBody.houseId = electiAlias;

                        energyPredictionBodyList.Add(energyPredictionBody);
                    }
                }
                return Results.Created($"{url}", energyPredictionBodyList);
            })
            .WithName("energypredictions")
            .WithOpenApi();

        }
    }
}
