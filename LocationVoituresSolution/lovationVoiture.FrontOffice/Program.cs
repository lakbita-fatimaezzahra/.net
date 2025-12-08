using LocationVoitures.FrontOffice.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ajouter LocationContextCore avec SQL Server
builder.Services.AddDbContext<LocationContextCore>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));

// Services MVC
builder.Services.AddControllersWithViews();

// Sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Test de connexion au démarrage
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LocationContextCore>();
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"? Connexion BD : {(canConnect ? "OK" : "ÉCHEC")}");

        if (canConnect)
        {
            var voituresCount = await context.Voitures.CountAsync();
            Console.WriteLine($"?? Voitures trouvées : {voituresCount}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Erreur connexion BD : {ex.Message}");
    }
}

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();