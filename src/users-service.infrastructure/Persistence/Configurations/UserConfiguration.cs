using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using users_service.domain.ValueObjects;
using System.Data;
using users_service.infrastructure.Persistence.Models;

namespace users_service.infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserPostgres>
    {
        public void Configure(EntityTypeBuilder<UserPostgres> builder)
        {
            builder.HasKey(u => u.Id); // Clave primaria

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(30);
            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(30);
            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30);
            builder.Property(u => u.Address)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(u => u.Birthdate)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(u => u.RoleUser)
                .IsRequired()
                .HasMaxLength(50);

        }
    }
}
