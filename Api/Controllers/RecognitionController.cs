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
public class RecognitionController : ControllerBase
{

    private readonly ILogger<RecognitionController> _logger;
    private readonly IRepository _repository;
    private readonly LearningManager _learningManager;

    public RecognitionController(ILogger<RecognitionController> logger, IRepository repository, LearningManager learningManager)
    {
        _logger = logger;
        _repository = repository;
        _learningManager = learningManager;
    }

    [HttpGet("RecognizeTest")]
    public IActionResult RecognizeTest(){
        return Ok(new {requestStatus = "Ok"});
    }
    [HttpPost("Recognition")]
    public async Task<IActionResult> Recognition([FromBody] Object Image){ //!!!!!!!!!!!11
        await _learningManager.Recognition((string)Image); //!!!!! нужно решить, что будет подаваться в эту функцию api
        return Ok(new {requestStatus = "Ok"});
    }
}
