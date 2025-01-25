using CompanyEmployees.Extensions;
using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Utility;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;
using Service.DataShaping;
using Shared.DataTransferObjects;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config.txt"));

//LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),
//"/nlog.config.txt"));


builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureVersioning();
builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeaders();


builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();


//use to configures support for JSON Patch using Newtonsoft.Json while leaving the other formatters unchanged
NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() => new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson().Services.BuildServiceProvider().GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters.OfType<NewtonsoftJsonPatchInputFormatter>().First();
// builder.Services.AddControllers().AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);
//this is configure to accept xmlFormat
builder.Services.AddControllers(config =>
{
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
}).AddXmlDataContractSerializerFormatters().AddCustomCSVFormatter().AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);

builder.Services.AddCustomMediaTypes();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();





//this is use to suppress a default model state validation that is implemented due to the existence of the [ApiController] attribute in all API controllers
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigureSwagger();



//builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);
if (app.Environment.IsProduction())
{
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});
app.UseCors("CorsPolicy");
app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(s =>
{ 
    s.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Maze API v1");
    s.SwaggerEndpoint("/swagger/v2/swagger.json", "Code Maze API v2");
});

app.MapControllers();

app.Run();
