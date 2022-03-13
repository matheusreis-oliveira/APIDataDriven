using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstudosAPI.Data;
using EstudosAPI.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using EstudosAPI.Services;

namespace EstudosAPI.Controllers
{
    [Route("v1/user")]
    public class UserController : Controller
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<UserModel>>> GetAll([FromServices] DataContext context)
        {
            try
            {
                var users = await context
                    .Users
                    .AsNoTracking()
                    .ToListAsync();
                return Ok(users);
            }
            catch
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<UserModel>> GetById(int id, [FromServices] DataContext context)
        {
            try
            {
                var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(usr => usr.Id == id);
                return user;
            }
            catch
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [Route("")]
        //[AllowAnonymous]
        [Authorize(Roles = "admin")] //porem é necessario ja ter um admin no banco
        public async Task<ActionResult<UserModel>> Post([FromServices] DataContext context, [FromBody] UserModel model)
        {
            //verifica se os dados são válidos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                //força o usuário a ser sempre employee
                model.Role = "employee";

                context.Users.Add(model);
                await context.SaveChangesAsync();

                //esconde a senha
                model.Password = "";
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { erro = "Não foi possível criar o usuário" });

            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<UserModel>> Put(int id, [FromServices] DataContext context, [FromBody] UserModel model)
        {
            //verifica se os dados sso validos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //verifica se o id informado é o mesmo do modelo
            if (id != model.Id)
                return NotFound(new { erro = "Usuário não encontrado" });

            try
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { erro = "Não foi possível alterar o usuário" });

            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<UserModel>> Delete(int id, [FromServices] DataContext context)
        {
            //cria um proxy do produto (EF)
            var user = await context.Users.FirstOrDefaultAsync(pdc => pdc.Id == id);

            if (user == null)
                return NotFound(new { erro = "Usuário não encontrado" });


            try
            {
                //dentro do contexto, pego a categoria(categories) e removo pelo category(que estou pegando pelo id)
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return user;
            }
            catch (Exception)
            {
                return BadRequest(new { erro = "Não foi possível remover o usuário" });
            }
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate([FromServices] DataContext context, [FromBody] UserModel model)
        {
            var user = await context.Users.AsNoTracking()
            .Where(usr => usr.Id == model.Id && usr.Password == model.Password).FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { erro = "Usuário ou senha inválidos" });

            var token = TokenService.GenerateToken(user);

            //esconde a senha
            //user.Password = "";
            return new
            {
                token = token
            };
        }


    }
}


