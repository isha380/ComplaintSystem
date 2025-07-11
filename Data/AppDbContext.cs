using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserApp.Models;
using Complain.Models; 
namespace UserApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<LoginModel> Users { get; set; }
    

   
}
}