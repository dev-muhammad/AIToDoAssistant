using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToDoAssistant.Services;
using ToDoAssistant.Hubs;
using ToDoAssistant.Configuration;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add validation service to the container.
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddOptions<OpenAISettings>()
        .Bind(builder.Configuration.GetSection("OpenAI"))
        .ValidateDataAnnotations();

// Validate the configuration
var openAISettings = new OpenAISettings();
builder.Configuration.GetSection("OpenAI").Bind(openAISettings);

var context = new ValidationContext(openAISettings, serviceProvider: null, items: null);
var results = new List<ValidationResult>();

if (!Validator.TryValidateObject(openAISettings, context, results, true))
{
    var errors = string.Join(", ", results.Select(r => r.ErrorMessage));
    throw new Exception($"OpenAI configuration is invalid: {errors}");
}

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ToDoService>();
builder.Services.AddSingleton<FunctionCallService>();
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chathub"); // Map the SignalR hub

app.Run();
