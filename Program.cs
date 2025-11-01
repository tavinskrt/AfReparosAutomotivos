using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Repositories;

var builder = WebApplication.CreateBuilder(args);

/// Configurando autenticação por cookies com Identity.Login, permitindo o uso do SignInAsync e SignOutAsync.
builder.Services.AddAuthentication("Identity.Login")
    .AddCookie("Identity.Login", options =>
    {
        /// Definindo o caminho para a página de login caso o usuário não esteja autenticado.
        options.LoginPath = "/Login/Index";

        /// Definindo o tempo de expiração do cookie de autenticação.
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

/// Implementando interface de Login
builder.Services.AddScoped<ILoginRepository, LoginRepository>();

/// Implementando inferface de Orçamentos
builder.Services.AddScoped<IOrcamentoRepository, OrcamentoRepository>(); 

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

/// Utilizando autenticação
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
