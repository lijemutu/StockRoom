using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using StockRoomBlazor;
using StockRoomBlazor.Data;

var builder = WebApplication.CreateBuilder(args);

var Configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

//Google Authentication service         
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "StockRoom.Cookie";
    options.LoginPath = "/login";
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = Configuration["web:client_id"];
    googleOptions.ClientSecret = Configuration["web:client_secret"];
    googleOptions.CallbackPath = "/signin-google";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LoggedIn", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddSingleton<RabbitMQServiceProducer>();
builder.Services.AddHostedService<RabbitMQConsumer>();
//builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.MapBlazorHub();
//app.MapFallbackToPage("/_Host");
app.UseEndpoints(endpoints =>
{
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
    endpoints.MapHub<StockRoomSampleHub>(StockRoomSampleHub.HubUrl);
});
app.Run();
