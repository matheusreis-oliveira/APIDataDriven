using EstudosAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EstudosAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Banco"));

            //injeção de dependencia ↓
            services.AddScoped<DataContext, DataContext>();
            //AddScoped => sempre vai abrir apenas um datacontext por requisição, nunca vou ter 2 conexões abertas pq o addscoped destrói a conexão
            //AddTransient => sempre quando eu pedir um datacontext ele vai me abrir um novo datacontext (cria um novo na memoria)
            //AddSingleton => cria uma instancia do datacontext por aplicação, primeira vez que inciar a aplicação vai criar uma instancia e todas as requisiçoes vao utilizar o mesmo datacontext(nao da pra usar quando tem usario !=)
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}