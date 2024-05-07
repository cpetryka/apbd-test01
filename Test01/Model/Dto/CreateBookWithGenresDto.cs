namespace Test01.Model.Dto;

public class CreateBookWithGenresDto
{
    public string Title { get; set; } = string.Empty;
    public List<int> GenreIds { get; set; } = null!;
}

public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
}