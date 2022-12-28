using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Microsoft.EntityFrameworkCore.InMemory
//Criar banco de dados em memória
builder.Services.AddDbContext<AppDbContext>(options => 
options.UseInMemoryDatabase("TarefaDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapGet("/", () => "Return");
//Metodo Get que retorna frases aleatórias
//app.MapGet("frases_aleatorias", async () => 
//await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));

app.MapGet("/tarefas", async (AppDbContext db) =>
{
    return await db.Tarefas.ToListAsync();

});

app.MapGet("/tarefas/{id}", async (AppDbContext db, int id) =>

{
    var tarefaId = await db.Tarefas.FirstOrDefaultAsync(tarefa => tarefa.Id == id);
    if(tarefaId != null && tarefaId.Id == id)
    {
        return Results.Ok(tarefaId);
    }
    else
    {
        return Results.NotFound($"Tarefa {id} não encontrada");
    }
 
    //outra possibilidade de retorno
    //await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());
});

app.MapGet("/tarefas/concluida", async (AppDbContext db) =>
{
    return await db.Tarefas.Where(tarefa => tarefa.IsConcluida).ToListAsync();
});

app.MapGet("/tarefas/Nao-concluida", async (AppDbContext db) =>
{
    return  await db.Tarefas.Where(tarefa => tarefa.IsConcluida == false).ToListAsync();
});

app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    if(tarefa == null)
    {
        return Results.BadRequest();
    }
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefa/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefa/{id}", async (Tarefa tarefa, AppDbContext db, int id) =>
{
    var tarefaLocalizada = await db.Tarefas.FindAsync(id);

    if(tarefaLocalizada != null)
    {
        tarefaLocalizada.Nome = tarefa.Nome;
        tarefaLocalizada.IsConcluida = tarefa.IsConcluida;

        await db.SaveChangesAsync();

        return Results.Ok(tarefaLocalizada);
    }
    else
    {
        return Results.NotFound($"Tarefa {id} não encontrada");
    }
});

app.MapDelete("/tarefa/{id}", async (int id, AppDbContext db) =>
{
    if(await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }

    return Results.NotFound($"Tarefa {id} não encontrada");
});

app.Run();

class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConcluida { get; set; }
}

class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}   

