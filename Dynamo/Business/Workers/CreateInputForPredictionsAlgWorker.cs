using Dynamo.Business.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Timers;
using CsvHelper;
using System.Globalization;
using System.Diagnostics;
using Dynamo.Business.Models;

namespace Dynamo.Business.Workers;


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
        System.Timers.Timer timer = new System.Timers.Timer(10000);
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

        // PART 1 - FILL UP ENERGY MEASUREMENTS LIST FROM DB
        List<EnergyDataForPrediction> energyMeasurements = new List<EnergyDataForPrediction>();
        List<Houses> houses = await db.Houses
                            .AsNoTracking()
                            .ToListAsync();

        foreach (Houses house in houses)
        {
            
            List<HouseAliases> houseAliases = await db.HouseAliases
                            .Where(ha => ha.houseId == house.id && ha.MeasurementsAlias != null)
                            .AsNoTracking()
                            .ToListAsync();
            string measurementsAlias = houseAliases.FirstOrDefault().MeasurementsAlias;

            // Get all measurements for house, for today
            List<EnergyMeasurements> measurements = await db.EnergyMeasurements
                            .Where(m => m.houseId == house.id && (
                            m.measurementDatetime.DayOfYear == DateTime.Today.DayOfYear ||
                            m.measurementDatetime.DayOfYear == DateTime.Today.DayOfYear -1 ||
                            m.measurementDatetime.DayOfYear == DateTime.Today.DayOfYear -2 ||
                            m.measurementDatetime.DayOfYear == DateTime.Today.DayOfYear -3)
                            )
                            .OrderBy(x => x.measurementDatetime)
                            .AsNoTracking()
                            .ToListAsync();
            foreach (EnergyMeasurements measure in measurements)
            {
                if (measure != null)
                {
                    energyMeasurements.Add(new EnergyDataForPrediction
                    {
                        consumption = measure.consumption,
                        production = measure.production,
                        houseId = measurementsAlias,
                        measurementDatetime = measure.measurementDatetime,
                    });
                }
            }
        }

        // PART 2 - FILL UP CSV AND SAVE
        FileStream fileStream = new FileStream($"{path}/data/inputPredictions.csv", FileMode.Create);
        using (StreamWriter sw = new StreamWriter(fileStream))
        using (CsvWriter cw = new CsvWriter(sw, CultureInfo.InvariantCulture))
        {
            cw.WriteHeader<EnergyDataForPrediction>();
            cw.NextRecord();
            foreach (EnergyDataForPrediction measurement in energyMeasurements)
            {
                cw.WriteRecord(measurement);
                cw.NextRecord();
            }
        }

        // PART 3 - CALL PREDICTIONS SCRIPT
        ProcessStartInfo processInfo;
        Process process = new Process();

        processInfo = new ProcessStartInfo();
        //processInfo = new ProcessStartInfo("java"); // or /c /k
        //processInfo = new ProcessStartInfo($"{originPath}/hello.bat"); // or /c
        processInfo.CreateNoWindow = false;
        processInfo.UseShellExecute = true;
        //processInfo.Arguments = Command;
        //ProcessInfo.RedirectStandardOutput = true;
        processInfo.FileName = "cmd.exe";
        processInfo.Arguments = $"/k {originPath}/hello.bat";

        process.StartInfo = processInfo;
        process.Start();
        process.WaitForExit();
    }

}
