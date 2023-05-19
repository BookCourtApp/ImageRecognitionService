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
using Microsoft.Extensions.Logging;

namespace BusinessLogic;

/// <summary>
/// Класс, отвечающий за взаимодействие с файлами ИИ.
/// С его помощью можно администрировать: обучение, запуск.
/// </summary>
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
    private readonly ILogger<LearningManager> _logger;
    public LearningManager(IRepository repository, ILogger<LearningManager> Logger)
    {
        _reposiotry = repository;
        _logger = Logger;
    }

    /// <summary>
    /// Функция для выбора обучения, при котором пользователь можно задать настройки выбора версии. 
    /// Можно выбрать: Послденюю, лучшую и по айди 
    /// </summary>
    /// <param name="Request">Версию обучения для выбора</param>
    /// <returns>Возвращает выбранну версию обучения</returns>
    public async Task<LearnSession> PickLearnVersion(string Request) //api call выставить версию обучения(а.к.а выставить рабочие веса)
    {
        switch (Request)
        {
            case "Latest":
                _currentLearnVersion = await _reposiotry.GetLatestLearnSession();
                break;
            case "Best":
                _currentLearnVersion = await _reposiotry.GetBestLearnSession();
                break;
            default:
                Guid Id = new Guid(Request);
                _currentLearnVersion = await _reposiotry.GetLearnSessionById(Id);
                break;
        }
        _logger.LogInformation($"Learn version picked: {Request}");
        return _currentLearnVersion;
    }
    /// <summary>
    /// Функция, позволяющая настроить автоматическое обучение ИИ 
    /// </summary>
    /// <param name="Config">Конфигурая плана для запуска обучений</param>
    public void StartPlan(PlanConfig Config) //api call запланировать 
    {
        _currentPlanConfig = Config;
        StopPlan();
        LearnTimer.Interval = TimeInterval[Config.LearnDate];
        LearnTimer.AutoReset = Config.AutoReset;
        LearnTimer.Elapsed += StartLearningByEvent;
        LearnTimer.Start();
        _logger.LogInformation("Learning plan is configured");
    }
    /// <summary>
    /// Функция для получения текущего плана автоматического запуска обучений
    /// </summary>
    /// <returns>Возвращает текущий план автоматического запуска обучений ИИ</returns>
    public PlanConfig GetCurrentPlanConfig()
    {
        _logger.LogInformation("Current plan config returned");
        return _currentPlanConfig;
    }
    /// <summary>
    /// Функция для получения информации о текущей версии обучения для алгоритмов распознавания 
    /// </summary>
    /// <returns>Возращает текущую версию обучения для запуска распознанвания</returns>
    public LearnSession GetCurrentLearnVersion(){
        _logger.LogInformation("Current learn version returned");
        return _currentLearnVersion;
    }
    /// <summary>
    /// Функция для получения информации о всех версия обучений
    /// </summary>
    /// <returns>Возвращает список всех версий обучений </returns>
    public async Task<List<LearnSession>> ShowLearnSessions()
    {
        var result = await _reposiotry.GetAllLearnSessions();
        _logger.LogInformation("Learn session returned");
        return result;
    }
    /// <summary>
    /// Функция для остановки автоматического обучения ИИ
    /// </summary>
    public void StopPlan()
    {
        LearnTimer.Enabled = false;
        _logger.LogInformation("Learning plan stopped ");
    } 
    /// <summary>
    /// Функция для исполнения функции StartLearning, но в качестве ивента, если включен режим автоматического обучения
    /// </summary>
    /// <param name="source">Служебный параметр ивента</param>
    /// <param name="e">Служебный параметр ивента</param>
    public void StartLearningByEvent(Object source, ElapsedEventArgs e)
    {
        _logger.LogInformation("Starting learning by event");
        StartLearning();
    } 
    /// <summary>
    /// Функция для запуска обучения ИИ
    /// </summary>
    public void StartLearning() //api call принудительный старт
    {
        _logger.LogInformation("Starting learning");

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

        _logger.LogInformation("Ending learning");
    }

    /// <summary>
    /// Функция для запуска распознавания
    /// </summary>
    /// <param name="Image">Изображения для распознавания</param>
    /// <returns>Возвращает структуру с информацией о распознанном изображении</returns>
    public async Task<RecognizedImage> Recognition(string Image) //api call распознавание
    {
        //string jsonString = System.Text.Json.JsonSerializer.Serialize(Image);
        _logger.LogInformation("Starting recognition");

        var runtime = Python.CreateRuntime();
        var engine = runtime.GetEngine("python");
        var Paths = SetPaths(engine);
        engine.SetSearchPaths((ICollection<string>)Paths);
        var scope = engine.CreateScope();
        try{
            var argv = new List<string>();
            string[] args = { "../BusinessLogic/prog.py", Image};
            args.ToList().ForEach(a => argv.Add(a));

            runtime.GetSysModule().SetVariable("argv", argv);

            await Task.Run(() =>
            {
                engine.ExecuteFile(args[0], scope);
            });

            var result = scope.GetVariable("result");
            //Console.WriteLine(result);
            _logger.LogInformation("Ending recognition");
            return JsonConvert.DeserializeObject(result);
        }
        catch(Exception ex){
            Console.WriteLine($"ERROR: {ex.Message}");
            _logger.LogInformation("Ending recognition with error");
            return new RecognizedImage();
        }

    }
    /// <summary>
    /// Функция, нужная для работы библиотеки IronPython.
    /// </summary>
    /// <param name="engine">Служебный параметр</param>
    /// <returns>Возвращает список библиотек для запуска скрипта питона</returns>
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
