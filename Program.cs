using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SuppliesManagement.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SuppliesManagementProjectContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SuppliesManagement")));
// Add services to the container.
builder.Services.AddRazorPages();
/*builder.Services.AddDbContext<SuppliesManagementDBContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("SuppliesManagement"));
});*/

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "dungnt";
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "dungnt";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
        });
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapGet("/", context =>
{
    context.Response.Redirect("/DanhSachHang");
    return Task.CompletedTask;
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseSession();

app.MapRazorPages();

app.Run();
