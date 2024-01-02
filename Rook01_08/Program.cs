using Microsoft.EntityFrameworkCore;
using Rook01_08.Data.EF;
using Rook01_08.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Identity store
builder.Services.AddDbContext<ApplicationDBContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    //app.UseExceptionHandler("/Error");
    app.UseCustomExceptionHandlingMiddleware();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
