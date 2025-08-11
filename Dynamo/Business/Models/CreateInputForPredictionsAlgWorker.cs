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

namespace Dynamo.Business.Models;


public class CreateInputForPredictionsAlgWorker
{
    private static ILogger logger;

    private static BackgroundWorker worker;

    private readonly DynamoContext db;
    private readonly IWebHostEnvironment webHostEnvironment;

    private static string path;
    private static string originPath;
    public CreateInputForPredictionsAlgWorker(DynamoContext dbcontext, IWebHostEnvironment hostEnvironment)
    {
        
        webHostEnvironment = hostEnvironment;
        path = webHostEnvironment.WebRootPath;
        originPath = webHostEnvironment.ContentRootPath;
        
        db = dbcontext;
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        logger = factory.CreateLogger("Program");

        worker = new BackgroundWorker();
        worker.DoWork += worker_DoWork;
        System.Timers.Timer timer = new System.Timers.Timer(30000);
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
        //String finalFileName = $"{path}/results/Winner/Pareto";
        List<EnergyMeasurements> energyMeasurements = new List<EnergyMeasurements>();

        using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(true)))
        using (CsvWriter cw = new CsvWriter(sw, CultureInfo.InvariantCulture))
        {
            cw.WriteHeader<EnergyMeasurements>();
            cw.NextRecord();
            foreach (EnergyMeasurements measurement in energyMeasurements)
            {
                cw.WriteRecord<EnergyMeasurements>(measurement);
                cw.NextRecord();
            }
        }
    }

}
