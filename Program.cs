using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SuppliesManagement.Models;

// using SuppliesManagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SuppliesManagementProjectContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("SuppliesManagement"))
);

/*builder.Services.AddDNTCaptcha(options =>
{
    options.UseCookieStorageProvider() // Lưu trạng thái CAPTCHA bằng cookies
           .ShowThousandsSeparators(false);
        //    .WithEncryptionKey("YourSecureKey123");
});*/

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".CaptchaSession";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services
    .AddAuthentication("CookieAuth")
    .AddCookie(
        "CookieAuth",
        options =>
        {
            options.LoginPath = "/Common/SignIn";
            options.AccessDeniedPath = "/Error/AccessDenied";
        }
    );
builder.Services.AddAntiforgery(options => options.HeaderName = "XSRF-TOKEN");

// builder.Services.AddScoped<ISignInService, SignInService>();
// builder.Services.AddScoped<RecallService>();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/PageNotFound");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Error/PageNotFound");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapGet(
    "/",
    context =>
    {
        context.Response.Redirect("/Common/SignIn");
        return Task.CompletedTask;
    }
);

// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapRazorPages();

//     foreach (var endpoint in endpoints.DataSources.SelectMany(ds => ds.Endpoints))
//     {
//         Console.WriteLine(endpoint.DisplayName);
//     }
// });
app.MapRazorPages();

app.Run();
