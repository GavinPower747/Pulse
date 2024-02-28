using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Pulse.Followers;
using Pulse.Shared.Auth;
using Pulse.Timeline;
using Pulse.WebApp.Client;
using Pulse.WebApp.Configuration;
using Pulse.WebApp.Features.Posts.API;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Use Autofac as a DI Container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(autofac =>
{
    var module = new ConfigurationModule(config);
    autofac.RegisterModule(module);
});

builder.Logging.AddJsonConsole();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IdentityProvider>();

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

builder.Services.AddMediatR(cfg =>
{
    var autofacModuleAssemblies = builder
        .Configuration.GetSection("modules")
        .Get<List<ModuleInfo>>();

    var moduleAssemblies = autofacModuleAssemblies!
        .Select(m => Type.GetType(m.Type)?.Assembly)
        .ToArray();

    cfg.RegisterServicesFromAssemblies(moduleAssemblies!);
});

builder.Services.AddMassTransit(cfg =>
{
    var autofacModuleAssemblies = builder
        .Configuration.GetSection("modules")
        .Get<List<ModuleInfo>>();

    var moduleAssemblies = autofacModuleAssemblies!
        .Select(m => Type.GetType(m.Type)?.Assembly)
        .ToList();

    var consumerTypes = moduleAssemblies
        .SelectMany(a => a!.GetTypes())
        .Where(t =>
            t.IsClass
            && !t.IsAbstract
            && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
        )
        .ToList();

    foreach (var type in consumerTypes)
    {
        cfg.AddConsumer(type);
    }

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
app.MapFollowerRoutes();
app.MapTimelineRoutes();

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
