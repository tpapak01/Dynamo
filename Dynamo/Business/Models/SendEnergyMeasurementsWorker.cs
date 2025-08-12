using Dynamo.Business.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using System.Timers;

namespace Dynamo.Business.Models;


public class SendEnergyMeasurementsWorker
{
    private static ILogger logger;

    private static BackgroundWorker worker;

    private readonly DynamoContext db;
    public SendEnergyMeasurementsWorker(DynamoContext dbcontext)
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
        //logger.LogInformation("Hmmm...");
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", "AAA");
        httpClient.BaseAddress = new Uri("https://localhost:7227/");
        // TODO: replace body with EnergyDataElectiBody
        List<EnergyDataBody> model = new List<EnergyDataBody>();

        List<Houses> houses = await db.Houses
                            .AsNoTracking()
                            .ToListAsync();
        
        foreach (Houses house in houses)
        {
            List<HouseAliases> houseAliases = await db.HouseAliases
                            .Where(ha => ha.houseId == house.id && ha.ElectiAlias != null)
                            .AsNoTracking()
                            .ToListAsync();
            string electiAlias = houseAliases.FirstOrDefault().ElectiAlias;

            List<EnergyMeasurements> measurements = await db.EnergyMeasurements
                            .Where(m => m.houseId == house.id)
                            .OrderByDescending(x => x.measurementDatetime)
                            .AsNoTracking()
                            .ToListAsync();
            EnergyMeasurements measure = measurements.FirstOrDefault();
            if (measure != null)
            {
                model.Add(new EnergyDataBody
                {
                    consumption = measure.consumption,
                    production = measure.production,
                    houseIdentifier = electiAlias,
                    measurementDatetime = measure.measurementDatetime,
                });
            }
        }

        logger.LogInformation(model[0].measurementDatetime.ToString());
        var stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/energydata", stringContent);

    }
}
