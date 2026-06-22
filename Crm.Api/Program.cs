using System.Text;
using Crm.Api.Application.Messaging;
using Crm.Api.Application.Sorting;
using Crm.Api.Features.Contact;
using Crm.Api.Features.user;
using Crm.Domain.Entities;
using Crm.Infrastucture;
using Crm.Infrastucture.Services;
using Crm.Infrastucture.Settings;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title = "CRM API";
        doc.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        doc.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
        doc.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization"
        };
        return Task.CompletedTask;
    });
});

var cs = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(cs));
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddTransient<SortMappingProvider>();
builder.Services.AddSingleton<ISortMappingDefinition>(GetUsers.Mappings.SortMapping);
builder.Services.AddSingleton<ISortMappingDefinition>(GetContacts.Mappings.SortMapping);
  builder.Services.Scan(scan => scan                                                                                                                                                                                                 
      .FromAssemblyOf<Program>()                                                                                                                                                                                                     
      .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))                                                                                                                                                                     
      .AsImplementedInterfaces()                                                                                                                                                                                                     
      .WithScopedLifetime()                                                                                                                                                                                                          
      .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))                                                                                                                                                                   
      .AsImplementedInterfaces()                                                                                                                                                                                                     
      .WithScopedLifetime()                                                                                                                                                                                                          
      .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))                                                                                                                                            
      .AsImplementedInterfaces()                                                                                                                                                                                                     
      .WithScopedLifetime());     
builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Authentication = new ScalarAuthenticationOptions
    {
        PreferredSecurityScheme = "Bearer"
    };
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints();

app.Run();
