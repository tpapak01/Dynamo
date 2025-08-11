using Dynamo.Business.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using System.Timers;

namespace Dynamo.Business.Models;


public class MyWorker
{
    private static ILogger logger;

    private static BackgroundWorker worker;

    private readonly DynamoContext db;
    public MyWorker(DynamoContext dbcontext)
    {
        db = dbcontext;
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        logger = factory.CreateLogger("Program");

        worker = new BackgroundWorker();
        worker.DoWork += worker_DoWork;
        System.Timers.Timer timer = new System.Timers.Timer(20000);
        timer.Elapsed += timer_Elapsed;
        timer.Start();
    }

    void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (!worker.IsBusy)
            worker.RunWorkerAsync();
    }

    async void worker_DoWork(object sender, DoWorkEventArgs e)
    {
        logger.LogInformation("Hmmm...");
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", "AAA");
        httpClient.BaseAddress = new Uri("https://localhost:7227/");
        List<EnergyDataBody> model = new List<EnergyDataBody>();

        List<HouseAliases> houseAliases = await db.HouseAliases
                            .AsNoTracking() //fast fast
                            .ToListAsync();
        foreach (HouseAliases houseAlias in houseAliases)
        {
            List<EnergyMeasurements> measurements = await db.EnergyMeasurements
                            .Where(m => m.houseId == houseAlias.houseId)
                            .OrderByDescending(x => x.measurementDatetime)
                            .AsNoTracking() //fast fast
                            .ToListAsync();
            EnergyMeasurements measure = measurements.FirstOrDefault();
            if (measure != null)
            {
                model.Add(new EnergyDataBody
                {
                    consumption = measure.consumption,
                    production = measure.production,
                    houseIdentifier = houseAlias.MeasurementsAlias,
                    measurementDatetime = measure.measurementDatetime,
                });
            }
        }

        logger.LogInformation(model[0].measurementDatetime.ToString());
        var stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        //var response = await httpClient.GetAsync("/weatherforecast");
        var response = await httpClient.PostAsync("/energydata", stringContent);

    }
}
