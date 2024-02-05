using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Pulse.Timeline.Consumers;
using Pulse.WebApp.Client;
using Pulse.WebApp.Features.Posts.API;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Use Autofac as a DI Container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    var module = new ConfigurationModule(config);
    builder.RegisterModule(module);

    builder.AddPostEndpoints();
});

builder.Logging.AddJsonConsole();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder
    .Services.AddAuthentication(opt =>
    {
        opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = config["Authentication:Authority"];
        options.ClientId = config["Authentication:ClientId"];
        options.ClientSecret = config["Authentication:ClientSecret"];
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<PostCreatedConsumer>();

    cfg.UsingRabbitMq(
        (context, cfg) =>
        {
            cfg.Host(
                config["RabbitMQ:Host"],
                config["RabbitMQ:VirtualHost"],
                h =>
                {
                    h.Username(config["RabbitMQ:Username"]);
                    h.Password(config["RabbitMQ:Password"]);
                }
            );

            cfg.ConfigureEndpoints(context);
        }
    );
});

var app = builder.Build();

app.MapPostRoutes();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
