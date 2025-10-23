namespace Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAll();
    Task<T> GetById(long id);
    Task<T> Add(T obj);
    Task<List<T>> AddList(List<T> objs);
    Task Delete(long id);
    Task<T> Update(T obj);
}