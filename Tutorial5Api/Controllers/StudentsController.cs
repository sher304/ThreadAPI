using Lecture5.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Lecture5.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController : ControllerBase
{

    [HttpGet]
    public IActionResult GetStudents()
    {
        var dane=new StudentsRepository().GetStudents();
        return Ok(dane);
    }

    [HttpGet("async")]
    public async Task<IActionResult> GetStudentsAsync()
    {
        var repository = new StudentsRepository2();
        var result=await repository.GetStudentsAsync();
        return Ok(result);
    }
    
}