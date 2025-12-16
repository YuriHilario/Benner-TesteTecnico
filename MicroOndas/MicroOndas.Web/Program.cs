using MicroOndas.Application.Services;
using MicroOndas.Domain.Interfaces;
using MicroOndas.Infrastructure.Repositories;
using MicroOndas.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// =======================================================
// SERVICES
// =======================================================

// Razor Pages
builder.Services.AddRazorPages();

// -------------------------------------------------------
// CORE DO MICRO-ONDAS
// -------------------------------------------------------

// Serviço principal do micro-ondas
// Singleton porque mantém estado em memória
builder.Services.AddSingleton<MicroOndasService>();

// Timer (BackgroundService)
// Executa o processamento a cada 1 segundo
builder.Services.AddHostedService<MicroOndasTimerService>();

// SignalR (atualização em tempo real da UI)
builder.Services.AddSignalR();

// -------------------------------------------------------
// REPOSITÓRIOS — SQL SERVER (NÍVEL 3)
// -------------------------------------------------------

// Programas de aquecimento dinâmicos
builder.Services.AddScoped<IHeatingProgramRepository>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    return new SqlHeatingProgramRepository(connectionString!);
});

// Programas pré-definidos (memória / banco)
builder.Services.AddScoped<IPredefinedProgramRepository>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    return new SqlPredefinedProgramRepository(connectionString!);
});

// =======================================================
// BUILD
// =======================================================

var app = builder.Build();

// =======================================================
// MIDDLEWARE
// =======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// =======================================================
// ENDPOINTS
// =======================================================

app.MapRazorPages();

// Hub SignalR
app.MapHub<MicroOndasHub>("/microondashub");

app.Run();
