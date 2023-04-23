using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

using Core.Repository;
using Core.Models;
//using Core.ModelsDto;
//using Api.Mapping;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class UserController : ControllerBase
{

    private readonly ILogger<UserController> _logger;
    private readonly IRepository _repository;

    public UserController(ILogger<UserController> logger, IRepository repository )
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpGet("TestController")]
    public async Task<IActionResult> PhotoMarkup(){
        return Ok(new {requestStatus = "Ok"});
    }
    [HttpPost("PhotoMarkup")]
    public async Task<IActionResult> PhotoMarkup([FromBody] MarkupDto Dto){
        await _repository.TestAddRangeAsync(Dto.Photos);
        _logger.LogInformation("Pushed marukps into a database");
        return Ok(new {requestStatus = "Ok"});
    }
}
