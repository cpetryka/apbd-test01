namespace Test01.Model.Dto;

public class GetBookWithGenresDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<String> Genres { get; set; } = null!;
}

public class GetGenreDto
{
    public string Genre { get; set; } = string.Empty;
    
    
}