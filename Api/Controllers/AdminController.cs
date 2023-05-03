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
    public IActionResult RecognizeTest(){
        return Ok(new {requestStatus = "Ok"});
    }
    [HttpPost("Recognition")]
    public async Task<IActionResult> Recognition([FromBody] Object Image){ //!!!!!!!!!!!11
        await _learningManager.Recognition((string)Image); //!!!!! нужно решить, что будет подаваться в эту функцию api
        return Ok(new {requestStatus = "Ok"});
    }
}
