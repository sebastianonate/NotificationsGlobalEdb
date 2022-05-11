using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Notifications.Infrastructure.Mails;
using Notifications.Infrastructure.Teams;
using Swashbuckle.Swagger;
using Microsoft.OpenApi.Models;
using Notifications.Infrastructure.Dataverse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Notifications.WebAPI.Services;
using Notifications.Infrastructure.Logs;
using Notifications.Infrastructure.BlobStorage;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using FluentValidation;
using Notifications.WebAPI.Filters;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notifications.Infrastructure.SystemNotifications;
using Notifications.Infrastructure.Sms;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager Configuration = builder.Configuration;
// Add services to the container.
var SmtpConfig = Configuration.GetSection("Smtp").Get<SmtpConfiguration>();
builder.Services.AddSingleton(SmtpConfig);
/*builder.Services.AddLogging(builder => {
    builder.AddNLog(Configuration);
});*/
builder.Logging.AddConsole();
builder.Services.AddTransient(typeof(LogService<>));
builder.Services.AddTransient<BlobStorageService>();
builder.Services.AddTransient<Microsoft.Extensions.Hosting.IHostedService, FetchNotificationsScheduledHostedService>();
builder.Services.AddTransient<Microsoft.Extensions.Hosting.IHostedService, SaveLogsInBlobStorageHostedService>();
builder.Services.AddTransient<Microsoft.Extensions.Hosting.IHostedService, DeleteOldLogFilesHostedService>();
//builder.Services.AddValidatorsFromAssemblyContaining<MailNotificationDataValidator>();
//Dataverse
builder.Services.AddSingleton<GenerateDataverseToken>();
builder.Services.AddScoped<DataverseService>();
builder.Services.AddScoped<FetchNotificationsScheduleService>();
builder.Services.AddScoped<FetchTemplateService>();
//Mail
builder.Services.AddScoped<MailSendGridService>();
builder.Services.AddScoped<MailSmtpService>();
builder.Services.AddScoped<ScheduledNotificationsMailService>();

//Teams
builder.Services.AddSingleton<GraphTokenGenerator>();
builder.Services.AddScoped<ScheduledNotificationsTeamsService>();
builder.Services.AddScoped<FlatFileService>();
builder.Services.AddScoped<TeamsService>();

//Sms
builder.Services.AddScoped<SmsService>();

//
builder.Services.AddScoped<SystemNotificationsService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

/*builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    //suprime el filtro por defecto q trae asp.net core para enviar los mensajes cuando el modelo no es valido
    options.SuppressModelStateInvalidFilter = true;
});*/
builder.Services.AddControllers(
    // options => options.Filters.Add<ValidatorFilter>()
     ).AddNewtonsoftJson().AddFluentValidation(fv => {
         fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
         fv.RegisterValidatorsFromAssemblyContaining<Program>();
     });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    // Include 'SecurityScheme' to use JWT Authentication
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

});

//Configure KeyVault
builder.Host.ConfigureAppConfiguration((context, config) =>
{
    var buildConfiguration = config.Build();
    string kvUrl = buildConfiguration["KeyVault:KvUrl"];
    string tenantId = buildConfiguration["KeyVault:TenantId"];
    string clientId = buildConfiguration["KeyVault:ClientId"];
    string clientSecret = buildConfiguration["KeyVault:ClientSecret"];

    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
    var client = new SecretClient(new Uri(kvUrl), credential);
    config.AddAzureKeyVault(client, new KeyVaultSecretManager());
});
var app = builder.Build();
// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/
app.UseSwagger();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notifications v1"));

app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

/*Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(builder.Environment.ContentRootPath+Path.DirectorySeparatorChar+"seba.txt")
    .CreateLogger();
Log.Information("hola");*/
app.Run();
