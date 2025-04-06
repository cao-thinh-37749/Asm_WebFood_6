using Microsoft.AspNetCore.Identity;
using Models_Asm;
using System.Threading.Tasks;

namespace Web_food_Asm.Data
{
    public class DataSeeder
    {
        public static async Task SeedRolesAndUsers(RoleManager<IdentityRole> roleManager, UserManager<KhachHang> userManager)
        {
            // Tạo role nếu chưa có
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Tạo tài khoản Admin nếu chưa có
            string adminEmail = "admin@123.com";
            string adminPassword = "Admin@123";
            string hoTenAdmin = "admin";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new KhachHang { UserName = adminEmail, Email = adminEmail, HoTen = hoTenAdmin };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Tạo tài khoản Customer nếu chưa có
            string customerEmail = "customer@123.com";
            string customerPassword = "Customer@123";
            string hoTenCustomer = "customer";
            if (await userManager.FindByEmailAsync(customerEmail) == null)
            {
                var customer = new KhachHang { UserName = customerEmail, Email = customerEmail, HoTen = hoTenCustomer };
                var result = await userManager.CreateAsync(customer, customerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customer, "Customer");
                }
            }
        }
    }
}
