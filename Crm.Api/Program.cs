using System.Text;
using Crm.Api.Application.Messaging;
using Crm.Api.Features.Authentication;
using Crm.Domain.Entities;
using Crm.Infrastucture;
using Crm.Infrastucture.Services;
using Crm.Infrastucture.Settings;
using Crm.Presentation.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var cs = builder.Configuration.GetConnectionString("defaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(cs));
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<Login.CommandHandler>();
builder.Services.AddScoped<Register.CommandHandler>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<TokenProvider>();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseAuthentication();                                                                                                                                                   
app.UseAuthorization();
app.UseEndpoints();

app.Run();

