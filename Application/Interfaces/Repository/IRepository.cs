using System.Collections.Generic;

namespace Application.Interfaces.Repository
{
    public interface IRepository<T>
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);

        // Get all entities.
        IEnumerable<T> GetAll();

        // Get entity by primary key.
        T? GetById(int id);
    }
}
