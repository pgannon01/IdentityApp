using IdentityApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityApp.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // can assign roles to our Identity User, like an Admin role
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// Set up settings
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
});

builder.Services.AddAuthorization(options =>
{
    // Add authorization to every request, if you're not a registered user, you're not allowed to do anything
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<IAuthorizationHandler, InvoiceCreatorAuthorizationHandler>(); // Using addscoped because in our invoicehandler we're making use of EFC
builder.Services.AddSingleton<IAuthorizationHandler, InvoiceManagerAuthorizationHandler>(); // Don't need AddScoped here cause not using EFC (Entity Framework Core)
builder.Services.AddSingleton<IAuthorizationHandler, InvoiceAdminAuthorizationHandler>();

var app = builder.Build();

// Since we can't use DI, we have to make this to actually call the Initialize function and actually seed our db
using (var scope = app.Services.CreateScope())
{
    // Auto Migrate/Update DB on start/host
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    //make use of services
    var seedUserPass = builder.Configuration.GetValue<string>("SeedUserPass"); // Using this for secrets instead of storing important passwords locally
    await SeedData.Initialize(services, seedUserPass);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
