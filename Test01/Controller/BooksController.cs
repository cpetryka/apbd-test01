using Microsoft.AspNetCore.Mvc;
using Test01.Repository;

namespace Test01.Controller;

[Route("api/books")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;
    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }
    
    [HttpGet("{id}/genres")]
    public async Task<IActionResult> GetBookGenres(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
        {
            Console.WriteLine("----------------------------------------");
            return NotFound($"Book with given ID - {id} doesn't exist");
        }
        
        var bookWithGenres = await _booksRepository.GetBookWithGenres(id);
        
        return Ok(bookWithGenres);
    }
}