using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todolists/{id}/item")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly TodoContext _context;

        public ItemController(TodoContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Item>> PostItem(long id, Item item)
        {
            if (_context.TodoList == null)
                return Problem("Entity set 'TodoContext.TodoList'  is null.");

            if (item == null)
                return Problem("'item'  is null.");

            var list = await _context.TodoList.FindAsync(id);

            if (list == null) return NotFound();

            list?.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostItem", new { id = list?.Items.Last() }, item);
        }

        [HttpGet]
        public async Task<ActionResult<Item>> GetItems(long id)
        {
            if (_context.TodoList == null)
                return Problem("Entity set 'TodoContext.TodoList'  is null.");

            await _context.TodoList.Include(todoList => todoList.Items).ToListAsync();
            var list = await _context.TodoList.FindAsync(id);

            if (list == null) return NotFound();

            return Ok(list.Items);
        }

        [HttpGet("{idItem}")]
        public async Task<ActionResult<Item>> GetItemById(long id, long idItem)
        {
            if (_context.TodoList == null)
                return Problem("Entity set 'TodoContext.TodoList'  is null.");

            await _context.TodoList.Include(todoList => todoList.Items).ToListAsync();
            var list = await _context.TodoList.FindAsync(id);

            if (list == null) return NotFound();

            var item = list?.Items.Where(x => x.Id.Equals(idItem)).SingleOrDefault();
            if (item == null) return NotFound();

            return Ok(item);
        }

        [HttpPut("{idItem}")]
        public async Task<ActionResult<Item>> UpdateItem(long id, long idItem, Item Newitem)
        {
            if (_context.TodoList == null)
                return Problem("Entity set 'TodoContext.TodoList'  is null.");

            await _context.TodoList.Include(todoList => todoList.Items).ToListAsync();
            var list = await _context.TodoList.FindAsync(id);

            if (list == null) return NotFound();

            var item = list.Items.Where(x => x.Id.Equals(idItem)).SingleOrDefault();

            if (item == null) return NotFound();

            item.Name = Newitem.Name;
            item.Done = Newitem.Done;

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(item);
        }


        [HttpDelete("{idItem}")]
        public async Task<ActionResult<Item>> DeleteItem(long id, long idItem)
        {
            if (_context.TodoList == null)
                return Problem("Entity set 'TodoContext.TodoList'  is null.");

            await _context.TodoList.Include(todoList => todoList.Items).ToListAsync();
            var list = await _context.TodoList.FindAsync(id);

            if (list == null) return NotFound();

            var item = list.Items.Where(x => x.Id.Equals(idItem)).SingleOrDefault();

            if (item == null) return NotFound();

            list.Items.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }

    }
}
