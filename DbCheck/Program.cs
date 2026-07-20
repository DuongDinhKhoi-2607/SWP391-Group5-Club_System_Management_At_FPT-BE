using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DbCheck
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ClubSystemDbContext>();
            optionsBuilder.UseNpgsql("Host=ep-square-fire-a17p725d.ap-southeast-1.aws.neon.tech;Database=ClubManagement;Username=ClubManagement_owner;Password=yI9d5DkXqQcb;SSL Mode=Require;Trust Server Certificate=true");
            
            using var context = new ClubSystemDbContext(optionsBuilder.Options);
            
            var user = await context.Users
                .Include(u => u.Userinformation)
                .ThenInclude(ui => ui.Student)
                .FirstOrDefaultAsync(u => u.Userid == 2); // Get a sample user
                
            if (user != null)
            {
                user.Userinformation.Avatar = "https://res.cloudinary.com/test/image/upload/v123/test.png";
                user.Userinformation.Infoupdatedat = DateTime.Now;
                
                try
                {
                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Update successful!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                    if (ex.InnerException != null) Console.WriteLine("INNER: " + ex.InnerException.Message);
                }
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }
    }
}
