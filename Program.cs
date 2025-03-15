using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using OAuthSample.Configuration;
using OAuthSample.Services;
using OAuthSample.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
ConfigureMiddleware(app);

// Configure endpoints
ConfigureEndpoints(app);

app.Run();

// Service configuration
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Add controllers and API explorer
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    
    // Configure Swagger
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "OAuth Sample API",
            Version = "v1",
            Description = "A sample API with JWT authentication"
        });

        // Add JWT Authentication
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Configure JWT authentication
    var jwtConfig = configuration.GetSection("JwtConfig").Get<JwtConfig>();
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig?.Issuer,
                ValidAudience = jwtConfig?.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig?.Secret ?? string.Empty)),
                NameClaimType = ClaimTypes.Name
            };

            // Extract the JWT token from the cookie
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["jwt"];
                    return Task.CompletedTask;
                }
            };
        });

    // Register application services
    services.AddSingleton<TokenRepository>();
    services.AddSingleton<ContactRepository>();
    services.AddSingleton<UserRepository>();
    services.AddSingleton<RoleRepository>();
    services.AddSingleton<PermissionRepository>();
    services.AddScoped<AuthService>();
    services.AddScoped<PermissionService>();
}

// Middleware configuration
void ConfigureMiddleware(WebApplication app)
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// Endpoint configuration
void ConfigureEndpoints(WebApplication app)
{
    app.MapControllers();
    
    // Redirect root to index.html
    app.MapGet("/", () => Results.Redirect("/index.html"));
    
    // Add default admin role to admin user with all permissions during startup
    using (var scope = app.Services.CreateScope())
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<RoleRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<PermissionRepository>();
        
        // Get the admin user
        var adminUser = userRepository.GetUserByUsername("admin");
        if (adminUser != null)
        {
            // Get the admin role
            var adminRole = roleRepository.GetRoleByName("Administrator");
            if (adminRole != null)
            {
                // Assign admin role to admin user
                roleRepository.AssignRoleToUser(adminUser.Id, adminRole.Id);
                
                // Assign all permissions to admin role
                var allPermissions = permissionRepository.GetAllPermissions();
                foreach (var permission in allPermissions)
                {
                    permissionRepository.AssignPermissionToRole(adminRole.Id, permission.Id);
                }
            }
        }
        
        // Get the regular user
        var regularUser = userRepository.GetUserByUsername("user");
        if (regularUser != null)
        {
            // Get the user role
            var userRole = roleRepository.GetRoleByName("User");
            if (userRole != null)
            {
                // Assign user role to regular user
                roleRepository.AssignRoleToUser(regularUser.Id, userRole.Id);
                
                // Assign basic permissions to user role (e.g., viewing contacts)
                var permissionViewContacts = permissionRepository.GetPermissionByName("contacts.view");
                if (permissionViewContacts != null)
                {
                    permissionRepository.AssignPermissionToRole(userRole.Id, permissionViewContacts.Id);
                }
            }
        }
    }
}

// Weather forecast demo endpoint (can be removed in production)
// ... existing code ...
