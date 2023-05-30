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
public class MarkupController : ControllerBase
{

    private readonly ILogger<MarkupController> _logger;
    private readonly IRepository _repository;

    public MarkupController(ILogger<MarkupController> logger, IRepository repository )
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
        BooksImage Image = new BooksImage
        {
            Image   = Dto.Image,
            Markups = Dto.Markups
        };
        await _repository.MarkupAddAsync(Image);
        _logger.LogInformation("Pushed marukps into a database");
        return Ok(new {requestStatus = "Ok"});
    }
}
