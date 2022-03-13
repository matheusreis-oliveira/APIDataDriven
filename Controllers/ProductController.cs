using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstudosAPI.Data;
using EstudosAPI.Models;
using System;
using Microsoft.AspNetCore.Authorization;

namespace EstudosAPI.Controllers
{
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductModel>>> GetAll([FromServices] DataContext context)
        {
            try
            {
                var products = await context
                .Products   //pego os produtos do datacontxt que estão atrelados ao productmodel
                .Include(pdc => pdc.Category) //Incluo as categorias quando busco os produtos (include é um JOIN)
                                              // ↑ se eu quisesse exibir soment o Id da categoria eu nao precisaria do include pois dentro do modelo temos a propriedade do Id
                .AsNoTracking()
                .ToListAsync();

                return products;
            }
            catch
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductModel>> GetById(int id, [FromServices] DataContext context)
        {
            try
            {
                var product = await context
                .Products
                .Include(ctg => ctg.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(ctg => ctg.Id == id);

                return product;
            }
            catch
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("categories/{id:int}")] //products/categories/1 => listar todos os products da category 1
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductModel>>> GetProducts(int id, [FromServices] DataContext context)
        {
            try
            {
                var products = await context.Products.Include(ctg => ctg.Category).AsNoTracking()
                .Where(ctg => ctg.CategoryId == id) //fazendo um filtro para buscar o CategoryId igual a id
                .ToListAsync();

                return products;
            }
            catch
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<ProductModel>> Post([FromBody] ProductModel model, [FromServices] DataContext context)
        {
            //ModelState = estado do modelo (no caso o productmodel)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<ProductModel>> Put(int id, [FromBody] ProductModel model, [FromServices] DataContext context)
        {
            //veriica se o id informado é o mesmo do model
            if (model.Id != id)
                return NotFound(new { produto = "Produto não encontrado" });

            //verifica se os dados validos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<ProductModel>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (DbUpdateConcurrencyException) //tipo especifico = erro de concorrencia
            {
                return BadRequest(new { produto = "Esse registro já foi atualizado" });
            }
            catch (Exception)
            {
                return BadRequest(new { produto = "Não foi possível atualizar o produto " });
            }
        }

        [HttpDelete]
        [Route("categories/{id:int}")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<ProductModel>> Delete(int id, [FromServices] DataContext context)
        {
            //cria um proxy do produto (EF)
            var product = await context.Products.FirstOrDefaultAsync(pdc => pdc.Category.Id == id);

            if (product == null)
                return NotFound(new { categoria = "Produto não encontrado" });


            try
            {
                //dentro do contexto, pego a categoria(categories) e removo pelo category(que estou pegando pelo id)
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return product;
            }
            catch (Exception)
            {
                return BadRequest(new { categoria = "Não foi possível remover o produto" });
            }
        }
    }
}
