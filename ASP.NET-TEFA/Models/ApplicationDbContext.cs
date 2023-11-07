using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_TEFA.Models;
public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MsCustomer> MsCustomers { get; set; }

    public virtual DbSet<MsVehicle> MsVehicles { get; set; }

    public virtual DbSet<TrsBooking> TrsBookings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MsCustomer>(entity =>
        {
            entity.HasKey(e => e.IdCustomer);

            entity.ToTable("ms_customers");

            entity.Property(e => e.IdCustomer)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("id_customer");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("phone");
        });

        modelBuilder.Entity<MsVehicle>(entity =>
        {
            entity.HasKey(e => e.IdVehicle);

            entity.ToTable("ms_vehicles");

            entity.Property(e => e.IdVehicle)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("id_vehicle");
            entity.Property(e => e.ChassisNumber)
                .HasMaxLength(200)
                .IsFixedLength()
                .HasColumnName("chassis_number");
            entity.Property(e => e.Classify)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("classify");
            entity.Property(e => e.Color)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("color");
            entity.Property(e => e.IdCustomer)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("id_customer");
            entity.Property(e => e.MachineNumber)
                .HasMaxLength(200)
                .IsFixedLength()
                .HasColumnName("machine_number");
            entity.Property(e => e.PoliceNumber)
                .HasMaxLength(50)
                .IsFixedLength()
                .HasColumnName("police_number");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("type");
            entity.Property(e => e.VehicleOwner)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("vehicle_owner");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.MsVehicles)
                .HasForeignKey(d => d.IdCustomer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehicle_Customer");
        });

        modelBuilder.Entity<TrsBooking>(entity =>
        {
            entity.HasKey(e => e.IdBooking);

            entity.ToTable("trs_booking");

            entity.Property(e => e.IdBooking)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("id_booking");
            entity.Property(e => e.Complaint)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("complaint");
            entity.Property(e => e.IdVehicle)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("id_vehicle");
            entity.Property(e => e.Odometer).HasColumnName("odometer");
            entity.Property(e => e.OrderDate)
                .HasColumnType("date")
                .HasColumnName("order_date");

            entity.HasOne(d => d.IdVehicleNavigation).WithMany(p => p.TrsBookings)
                .HasForeignKey(d => d.IdVehicle)
                .HasConstraintName("FK_id_vehicle");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

