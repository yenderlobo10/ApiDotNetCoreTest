using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiAngularTest.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        //constructor =>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        //table-entities =>
        public DbSet<ToDo> ToDoes { get; set; }

    }
}
