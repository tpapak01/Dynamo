using Dynamo.Business.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using System.Timers;
using CsvHelper;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Diagnostics;

namespace Dynamo.Business.Models;


public class ReadFromPredictionsAlgWorker
{
    private static ILogger logger;

    private static BackgroundWorker worker;

    private readonly DynamoContext db;
    private readonly IWebHostEnvironment webHostEnvironment;

    private static string path;
    private static string originPath;
    public ReadFromPredictionsAlgWorker(DynamoContext dbcontext, IWebHostEnvironment hostEnvironment)
    {
        
        webHostEnvironment = hostEnvironment;
        path = webHostEnvironment.WebRootPath;
        originPath = webHostEnvironment.ContentRootPath;
        
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
        var filename = $"{path}/data/responsePredictions.csv";
        if (!System.IO.File.Exists(filename))
        {
            return;
        }
        FileStream fileStream = new FileStream(filename, FileMode.Open);
        using (StreamReader sr = new StreamReader(fileStream))
        using (CsvReader cr = new CsvReader(sr, CultureInfo.InvariantCulture))
        {
            List<EnergyDataFromPrediction> dataFromPred = cr.GetRecords<EnergyDataFromPrediction>().ToList();
           foreach (EnergyDataFromPrediction predictionData in dataFromPred)
           {
                HouseAliases houseAlias = new HouseAliases();
                houseAlias.MeasurementsAlias = predictionData.House_ID;

                List<HouseAliases> houseAliases = await db.HouseAliases
                    .Where(h => h.MeasurementsAlias == houseAlias.MeasurementsAlias)
                    .AsNoTracking() //fast fast
                    .ToListAsync();

                EnergyPredictions energyPrediction = new EnergyPredictions();
                energyPrediction.production = predictionData.Forecasted_PV;
                energyPrediction.consumption = predictionData.Forecasted_Load_Consumption;
                energyPrediction.predictionDatetime = predictionData.Datetime;
                energyPrediction.houseId = houseAliases[0].houseId;

                db.Add(energyPrediction);
            }
        }
        await db.SaveChangesAsync();

    }

}
