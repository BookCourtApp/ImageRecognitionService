using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

using Core.Repository;
using Core.Models;
using BusinessLogic;
//using Core.ModelsDto;
//using Api.Mapping;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class AdminController : ControllerBase
{

    private readonly ILogger<AdminController> _logger;
    private readonly IRepository _repository;
    private readonly LearningManager _learningManager;

    public AdminController(ILogger<AdminController> logger, IRepository repository, LearningManager learningManager)
    {
        _logger = logger;
        _repository = repository;
        _learningManager = learningManager;
    }

    [HttpGet("AdminTest")]
    public IActionResult AdminTest(){
        return Ok(new {requestStatus = "Ok"});
    }
    [HttpGet("StartLearning")]
    public async Task<IActionResult> StartLearning(){ //!!!!!!!!!!!11
        _learningManager.StartLearning();
        return Ok(new {requestStatus = "Learn ended"});
    }
    [HttpGet("ShowLearnVersions")]
    public async Task<IActionResult> ShowLearnVersions(){ //!!!!!!!!!!!11
        var LearnVersions = _learningManager.ShowLearnSessions();
        return Ok(new {LearnVersions = LearnVersions});
    }
    [HttpPost("PickLearnVersion")]
    public async Task<IActionResult> LearnVersionPicker([FromBody] Object Request){ //!!!!!!!!!!!11
        var PickedVersion = _learningManager.PickLearnVersion((string)Request);
        return Ok(new {requestStatus = "Version was chanage", currentVerison = PickedVersion});
    }
    [HttpPost("ChangePlanConfig")]
    public async Task<IActionResult> ChangePlanConfig([FromBody] PlanConfig Request){ //!!!!!!!!!!!11
        _learningManager.StartPlan(Request);
        return Ok(new {requestStatus = "Plan config changed"});
    }
    [HttpGet("GetCurrentVersion")]
    public async Task<IActionResult> GetCurrentVersion(){ //!!!!!!!!!!!11
        var CurrentVersion = _learningManager.GetCurrentLearnVersion();
        return Ok(new {currentVersion = CurrentVersion});
    }
    [HttpGet("GetCurrentPlan")]
    public async Task<IActionResult> GetCurrentPlan(){ //!!!!!!!!!!!11
        var CurrentPlan = _learningManager.GetCurrentPlanConfig();
        return Ok(new {currentPlan = CurrentPlan});
    }
}
