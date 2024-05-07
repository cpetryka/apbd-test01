using Microsoft.Data.SqlClient;
using Test01.Model.Dto;

namespace Test01.Repository;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;
    
    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM Books WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<List<GetGenreDto>> GetBookGenres(int id)
    {
	    var query =	@"
						SELECT name
						FROM books
						JOIN books_genres ON books.PK = books_genres.FK_book
						JOIN genres ON books_genres.FK_genre = genres.PK
						WHERE books.PK = @ID;
					";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@ID", id);
	    
	    await connection.OpenAsync();

	    var reader = await command.ExecuteReaderAsync();
	    
	    // Przetwarzamy wynik zapytania
	    var genres = new List<GetGenreDto>();

	    int genreNameOrdinal = reader.GetOrdinal("name");

	    while (await reader.ReadAsync())
	    {
		    genres.Add(new GetGenreDto()
		    {
			    Genre = reader.GetString(genreNameOrdinal)
		    });
	    }

	    return genres;
    }

    public async Task<GetBookWithGenresDto> GetBookWithGenres(int id)
    {
	    var query =	@"
						SELECT PK, title
						FROM books
						WHERE books.PK = @ID;
					";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@ID", id);
	    
	    await connection.OpenAsync();

	    var reader = await command.ExecuteReaderAsync();
	    
	    // Przetwarzamy wynik zapytania
	    GetBookWithGenresDto getBookWithGenresDto = null;

	    int idOrdinal = reader.GetOrdinal("PK");
	    int titleOrdinal = reader.GetOrdinal("title");

	    while (await reader.ReadAsync())
	    {
		    if (getBookWithGenresDto == null)
		    {
			    getBookWithGenresDto = new GetBookWithGenresDto()
			    {
				    Id = reader.GetInt32(idOrdinal),
				    Title = reader.GetString(titleOrdinal),
				    Genres = GetBookGenres(id).Result.ConvertAll(input => input.Genre)
			    };
		    }
	    }

	    if (getBookWithGenresDto == null)
	    {
		    throw new Exception("GetBookWithGenresDto instance is null");
	    }
	    
	    return getBookWithGenresDto;
    }
}