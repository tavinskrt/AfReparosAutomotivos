namespace TaskWeb.Repositories;

using System.Collections.Generic;
using System.Collections.Immutable;
using TaskWeb.Models;

public class TagMemoryRepository : ITagRepository
{
    private List<Tag> lista = new List<Tag>();

    public void Create(Tag tag)
    {
        tag.TagId = Math.Abs((int)DateTimeOffset.Now.ToUnixTimeMilliseconds());
        lista.Add(tag);
    }

    public void Delete(int id)
    {
        // SELECT * FROM lista WHERE TagId = id
        // var tags = from item in lista
        //            where item.TagId == id
        //            select item;

        // var tag = tags.Single();

        var tag = lista.SingleOrDefault((e) => e.TagId == id);
        lista.Remove(tag);
    }

    public List<Tag> Read()
    {
        return lista;
    }

    public Tag Read(int id)
    {
        return lista.SingleOrDefault((e) => e.TagId == id);
    }

    public void Update(Tag tag)
    {
        var _tag = lista.SingleOrDefault((e) => e.TagId == tag.TagId);
        _tag.Title = tag.Title;
    }
}