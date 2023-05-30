using IronPython.Hosting;
using IronPython;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Scripting.Hosting;
using System.Collections;
using System.Timers;
using System.Diagnostics;

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

        //ExecuteCommand();

        _logger.LogInformation("Ending learning");
    }

    /// <summary>
    /// Функция для запуска распознавания
    /// </summary>
    /// <param name="Image">Изображения для распознавания</param>
    /// <returns>Возвращает структуру с информацией о распознанном изображении</returns>
    public async Task<List<OcrRequest>> Recognition(string Image) //api call распознавание
    {
        File.WriteAllText("photo.b64", Image);
        _logger.LogInformation("Starting recognition");

        try{
            ExecuteCommand("python3 ../BusinessLogic/ai/segmentation.py ../BusinessLogic/ai/model/config.yaml ../BusinessLogic/ai/model/model_final.pth photo.b64");
            string OcrResultJson = File.ReadAllText("books.json");
            List<OcrResult> OcrResults = JsonConvert.DeserializeObject<List<OcrResult>>(OcrResultJson);
            List<OcrRequest> OcrRequests = new List<OcrRequest>();
            foreach(var Result in OcrResults){
                var TempOcrRequest = new OcrRequest();
                TempOcrRequest.x1 = Result.x1;
                TempOcrRequest.x2 = Result.x2;
                TempOcrRequest.x3 = Result.x3;
                TempOcrRequest.x4 = Result.x4;
                TempOcrRequest.y1 = Result.y1;
                TempOcrRequest.y2 = Result.y2;
                TempOcrRequest.y3 = Result.y3;
                TempOcrRequest.y4 = Result.y4;
                var PossibleBooks = new List<string>();
                var Books = await _reposiotry.GetPossibleBooksAsync(Result.RecognizedText);
                foreach(var Book in Books){
                    PossibleBooks.Add(Book);
                }
                TempOcrRequest.PossibleBooks = PossibleBooks;
                OcrRequests.Add(TempOcrRequest);
            }
            foreach(var Result in OcrResults){
                foreach(var text in Result.RecognizedText){
                    Console.WriteLine(text);
                }
            }
            return OcrRequests;
        }catch(Exception ex){
            Console.WriteLine("Скрипт питона упал");
        }

        _logger.LogInformation("Ending recognition");
        return new List<OcrRequest>();
    }
    private void ExecuteCommand(string command)
    {
        // Create a process start info object
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "/bin/bash"; // Change this to the appropriate shell for your system
        startInfo.Arguments = "-c \"" + command + "\"";
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;

        // Start the process and wait for it to complete
        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();

            // Print the output from the command
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }
    }

}
