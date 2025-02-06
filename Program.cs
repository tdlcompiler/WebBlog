using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using WebBlog.Repository;
using WebBlog.Repository.UserRepository;
using WebBlog.Services;

namespace WebBlog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddDbContext<BlogDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IPostRepository, PostgresPostRepository>();
            builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();

            string dbType = builder.Configuration["Database"] ?? "postgres";
            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (arg.StartsWith("database="))
                    {
                        dbType = arg.Split('=')[1];
                        break;
                    }
                }
            }

            if (dbType.Equals("file", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("Используется файловая БД (FileRepository)");
                builder.Services.AddSingleton(provider =>
                    new AuthService("data/users.json", provider.GetRequiredService<IConfiguration>()));
                builder.Services.AddSingleton(provider =>
                    new PostService("data/posts.json"));
            }
            else
            {
                Console.WriteLine("Используется PostgreSQL БД (PostgresRepository)");
                builder.Services.AddScoped<AuthService>();
                builder.Services.AddScoped<PostService>();
            }
            
            builder.Services.AddSingleton(provider =>
                new ImageService("data/images/"));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerFileOperationFilter>();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Укажите JWT токен в формате: Bearer ваш_токен"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebBlog API",
                    Version = "v1"
                });
            });

            var jwtKey = builder.Configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Secret not found in configuration.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                db.Database.Migrate();
            }

            if (args.Contains("--migrate"))
            {
                Console.WriteLine("Migrations applied successfully. Exiting...");
                return;
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
