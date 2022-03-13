using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EstudosAPI.Data;
using EstudosAPI.Models;

//população da memoria para ter manipulação de bd
namespace Backoffice.Controllers
{
    [Route("v1/data")]
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<dynamic>> Get([FromServices] DataContext context)
        {
            var employee = new UserModel { Id = 1, Username = "administrator", Password = "admin123", Role = "admin" };
            var manager = new UserModel { Id = 2, Username = "hardwork", Password = "ilovmyjob", Role = "employee" };
            var category = new CategoryModel { Id = 1, Title = "Hotbart Courses" };
            var product = new ProductModel { Id = 1, Category = category, Title = "How to sell courses", Price = 2999.99M, Description = "How to sell your course in hotbart!"};
            
            context.Users.Add(employee);
            context.Users.Add(manager);
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "Dados configurados"
            });
        }
    }
}