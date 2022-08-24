using LastGrind.Application.Authorization;
using LastGrind.Application.Interfaces;
using LastGrind.Persistance.Context;
using LastGrind.Persistance.Services;
using LastGrind.WebApi.Installers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtBearerInstaller(builder.Configuration);
builder.Services.AddSwaggerAndCustomJwtService();
//builder.Services.AddRepositoryImplementations();
builder.Services.AddDatabaseConnection(builder.Configuration);
builder.Services.AddDefaultIdentity<IdentityUser>(opt =>
{
    opt.User.RequireUniqueEmail = false;
    opt.Password.RequireDigit = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("MustHaveCompanyDomain", policy => policy.AddRequirements(new MustHaveCompanyDomainRequirement("gmail.com")));
    //opt.AddPolicy("TagViewer", builder => builder.RequireClaim("tags.view", "true"));
});
builder.Services.AddScoped<IIdentityService, IdentityService>();
//custom authorization
builder.Services.AddSingleton<IAuthorizationHandler, MustHaveCompanyDomainHandler>();

var app = builder.Build();
//await app.UseSeed();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//await app.UseSeed();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
