using TodoApi.Controllers;
using TodoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TodoApi.Tests;

#nullable disable
public class TodoListsControllerTests
{
    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoList.Add(new Models.TodoList { Id = 1, Name = "Task 1" });
        context.TodoList.Add(new Models.TodoList { Id = 2, Name = "Task 2" });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTodoList_WhenCalled_ReturnsTodoListList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoListsController(context);

            var result = await controller.GetTodoLists();

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(
              2,
              ((result.Result as OkObjectResult).Value as IList<TodoList>).Count
            );
        }
    }

    [Fact]
    public async Task GetTodoList_WhenCalled_ReturnsTodoListById()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoListsController(context);

            var result = await controller.GetTodoList(1);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(
              1,
              ((result.Result as OkObjectResult).Value as TodoList).Id
            );
        }
    }

    [Fact]
    public async Task PutTodoList_WhenTodoListIdDoesntMatch_ReturnsBadRequest()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoListsController(context);

            var todoList = await context.TodoList.Where(x => x.Id == 2).FirstAsync();
            var result = await controller.PutTodoList(1, todoList);

            Assert.IsType<BadRequestResult>(result);
        }
    }

    [Fact]
    public async Task PutTodoList_WhenTodoListDoesntExist_ReturnsBadRequest()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoListsController(context);

            var result = await controller.PutTodoList(3, new TodoList { Id = 3 });

            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    public async Task PutTodoList_WhenCalled_UpdatesTheTodoList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoListsController(context);

            var todoList = await context.TodoList.Where(x => x.Id == 2).FirstAsync();
            var result = await controller.PutTodoList(todoList.Id, todoList);

            Assert.IsType<NoContentResult>(result);
        }
    }

    [Fact]
    public async Task PostTodoList_WhenCalled_CreatesTodoList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoListsController(context);

            var todoList = new TodoList { Name = "Task 3" };
            var result = await controller.PostTodoList(todoList);

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(
              3,
              context.TodoList.Count()
            );
        }
    }

    [Fact]
    public async Task DeleteTodoList_WhenCalled_RemovesTodoList()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContext(context);

            var controller = new TodoListsController(context);

            var result = await controller.DeleteTodoList(2);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(
              1,
              context.TodoList.Count()
            );
        }
    }

    #region ItemsTest
    private void PopulateDatabaseContextWithItems(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "Task 1", Items = { new Item("FisrtItem"), new Item("SecondItem") } });
        context.TodoList.Add(new TodoList { Id = 2, Name = "Task 2", Items = { new Item("FisrtItem") } });
        context.SaveChanges();
    }


    #region ItemsGetTest
    [Fact]
    public async Task GetItems_WhenCalled_GetItems()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContextWithItems(context);

            var controller = new ItemController(context);

            var result = await controller.GetItems(1);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(
              2,
              ((result.Result as OkObjectResult).Value as IList<Item>).Count
            );
        }
    }
    [Fact]
    public async Task GetItems_WhenCalled_GetItemById()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContextWithItems(context);

            var controller = new ItemController(context);

            var result = await controller.GetItemById(1, 1);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(
              1,
               ((result.Result as OkObjectResult).Value as Item).Id
            );
        }
    }

    #endregion

    #region ItemsPostTest
    [Fact]
    public async Task InsertItems_WhenCalled_PostItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContextWithItems(context);

            var controller = new ItemController(context);
            Item item = new Item("NewItem");
            var result = await controller.PostItem(2, item);

            await context.TodoList.Include(todoList => todoList.Items).ToListAsync();
            var list = await context.TodoList.FindAsync((long)2);
            var expected = list.Items.Count();

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(
              2,
              expected
            );
        }
    }

    #endregion

    #region ItemsPutTest
    [Fact]
    public async Task PutItems_WhenIdsMatch_UpdateItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContextWithItems(context);

            var controller = new ItemController(context);

            Item item = new Item("NameChange");
            var result = await controller.UpdateItem(1, 1, item);

            await context.TodoList.Include(todoList => todoList.Items).ToListAsync();

            var list = await context.TodoList.FindAsync((long)1);
            var value = list.Items.Where(x => x.Id.Equals(1)).SingleOrDefault().Name;

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(
              "NameChange",
              value
            );
        }
    }

    [Fact]
    public async Task PutItems_WhenIdNotExist_ReturnNotFound()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContextWithItems(context);

            var controller = new ItemController(context);

            Item item = new Item("NameChange");
            var result = await controller.UpdateItem(0, 0, item);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
    #endregion

    #region ItemsDeleteTest
    [Fact]
    public async Task DeleteItem_WhenCalled_DeleteItem()
    {
        using (var context = new TodoContext(DatabaseContextOptions()))
        {
            PopulateDatabaseContextWithItems(context);

            var controller = new ItemController(context);
            var result = await controller.DeleteItem(1, 1);

            await context.TodoList.Include(todoList => todoList.Items).ToListAsync();
            var list = await context.TodoList.FindAsync((long)1);
            var value = list.Items.Count();

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(
              1,
             value
            );
        }
    }
    #endregion

    #endregion
}