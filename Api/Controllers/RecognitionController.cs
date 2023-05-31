using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

using Core.Repository;
using Core.Models;
using BusinessLogic;
//using Core.ModelsDto;
//using Api.Mapping;

namespace Api.Controllers;
public class Test{
    public string Name { get; set; }
    public List<string> Words { get; set; }
}

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

    [HttpGet("TransferData")]
    public async Task<IActionResult> Transfer(){

        await _repository.TransferData();
        return Ok(new {status = "Data was transfered"});
    }
    [HttpGet("RecognizeTest")]
    public IActionResult RecognizeTest(){

        var test = new Test { Name = "Pedir", Words = new List<string>{"ONe", "two", "three"}};
        return Ok(test);
    }
    [HttpPost("DbTest")]
    public IActionResult DbTest([FromBody] DbTestDto DbDto){
        var Books = _repository.GetPossibleBooksAsync(DbDto.Keywords);
        return Ok(new {PossibleBooks = Books.Result});
    }
    [HttpPost("Recognition")]
    public async Task<IActionResult> Recognition([FromBody] ImageDto Dto){ 
        var RecognitionResult = await _learningManager.Recognition(Dto.Image); 
        return Ok(new RecognitionRequestDto{Result = RecognitionResult});
    }
}

public class DbTestDto{
    public List<string> Keywords { get; set; }
}