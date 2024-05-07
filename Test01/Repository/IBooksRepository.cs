using Test01.Model.Dto;

namespace Test01.Repository;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);

    Task<List<GetGenreDto>> GetBookGenres(int id);
    Task<GetBookWithGenresDto> GetBookWithGenres(int id);
}