using ASC.Model.BaseTypes;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        //public async Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<ApplicationSettings> options)
        //{
        //    // Get All comma-separated roles
        //    var roles = options.Value.Roles.Split(new char[] { ',' });
        //    // Create roles if they don't exist
        //    foreach (var role in roles)
        //    {
        //        try
        //        {
        //            if (!roleManager.RoleExistsAsync(role).Result)
        //            {
        //                IdentityRole storageRole = new IdentityRole
        //                {
        //                    Name = role
        //                };
        //                IdentityResult roleResult = await roleManager.CreateAsync(storageRole);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex);
        //        }
        //    }
        //    // Create admin if he doesn't exist
        //    var admin = await userManager.FindByEmailAsync(options.Value.AdminEmail);
        //    if (admin == null)
        //    {
        //        IdentityUser user = new IdentityUser
        //        {
        //            UserName = options.Value.AdminName,
        //            Email = options.Value.AdminEmail,
        //            EmailConfirmed = true
        //        };
        //        IdentityResult result = await userManager.CreateAsync(user, options.Value.AdminPassword);
        //        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.AdminEmail));
        //        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
        //        // Add Admin to Admin roles
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
        //        }
        //    }
        //    // Create a service engineer if he doesn't exist
        //    var engineer = await userManager.FindByEmailAsync(options.Value.EngineerEmail);
        //    if (engineer == null)
        //    {
        //        IdentityUser user = new IdentityUser
        //        {
        //            UserName = options.Value.EngineerName,
        //            Email = options.Value.EngineerEmail,
        //            EmailConfirmed = true,
        //            LockoutEnabled = true
        //        };
        //        IdentityResult result = await userManager.CreateAsync(user, options.Value.EngineerPassword);
        //        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.EngineerEmail));
        //        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
        //        // Add Service Engineer to Engineer role
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
        //        }
        //    }
        //}

        public async Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<ApplicationSettings> options)
        {
            var roles = options.Value.Roles.Split(new char[] { ',' });

            // Tạo roles nếu chưa có
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var storageRole = new IdentityRole { Name = role };
                    await roleManager.CreateAsync(storageRole);
                }
            }

            // Kiểm tra và tạo admin
            var admin = await userManager.FindByEmailAsync(options.Value.AdminEmail);
            if (admin == null)
            {
                var user = new IdentityUser
                {
                    UserName = options.Value.AdminName,
                    Email = options.Value.AdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, options.Value.AdminPassword);
                if (!result.Succeeded)
                {
                    Console.WriteLine($"❌ Failed to create user {options.Value.AdminEmail}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                else
                {
                    Console.WriteLine($"✅ Successfully created user {options.Value.AdminEmail}");
                }
                if (result.Succeeded)
                {
                    await userManager.UpdateAsync(user); // Đảm bảo user đã lưu vào database
                    //await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", options.Value.AdminEmail));
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.AdminEmail));
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
                    await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                }
                else
                {
                    Console.WriteLine($"Admin creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Kiểm tra và tạo engineer
            var engineer = await userManager.FindByEmailAsync(options.Value.EngineerEmail);
            if (engineer == null)
            {
                var user = new IdentityUser
                {
                    UserName = options.Value.EngineerName,
                    Email = options.Value.EngineerEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                var result = await userManager.CreateAsync(user, options.Value.EngineerPassword);
                if (result.Succeeded)
                {
                    await userManager.UpdateAsync(user);
                    //await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", options.Value.EngineerEmail));
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.EngineerEmail));
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
                    await userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
                }
                else
                {
                    Console.WriteLine($"Engineer creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
