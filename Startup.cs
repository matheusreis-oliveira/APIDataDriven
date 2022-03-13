using System.Linq;
using System.Text;
using EstudosAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
            services.AddCors(); //removendo problemas de requisições localhost
            //compacta a informação e manda zipado para a tela, e o html descompacta a informação
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" }); //compactando tudo q é app/json
            });
            //services.AddResponseCaching(); //adiciona o cache na aplicação, porém posso utilizar direto no controller

            services.AddControllers();

            //transformando a chave em bytes
            var key = Encoding.ASCII.GetBytes(Settings.Key);
            //adicionando a autenticação => na maioria dos casos é usado esse padrao, somente quando estiver trabalhando com distribuição que muda
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false; //dizendo que não precisa do httpsmetadata
                x.SaveToken = true; //vai salvar o token
                x.TokenValidationParameters = new TokenValidationParameters //validando o token 
                {
                    ValidateIssuerSigningKey = true, //validar se ja tem uma chave
                    IssuerSigningKey = new SymmetricSecurityKey(key), //a chave que ele vai validar do front com o back (SymmetricSecurityKey aonde estamos passando os bytes de key)
                    //como nao estamos usando nada avançado, usamos os validete's como false
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });


            //Utilização do InMemoryDB para salvar em memoria ↓
             services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Banco")); 
            //Usando no DB ↓
            // services.AddDbContext<DataContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("connectionString")));

            //injeção de dependencia ↓
            //services.AddScoped<DataContext, DataContext>(); //services.AddDbContext ja faz a função do addScoped
            //AddScoped => sempre vai abrir apenas um datacontext por requisição, nunca vou ter 2 conexões abertas pq o addscoped destrói a conexão
            //AddTransient => sempre quando eu pedir um datacontext ele vai me abrir um novo datacontext (cria um novo na memoria)
            //AddSingleton => cria uma instancia do datacontext por aplicação, primeira vez que inciar a aplicação vai criar uma instancia e todas as requisiçoes vao utilizar o mesmo datacontext(nao da pra usar quando tem usario !=)

            //ferramenta para documentação da API (swashbuckle, swagger, openApi)
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Estudos API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger(); //permite que ja tenha uma especificação da api em um formato json
            //ferramenta visual para visualizar o api (padrão: c.SwaggerEndpoint("/swagger/v1/swagger.json", "NOME");)
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Estudos API v1");
            });


            app.UseRouting();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); //permite a chamada de origem, metodos e cabeçalhos

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}