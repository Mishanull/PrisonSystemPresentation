using System.Security.Claims;
using System.Text.Json;
using BlazorLoginApp.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Contracts;
using Entities;
using Microsoft.AspNetCore.Components.Authorization;
using RabbitMQClient;
using RabbitMqClients;
using Blazored.Modal;
using ConsumerBackgroundServices;

using Microsoft.AspNetCore.Mvc.RazorPages;
using RabbitMQClients;
using Syncfusion.Blazor;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSyncfusionBlazor(options => { options.IgnoreScriptIsolation = true; });
builder.Services.AddBlazoredModal();
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();
builder.Services.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>();

builder.Services.AddScoped<IUserService, UserClient >();
builder.Services.AddScoped<IPrisonerService, PrisonerClient >();
builder.Services.AddScoped<IGuardService, GuardClient >();
builder.Services.AddScoped<IAlertService, AlertClient>();
builder.Services.AddScoped<IVisitService, VisitClient>();
builder.Services.AddSingleton<StateContainer.StateContainer>();
builder.Services.AddSingleton<Consumer>();
builder.Services.AddHostedService(sp=>sp.GetService<Consumer>());

builder.Services.AddScoped<IWorkShiftService, WorkShiftClient >();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WardenAccess", pb => pb.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "warden"));
    options.AddPolicy("GuardAccess", pb => pb.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "guard"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();