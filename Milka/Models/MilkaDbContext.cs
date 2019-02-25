using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milka.Models
{
    
    public partial class MilkaDbContext : IdentityDbContext<AppUser>
    {
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Subscriber> Subscribers { get; set; }

        public MilkaDbContext(DbContextOptions<MilkaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscriber>()
                .HasAlternateKey(c => c.Email)
                .HasName("AlternateKey_Unique_Email");
            base.OnModelCreating(modelBuilder);
            
        }
        
        
        public static async Task CreateAdminAccount(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            UserManager<AppUser> userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string username = configuration["AdminUser:Name"];
            string email = configuration["AdminUser:Email"];
            string password = configuration["AdminUser:Password"];
            string role = configuration["AdminUser:Role"];

            if (await userManager.FindByNameAsync(username) == null)
            {
                AppUser user = new AppUser
                {
                    UserName = username,
                    Email = email
                };

                IdentityResult result = await userManager.CreateAsync(user, password);

            }
        }
    }
    
    [Table("Post")]
    public class Post
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Author { get; set; }

        [Required] 
        public DateTime Created { get; set; }
        
         
        public DateTime Edited { get; set; }

        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[] TimeStamp { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        public string PostText { get; set; }
        
        [StringLength(255)]
        public string Img1 { get; set; }
    }
    
}