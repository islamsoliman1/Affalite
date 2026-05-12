using AffalitePL.Extensions;
using System.Text.Json.Serialization;

namespace AffalitePL;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddInfrastructure(builder.Configuration)
            .AddApplicationServices()
            .AddAutoMapperProfiles()
            .AddJwtAuthentication(builder.Configuration)
            .AddSwaggerWithAuth()
            .AddCorsPolicy()
            .AddHttpClient();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        var app = builder.Build();

        app.UseGlobalExceptionHandler();
        app.UseSwaggerInDevelopment(app.Environment);
        app.UseCustomCors();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.SeedIdentityData();
        app.Run();
    }
}
