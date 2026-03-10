using System.Collections;
using System.Collections.Generic;
using Items.Model;

namespace Items.BLL;

public interface IService
{
    public IEnumerable<Item>? GetAllItems();
    public bool AddItem(Item item);
    public bool DeleteItem(Item item);
    public bool UpdateItem(Item item);
}