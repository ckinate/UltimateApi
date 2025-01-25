using Contracts;
using LoggerService;
using Repository;
using Service.Contracts;
using Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Marvin.Cache.Headers;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Entities.ConfigurationModels;
using Microsoft.OpenApi.Models;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("X-Pagination");
            });

            });
        }
        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options =>
            {

            });
        }
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();

        }
        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        }
        public static void ConfigureServiceManager(this IServiceCollection services)
        {
            services.AddScoped<IServiceManager, ServiceManager>();

        }
        public static void ConfigureSqlContext(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(opts =>opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));
        }
        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder)
        {
          return  builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

        }

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var systemTextJsonOutputFormatter = config.OutputFormatters
                .OfType<SystemTextJsonOutputFormatter>()?
                .FirstOrDefault();
                if (systemTextJsonOutputFormatter != null)
                {
                    systemTextJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+json");
                    systemTextJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.apiroot+json");

                }
                var xmlOutputFormatter = config.OutputFormatters
                .OfType<XmlDataContractSerializerOutputFormatter>()?
                .FirstOrDefault();
                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.apiroot+xml");

                }
            });
        }

        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(opt =>
            {
                opt.ReportApiVersions = true;
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ApiVersionReader = new HeaderApiVersionReader("api-version");
            }).AddMvc();
        }
        public static void ConfigureResponseCaching(this IServiceCollection services) => services.AddResponseCaching();
        public static void ConfigureHttpCacheHeaders(this IServiceCollection services) =>services.AddHttpCacheHeaders(
             (expirationOpt) =>
             {
                 expirationOpt.MaxAge = 65;
                 expirationOpt.CacheLocation = CacheLocation.Private;
             },
             (validationOpt) =>
             {
                 validationOpt.MustRevalidate = true;
             }

            );

        //   public static void ConfigureOutputCaching(this IServiceCollection services) => services.AddOutputCache();

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentity<User, IdentityRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 10;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<RepositoryContext>()
            .AddDefaultTokenProviders();
        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            //  To create an environment variable, we have to open the cmd window as an administrator and type the following command:
            // setx SECRET "CodeMazeSecretKey113211162023!!!!" /M 

           // var jwtSettings = configuration.GetSection("JwtSettings");
            var jwtConfiguration = new JwtConfiguration(); 
            configuration.Bind(jwtConfiguration.Section, jwtConfiguration);
          //  var secretKey = Environment.GetEnvironmentVariable("SECRET");
            var secretKey = jwtConfiguration.Secret;

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfiguration.ValidIssuer,
                    ValidAudience = jwtConfiguration.ValidAudience,

                    // ValidIssuer = jwtSettings["validIssuer"],
                    // ValidAudience = jwtSettings["validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        }
        public static void ConfigureSwagger(this IServiceCollection services)
        { 
            services.AddSwaggerGen(s =>
            { s.SwaggerDoc("v1", new OpenApiInfo 
              { Title = "Code Maze API",
                Version = "v1" ,
                Description = "CompanyEmployees API by CodeMaze",
                TermsOfService = new Uri("https://example.com/terms"),
                Contact = new OpenApiContact { Name = "John Doe", Email = "John.Doe@gmail.com", Url = new Uri("https://twitter.com/johndoe"), },
                License = new OpenApiLicense { Name = "CompanyEmployees API LICX", Url = new Uri("https://example.com/license"), }
            });
              s.SwaggerDoc("v2", new OpenApiInfo { Title = "Code Maze API", Version = "v2" });
                var xmlFile = $"{typeof(Presentation.AssemblyReference).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                s.IncludeXmlComments(xmlPath);
                //This is To add authorization support
                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                  In = ParameterLocation.Header,
                  Description = "Place to add JWT with Bearer",
                  Name = "Authorization",
                  Type = SecuritySchemeType.ApiKey,
                  Scheme = "Bearer"


              });
              s.AddSecurityRequirement(
                  new OpenApiSecurityRequirement()
                  {
                      {
                           new OpenApiSecurityScheme
                           {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,                  
                                    Id = "Bearer"
                                },
                                 Name = "Bearer",
                           },
                          new List<string>()
                      }
                  }
                  );
            });
        }

    }
}
