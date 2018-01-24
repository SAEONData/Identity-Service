using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace SAEON.Identity.Service.Data
{
    public class SAEONUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
    }

    public class SAEONRole : IdentityRole<Guid> { }

    public class SAEONDbContext : IdentityDbContext<SAEONUser, SAEONRole, Guid>
    {
        public SAEONDbContext(DbContextOptions<SAEONDbContext> options) : base(options) { }
    }
}
