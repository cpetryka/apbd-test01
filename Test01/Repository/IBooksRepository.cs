namespace Test01.Repository;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
}