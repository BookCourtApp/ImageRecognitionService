using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

using Core.Repository;
using Core.Models;
using Api.Mapping;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class UserController : ControllerBase
{

    private readonly ILogger<UserController> _logger;
    private readonly IRepository _repository;
    private readonly PhotoMapper _photoMapper;

    public UserController(ILogger<UserController> logger, IRepository repository, PhotoMapper photoMapper)
    {
        _logger = logger;
        _repository = repository;
        _photoMapper = photoMapper;
    }

    [HttpGet("TestController")]
    public async Task<IActionResult> PhotoMarkup(){
        return Ok(new {requestStatus = "Ok"});
    }
    [HttpPost("PhotoMarkup")]
    public async Task<IActionResult> PhotoMarkup([FromBody] JsonElement jasonString){
        List<Photo> Photos = _photoMapper.ProceedAllPhotos(jasonString);
        await _repository.TestAddRangeAsync(Photos);
        return Ok(new {requestStatus = "Ok"});
    }
}
