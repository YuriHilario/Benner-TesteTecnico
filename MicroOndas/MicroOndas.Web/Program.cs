using MicroOndas.Application.Services;
using MicroOndas.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// --- ADIÇÕES OBRIGATÓRIAS PARA O NÍVEL 1 ---

// 1. Injeção do Serviço de Aplicação (MicroOndasService)
// Registrado como Singleton para garantir que haja APENAS UMA instância
// gerenciando o estado do Micro-ondas (CurrentProgram) na memória.
builder.Services.AddSingleton<MicroOndasService>();

// 2. Injeção do Background Service (Timer)
// Responsável por executar o timer a cada segundo e chamar ProcessOneSecond().
builder.Services.AddHostedService<MicroOndasTimerService>();

// 3. Injeção do SignalR
// Necessário para permitir a comunicação em tempo real (push) para o navegador.
builder.Services.AddSignalR();

// ---------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// --- Mapeamento do SignalR ---

// 4. Mapeamento do Hub SignalR
// Cria um endpoint ('/microondashub') que o JavaScript do cliente irá se conectar.
app.MapHub<MicroOndasHub>("/microondashub");

// ------------------------------

app.Run();