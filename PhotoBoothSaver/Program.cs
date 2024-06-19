using PhotoBoothSaver;
using PhotoBoothSaver.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFileLogging();
builder.Services.AddConsoleLogging();

builder.Services.AddSingleton<FlowService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
var group = app.MapGroup("/").DisableAntiforgery();
group.MapPost("/save_image", async (IFormFileCollection col, HttpContext ctx) =>
    {
        if (col.Any())
        {
            var file = col.First();

            string cat = ctx.Request.Query["cat"];
            string folder = ctx.Request.Query["folder"];
            int index = int.Parse(ctx.Request.Query["index"]);
            string sex = ctx.Request.Query["sex"];

            using var readStream = file.OpenReadStream();
            
            using var memStream = new MemoryStream();
            
            await readStream.CopyToAsync(memStream);
            
            var dto = new FormFileDto()
            {
                File = memStream.GetBuffer(),
                
                SavePath = $@"{folder}\{sex}\{cat}\{index}.webp"
            };

            app.Services.GetService<FlowService>()?.Queue(dto);
        }
    
        return Results.Ok();
    })
    .Accepts<IFormFileCollection>("multipart/form-data")
.Produces(200);

app.Run();
