using Microsoft.AspNetCore.Mvc;
using Core.Repository;
using Core.Models;
using BusinessLogic;
using Nest;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class RecognitionController : ControllerBase
{
    private readonly ILogger<RecognitionController> _logger;
    private readonly IRepository _repository;
    private readonly LearningManager _learningManager;
    public RecognitionController(
        ILogger<RecognitionController> logger, 
        IRepository repository, 
        LearningManager learningManager)
    {
        _logger = logger;
        _repository = repository;
        _learningManager = learningManager;
    }
    [HttpPost("Recognition")]
    public async Task<IActionResult> Recognition([FromBody] ImageDto Dto){
        string ToDelete = "data:image/jpeg;base64,";
        if(Dto.Image.StartsWith(ToDelete)){
            Dto.Image = Dto.Image.Remove(0, ToDelete.Length);
        }
        var RecognitionResult = await _learningManager.Recognition(Dto.Image); 
        foreach(var result in RecognitionResult){
            //result.PossibleBooks = result.PossibleBooks.Select(b => b.ToLower()).Distinct().ToList();
            Console.WriteLine($"amount:{result.PossibleBooks.Count} ");
        }
        return Ok(new RecognitionRequestDto{Result = RecognitionResult});
    }
    [HttpPost("DbTest")]
    public async Task<IActionResult> DbTest([FromBody] DbTestDto DbDto){
        var Books = await _repository.GetBooksByKeywordAsync(DbDto.Keyword);
        var ConvertMe = new List<string>();
        foreach(var hit in Books.Hits){
            ConvertMe.Add(hit.Source.Info);
        }
        var Response = new DbTestRequest
        {
            Books = ConvertMe
        };
        return Ok(Response);
    }
    [HttpPost("DbTest2")]
    public async Task<IActionResult> DbTest([FromBody] DbTestDto2 DbDto){
        
        var Response = new DbTestRequest
        {
            Books = await _learningManager.GetPossibleBooksAsync(DbDto.Keywords)
        };
        return Ok(Response);
    }
    [HttpGet("test")]
    public async Task<IActionResult> Test(){
        
        return Ok("Hello");
    }
}

public class DbTestDto{
    public string Keyword { get; set; }
}
public class DbTestDto2{
    public List<string> Keywords { get; set; }
}
public class DbTestRequest
{
    public List<string> Books{ get;set;}
}

//[HttpGet("TransferData")]
//public async Task<IActionResult> Transfer(){

//    await _repository.TransferData();
//    return Ok(new {status = "Data was transfered"});
//}