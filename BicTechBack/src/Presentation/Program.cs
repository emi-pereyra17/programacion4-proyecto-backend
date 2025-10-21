using BicTechBack.src.Core.Interfaces;
using BicTechBack.src.Core.Services;
using Infrastructure.Config;   // donde estará StripeOptions (puedes cambiar el namespace)
using BicTechBack.src.Infrastructure.Data;
using BicTechBack.src.Infrastructure.Repositories;
using Infrastructure.Services; // donde estará StripeService
using Infrastructure.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
// 👇 agregados para Stripe
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.Text;
using Application.Interfaces;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BicTechBack API",
        Version = "v1",
        Description = "API para gestión de productos, categorías, marcas, carritos y pedidos.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Soporte BicTech",
            Email = "soporte@bictech.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa 'Bearer' seguido de tu token JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPedidoDetalleRepository, PedidoDetalleRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IMarcaRepository, MarcaRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaMarcaRepository, CategoriaMarcaRepository>();
builder.Services.AddScoped<ICarritoRepository, CarritoRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPaisRepository, PaisRepository>();
builder.Services.AddScoped<IPaisService, PaisService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IMarcaService, MarcaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICategoriaMarcaService, CategoriaMarcaService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 🔐 Configuración JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
var key = jwtKey;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});


// ===================================================
// 🧩 CONFIGURACIÓN STRIPE (HttpClientFactory + Polly)
// ===================================================
builder.Services.Configure<StripeOptions>(opts =>
{
    opts.BaseUrl = builder.Configuration["Stripe:BaseUrl"];
    opts.SecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRETKEY") 
                     ?? builder.Configuration["Stripe:SecretKey"];
});


builder.Services.AddHttpClient("Stripe", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<StripeOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BicTechBack/1.0");
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.SecretKey);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// Registro del servicio que usará ese cliente
builder.Services.AddScoped<IStripeService, StripeService>();

// Métodos estáticos locales para Polly
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
// ===================================================


var app = builder.Build();

// ✅ Mostrar Swagger en todos los entornos (incluyendo Render)
app.UseSwagger();
app.UseSwaggerUI();


app.UseCors("AllowAll");

app.UseMiddleware<BicTechBack.src.API.Extensions.ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ Redirigir la raíz "/" hacia Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// ✅ Puerto dinámico para Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();

