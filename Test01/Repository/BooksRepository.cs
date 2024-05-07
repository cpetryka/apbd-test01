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

    public async Task<bool> DoesAllGenresExist(List<int> ids)
    {
	    var allExist = true;
	    var query = "SELECT 1 FROM genres WHERE PK = @ID";

	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    
	    await connection.OpenAsync();
	    
	    foreach (var id in ids)
	    {
		    command.Parameters.Clear();
		    command.Parameters.AddWithValue("@ID", id);

		    var res = await command.ExecuteScalarAsync();

		    if (res is null)
		    {
			    allExist = false;
			    break;
		    }
	    }

	    return allExist;
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
    
    public async Task<GetBookWithGenresDto> AddNewBookWithGenres(CreateBookWithGenresDto createBookWithGenresDto)
    {
	    var insert = @"INSERT INTO books VALUES(@title); SELECT @@IDENTITY AS ID;";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();
	    
	    command.Connection = connection;
	    command.CommandText = insert;
	    
	    command.Parameters.AddWithValue("@title", createBookWithGenresDto.Title);
	    
	    await connection.OpenAsync();
	    

	    var transaction = await connection.BeginTransactionAsync();
	    command.Transaction = transaction as SqlTransaction;
	    
	    var id = await command.ExecuteScalarAsync();
	    
	    try
	    {
		    foreach (var genreId in createBookWithGenresDto.GenreIds)
		    {
			    command.Parameters.Clear();
			    command.CommandText = "INSERT INTO books_genres VALUES(@ID, @genreID)";
			    command.Parameters.AddWithValue("@ID", id);
			    command.Parameters.AddWithValue("@genreID", genreId);

			    await command.ExecuteNonQueryAsync();
		    }

		    await transaction.CommitAsync();
	    }
	    catch (Exception)
	    {
		    await transaction.RollbackAsync();
		    throw;
	    }

	    var result = await GetBookWithGenres(Convert.ToInt32(id));

	    return result;
    }

    public async Task<int> AddNewBook(CreateBookDto createBookDto)
    {
	    var insert = @"INSERT INTO books VALUES(@title); SELECT @@IDENTITY AS ID;";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();
	    
	    command.Connection = connection;
	    command.CommandText = insert;
	    
	    command.Parameters.AddWithValue("@title", createBookDto.Title);
	    
	    await connection.OpenAsync();
	    
	    var id = await command.ExecuteScalarAsync();

	    if (id is null) throw new Exception();
	    
	    return Convert.ToInt32(id);
    }
}