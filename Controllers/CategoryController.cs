using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EstudosAPI.Data;
using EstudosAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstudosAPI.Controllers
{
    //Endpoint => url

    //http://localhost:5000/...ROTA
    //https://localhost:5001/...ROTA

    //https://meuapp.azurewebsites.net/...ROTA => quando vai pro azure
    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CategoryModel>>> Get([FromServices] DataContext context)
        {
            //AsNoTracking = faz uma leitura da forma mais rapida no banco (usar no que é leitura)
            //ToList = executa a query (é sempre no final)
            try
            {
                var categories = await context.Categories.AsNoTracking().ToListAsync();
                return Ok(categories);
            }
            catch (Exception)
            {
                return BadRequest(new { categorias = "Categorias não encontradas" });
            }
        }

        [HttpGet]
        [Route("{id:int}")]//usando o parametro:tipo eu crio uma restriçao na rota
        [AllowAnonymous]
        public async Task<ActionResult<CategoryModel>> GetById(int id, [FromServices] DataContext context)
        {
            try
            {
                var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(ctg => ctg.Id == id);
                return Ok(category);
            }
            catch (Exception)
            {
                return BadRequest(new { categoria = "Categoria não encontrada" });
            }
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<CategoryModel>> Post(
            [FromBody] CategoryModel model, //usa o model binding                  
            [FromServices] DataContext context //dizendo que o datacontext vem do serviço
            )
        {
            //ModelState = estado do modelo (no caso o CateoryModel)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Categories.Add(model); // salva na memoria
                await context.SaveChangesAsync(); // salvo no banco
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<CategoryModel>> Put(int id, [FromBody] CategoryModel model, [FromServices] DataContext context)
        {
            //veriica se o id informado é o mesmo do model
            if (model.Id != id)
                return NotFound(new { categoria = "Categoria não encontrada" });

            //verifica se os dados validos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                //tipo da entrada no modelo, entao eu digo que o estado do modelo está modificado
                //        ↑entry     ↑categorymodel              ↑.state               ↑entitysatate.modifed  
                context.Entry<CategoryModel>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (DbUpdateConcurrencyException) //tipo especifico = erro de concorrencia
            {
                return BadRequest(new { categoria = "Esse registro já foi atualizado" });
            }
            catch (Exception)
            {
                return BadRequest(new { categoria = "Não foi possível atualizar a categoria" });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<CategoryModel>> Delete(int id, [FromServices] DataContext context)
        {
            //cria um proxy da categoria (EF)
            var category = await context.Categories.FirstOrDefaultAsync(ctg => ctg.Id == id);

            if (category == null)
                return NotFound(new { categoria = "Categoria não encontrada" });


            try
            {
                //dentro do contexto, pego a categoria(categories) e removo pelo category(que estou pegando pelo id)
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return category;
            }
            catch (Exception)
            {
                return BadRequest(new { categoria = "Não foi possível remover a cateoria" });
            }
        }
    }
}