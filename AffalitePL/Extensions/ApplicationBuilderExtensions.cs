using AffalitePL.Middleware;
using AffalitePL.Seed;

namespace AffalitePL.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseSwaggerInDevelopment(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
    {
        app.UseCors("AllowAngular");
        return app;
    }

    public static IApplicationBuilder SeedIdentityData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        IdentitySeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        return app;
    }
}
