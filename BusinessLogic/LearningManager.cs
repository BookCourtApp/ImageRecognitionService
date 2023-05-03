using IronPython.Hosting;
using IronPython;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Scripting.Hosting;
using System.Collections;
using System.Timers;

using Core.Repository;
using Core.Models;
using Infrastructure.DataAccessLayer;

namespace BusinessLogic;

public class LearningManager
{
    private Dictionary<string, uint> TimeInterval = new Dictionary<string, uint>{
        {"Day", 86400000},
        {"Week", 604800000},
        {"Month", 2592000000},
    };
    private readonly IRepository _reposiotry;
    private System.Timers.Timer LearnTimer;
    private string _weights;
    private LearnSession _currentLearnVersion;
    private PlanConfig _currentPlanConfig;
    public LearningManager(IRepository repository){
        _reposiotry = repository;
    }

    public async Task<LearnSession> PickLearnVersion(string Request) //api call выставить версию обучения(а.к.а выставить рабочие веса)
    {
        //Админ может запросить выставить версию обучения в виде трех вариантов:
        //1. - это последняя +
        //2. - это самая лучшая +
        //3. - это по номеру, которые он хочет из списка, который вызвал и ShowLearnSessions +
        switch(Request){
            case "Latest":
                _currentLearnVersion = await _reposiotry.GetLatestLearnSession();
                return _currentLearnVersion;
            case "Best":
                _currentLearnVersion = await _reposiotry.GetBestLearnSession();
                return _currentLearnVersion;
            default:
                Guid Id = new Guid(Request);
                _currentLearnVersion = await _reposiotry.GetLearnSessionById(Id);
                return _currentLearnVersion;
        }
    }


    public void StartPlan(PlanConfig Config) //api call запланировать 
    {
        _currentPlanConfig = Config;
        StopPlan();
        LearnTimer.Interval = TimeInterval[Config.LearnDate];
        LearnTimer.AutoReset = Config.AutoReset;
        LearnTimer.Elapsed += StartLearningByEvent;
        LearnTimer.Start();
    }
    public PlanConfig GetCurrentPlanConfig() => _currentPlanConfig;
    public LearnSession GetCurrentLearnVersion() => _currentLearnVersion;
    public async Task<List<LearnSession>> ShowLearnSessions() => await _reposiotry.GetAllLearnSessions();

    public void StopPlan() //api call остановить план
    {
        LearnTimer.Enabled = false;
    }

    public void StartLearningByEvent(Object source, ElapsedEventArgs e) => StartLearning();
    public void StartLearning() //api call принудительный старт
    {
        //List<Photo> Photos = await _reposiotry.GetAllPhotosAsync();
        //Console.WriteLine($"\nResult: {Photos[0].BookMarkups[0].TextMarkups[0].Text}\n");
        //string jsonString = JsonSerializer.Serialize(Photos);//JsonSerializer.Serialize(Photos);

        //var runtime = Python.CreateRuntime();
        //var engine = runtime.GetEngine("python");
        //var Paths = SetPaths(engine);
        //engine.SetSearchPaths((ICollection<string>)Paths);
        //var scope = engine.CreateScope();
        //try{
        //    var argv = new List<string>();
        //    string[] args = {"../BusinessLogic/prog.py", jsonString};
        //    args.ToList().ForEach(a => argv.Add(a));

        //    runtime.GetSysModule().SetVariable("argv", argv);

        //    await Task.Run(() =>
        //    {
        //        engine.ExecuteFile(args[0], scope);
        //    });

        //    var result = scope.GetVariable("result");
        //    //Console.WriteLine(result);

        //}
        //catch(Exception ex){
        //    Console.WriteLine($"ERROR: {ex.Message}");
        //}
        //LearnSession NewSession = new LearnSession //берем полученное обучение и пушим в бд
        //{
        //    Version = "One",
        //    LearnTime = 5.5f,
        //    Precision = .4f,
        //    LearnDate = default!,
        //    Weights = "5.5|5.6|5.7"
        //};
        //await _reposiotry.AddLearnSessionAsync(NewSession);
    }

    public async Task<RecognizedImage> Recognition(string Image) //api call распознавание
    {
        string jsonString = System.Text.Json.JsonSerializer.Serialize(Image);

        var runtime = Python.CreateRuntime();
        var engine = runtime.GetEngine("python");
        var Paths = SetPaths(engine);
        engine.SetSearchPaths((ICollection<string>)Paths);
        var scope = engine.CreateScope();
        try{
            var argv = new List<string>();
            string[] args = {"../BusinessLogic/prog.py", jsonString};
            args.ToList().ForEach(a => argv.Add(a));

            runtime.GetSysModule().SetVariable("argv", argv);

            await Task.Run(() =>
            {
                engine.ExecuteFile(args[0], scope);
            });

            var result = scope.GetVariable("result");
            //Console.WriteLine(result);

            return JsonConvert.DeserializeObject(result);
        }
        catch(Exception ex){
            Console.WriteLine($"ERROR: {ex.Message}");
            return new RecognizedImage();
        }

    }
    private ICollection SetPaths(ScriptEngine engine) //служебные функция для работы IronPython
    {
        var Paths = engine.GetSearchPaths();
        Paths.Add(@"/usr/lib/python38.zip");
        Paths.Add(@"/usr/lib/python3.8");
        Paths.Add(@"/usr/lib/python3.8/lib-dynload");
        Paths.Add(@"/usr/local/lib/python3.8/dist-packages");
        Paths.Add(@"/usr/lib/python3/dist-packages");
        return (ICollection)Paths;
    }

}

//Не получилось использовать из за переполнение Int типа в Month 
//enum TimeInterval
//{
//    Day = 86400000, // Number of milliseconds in a day.
//    Week = 604800000, // Number of milliseconds in a week.
//    Month = 2592000000 // Number of milliseconds in a month (30 days).
//}