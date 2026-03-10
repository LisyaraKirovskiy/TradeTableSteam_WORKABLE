using System.Collections;
using System.Collections.Generic;
using Items.Model;
namespace Items.DAL;
//Create Read Update Delete
public interface ICrud
{
    public IEnumerable<Item>? GetAll();
    public bool Add(Item item);
    public bool Delete(Item item);
    public bool Update(Item item);
}