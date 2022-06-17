using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityApp.Authorization;

namespace IdentityApp.Data
{
    public class SeedData
    {
        // Initialize called at app start to create an admin account
        public static async Task Initialize(IServiceProvider serviceProvider, string password)
        {
            // Add our user to the DB, don't use DI. Since we can't use DI, use ServiceProvider
            using(var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // manager
                var managerUid = await EnsureUser(serviceProvider, "manager@demo.com", password);
                await EnsureRole(serviceProvider, managerUid, Constants.InvoiceManagersRole);

                // admin
                var adminUid = await EnsureUser(serviceProvider, "admin@demo.com", password);
                await EnsureRole(serviceProvider, adminUid, Constants.InvoiceAdminRole);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider, string userName, string initPw) // Creates a new user from scratch
        {
            // This will create a new account and seed the data for it
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>(); // Seeds every time our application starts

            // Create a new account for managers, but make sure it's not already existing
            var user = await userManager.FindByNameAsync(userName);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = userName,
                    Email = userName,
                    EmailConfirmed = true, // Won't be able to log in without this
                };

                var result = await userManager.CreateAsync(user, initPw);
            }

            if (user == null)
                throw new Exception("User did not get created. Password policy problem?");

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            IdentityResult ir; 

            if(await roleManager.RoleExistsAsync(role) == false) // If this specific role does not exist, create that role
            {
                ir = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByIdAsync(uid);

            if(user == null)
            {
                throw new Exception("User not existing");
            }

            // assign the user to the role
            ir = await userManager.AddToRoleAsync(user, role); // Adds specific user to specific role

            return ir;
        }
    }
}
