using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Test01.Model.Dto;
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
            return NotFound($"Book with given ID - {id} doesn't exist");
        }
        
        var bookWithGenres = await _booksRepository.GetBookWithGenres(id);
        
        return Ok(bookWithGenres);
    }
    
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddBookWithGenres(CreateBookWithGenresDto createBookWithGenresDto)
    {
        if (!await _booksRepository.DoesAllGenresExist(createBookWithGenresDto.GenreIds))
            return NotFound($"At least one of given ids doesn't exist");

        var result = await _booksRepository.AddNewBookWithGenres(createBookWithGenresDto);

        return Created(Request.Path.Value ?? "api/books", result);
    }
}