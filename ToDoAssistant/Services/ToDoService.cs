using System.Collections.Generic;
using System.Linq;
using ToDoAssistant.Models;

namespace ToDoAssistant.Services
{
    public class ToDoService
    {
        private readonly List<ToDoItem> _toDoItems = new List<ToDoItem>();

        public IEnumerable<ToDoItem> GetAll() => _toDoItems;

        public ToDoItem Add(string description)
        {
            var item = new ToDoItem { Id = _toDoItems.Count + 1, Description = description, IsDone = false };
            _toDoItems.Add(item);
            return item;
        }

        public void MarkAsDone(int id)
        {
            var item = _toDoItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.IsDone = true;
            }
        }

        public void Remove(int id)
        {
            var item = _toDoItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _toDoItems.Remove(item);
            }
        }
    }
}
