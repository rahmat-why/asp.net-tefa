using System;
using System.Collections.Generic;
using ASP.NET_TEFA.Models;
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

    public virtual DbSet<MsEquipment> MsEquipments { get; set; }

    public virtual DbSet<MsUser> MsUsers { get; set; }

    public virtual DbSet<MsVehicle> MsVehicles { get; set; }

    public virtual DbSet<TrsBooking> TrsBookings { get; set; }

    public virtual DbSet<TrsInspectionList> TrsInspectionLists { get; set; }

    public virtual DbSet<TrsPending> TrsPendings { get; set; }

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
                .IsUnicode(false)
                .HasColumnName("id_customer");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<MsEquipment>(entity =>
        {
            entity.HasKey(e => e.IdEquipment).HasName("PK__ms_equip__D745C9DF7C91B91A");

            entity.ToTable("ms_equipments");

            entity.Property(e => e.IdEquipment)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_equipment");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MsUser>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__ms_users__D2D146377E3C63CA");

            entity.ToTable("ms_users");

            entity.HasIndex(e => e.Nim, "UQ_nim").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_username").IsUnique();

            entity.Property(e => e.IdUser)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_user");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.Nidn)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nidn");
            entity.Property(e => e.Nim)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nim");
            entity.Property(e => e.Password)
                .HasColumnType("text")
                .HasColumnName("password");
            entity.Property(e => e.Position)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("position");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<MsVehicle>(entity =>
        {
            entity.HasKey(e => e.IdVehicle);

            entity.ToTable("ms_vehicles");

            entity.HasIndex(e => e.PoliceNumber, "UQ_police_number").IsUnique();

            entity.Property(e => e.IdVehicle)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_vehicle");
            entity.Property(e => e.ChassisNumber)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("chassis_number");
            entity.Property(e => e.Classify)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("classify");
            entity.Property(e => e.Color)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("color");
            entity.Property(e => e.IdCustomer)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_customer");
            entity.Property(e => e.MachineNumber)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("machine_number");
            entity.Property(e => e.PoliceNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("police_number");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.VehicleOwner)
                .HasMaxLength(100)
                .IsUnicode(false)
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
                .IsUnicode(false)
                .HasColumnName("id_booking");
            entity.Property(e => e.AdditionalPrice)
                .HasColumnType("money")
                .HasColumnName("additional_price");
            entity.Property(e => e.AdditionalReplacementPart)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("additional_replacement_part");
            entity.Property(e => e.Complaint)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("complaint");
            entity.Property(e => e.Control).HasColumnName("control");
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_time");
            entity.Property(e => e.Decision).HasColumnName("decision");
            entity.Property(e => e.EndRepairTime)
                .HasColumnType("datetime")
                .HasColumnName("end_repair_time");
            entity.Property(e => e.Evaluation)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("evaluation");
            entity.Property(e => e.FinishEstimationTime)
                .HasColumnType("datetime")
                .HasColumnName("finish_estimation_time");
            entity.Property(e => e.HeadMechanic)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("head_mechanic");
            entity.Property(e => e.IdVehicle)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_vehicle");
            entity.Property(e => e.Odometer).HasColumnName("odometer");
            entity.Property(e => e.OrderDate)
                .HasColumnType("date")
                .HasColumnName("order_date");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.Progress)
                .HasDefaultValueSql("((0))")
                .HasColumnName("progress");
            entity.Property(e => e.RepairDescription)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("repair_description");
            entity.Property(e => e.RepairMethod)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("repair_method");
            entity.Property(e => e.RepairStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('WAITING')")
                .HasColumnName("repair_status");
            entity.Property(e => e.ReplacementPart)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("replacement_part");
            entity.Property(e => e.ServiceAdvisor)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("service_advisor");
            entity.Property(e => e.StartRepairTime)
                .HasColumnType("datetime")
                .HasColumnName("start_repair_time");

            entity.HasOne(d => d.HeadMechanicNavigation).WithMany(p => p.TrsBookingHeadMechanicNavigations)
                .HasForeignKey(d => d.HeadMechanic)
                .HasConstraintName("FK_head_mechanic");

            entity.HasOne(d => d.IdVehicleNavigation).WithMany(p => p.TrsBookings)
                .HasForeignKey(d => d.IdVehicle)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_id_vehicle");

            entity.HasOne(d => d.ServiceAdvisorNavigation).WithMany(p => p.TrsBookingServiceAdvisorNavigations)
                .HasForeignKey(d => d.ServiceAdvisor)
                .HasConstraintName("FK_service_advisor");
        });

        modelBuilder.Entity<TrsInspectionList>(entity =>
        {
            entity.HasKey(e => e.IdInspection).HasName("PK__trs_insp__8F958B5289EB343C");

            entity.ToTable("trs_inspection_list");

            entity.Property(e => e.IdInspection)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_inspection");
            entity.Property(e => e.Checklist).HasColumnName("checklist");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.IdBooking)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_booking");
            entity.Property(e => e.IdEquipment)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_equipment");

            entity.HasOne(d => d.IdBookingNavigation).WithMany(p => p.TrsInspectionLists)
                .HasForeignKey(d => d.IdBooking)
                .HasConstraintName("FK__trs_inspe__id_bo__31B762FC");

            entity.HasOne(d => d.IdEquipmentNavigation).WithMany(p => p.TrsInspectionLists)
                .HasForeignKey(d => d.IdEquipment)
                .HasConstraintName("FK__trs_inspe__id_eq__32AB8735");
        });

        modelBuilder.Entity<TrsPending>(entity =>
        {
            entity.HasKey(e => e.IdPending).HasName("PK__trs_pend__C59EEAF70A126F66");

            entity.ToTable("trs_pending");

            entity.Property(e => e.IdPending)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_pending");
            entity.Property(e => e.FinishTime)
                .HasColumnType("datetime")
                .HasColumnName("finish_time");
            entity.Property(e => e.IdBooking)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_booking");
            entity.Property(e => e.IdUser)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("id_user");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("reason");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");

            entity.HasOne(d => d.IdBookingNavigation).WithMany(p => p.TrsPendings)
                .HasForeignKey(d => d.IdBooking)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trs_pendi__id_bo__7EF6D905");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TrsPendings)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_trs_pending_id_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}