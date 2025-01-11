using CompanyEmployees.Extensions;
using CompanyEmployees.Presentation.ActionFilters;
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

builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();

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


//this is use to suppress a default model state validation that is implemented due to the existence of the [ApiController] attribute in all API controllers
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

builder.Services.AddAutoMapper(typeof(Program));


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

app.UseAuthorization();

app.MapControllers();

app.Run();
