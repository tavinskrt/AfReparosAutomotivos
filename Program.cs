using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Repositories;

var builder = WebApplication.CreateBuilder(args);

/// Adicionando cookie de autenticação
builder.Services.AddAuthentication("Identity.Login")
    .AddCookie("Identity.Login", options =>
    {
        options.LoginPath = "/Login/Index";
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

