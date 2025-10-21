namespace TaskWeb.Repositories;

using TaskWeb.Models;

public interface ITagRepository
{
    void Create(Tag tag);
    List<Tag> Read();
    Tag Read(int id);
    void Update(Tag tag);
    void Delete(int id);
}