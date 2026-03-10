using System.Collections.Generic;
using Items.DAL;
using Items.Model;

namespace Items.BLL;

public class Service : IService
{
    private readonly ICrud _db;

    public Service(ICrud db)
    {
        _db = db;
    }
    public IEnumerable<Item> GetAllItems()
    {
        return _db.GetAll();
    }

    public bool AddItem(Item item)
    {
        return _db.Add(item);
    }

    public bool DeleteItem(Item item)
    {
        return _db.Delete(item);
    }

    public bool UpdateItem(Item item)
    {
        return _db.Update(item);
    }
    
}