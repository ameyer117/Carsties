using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(x =>
{
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);

        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.UsingRabbitMq(
        (context, configuration) =>
        {
            configuration.Host(
                "localhost",
                "/",
                h =>
                {
                    h.Username("rabbitmq");
                    h.Password("rabbitmq");
                }
            );

            configuration.ConfigureEndpoints(context);
        }
    );
});

builder.Services.AddDbContext<AuctionDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

DbInitializer.Initialize(app);

app.Run();
