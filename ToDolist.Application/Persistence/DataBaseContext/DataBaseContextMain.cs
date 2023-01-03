﻿using Application.Interfaces.Context;
using Common.Priority;
using Common.Status;
using Domain.Attributes;
using Domain.Entites.Cart;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DataBaseContext
{
    public class DataBaseContextMain:DbContext,IDataBaseContext
    {
        public DataBaseContextMain(DbContextOptions<DataBaseContextMain> options):base(options)
        {

        }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<StatusInCarts> StatusInCarts { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<PriorityInCarts> PriorityInCarts { get; set; }

        

        protected override void OnModelCreating(ModelBuilder Builder)
        {
            SeedData(Builder);

            foreach (var entityType in Builder.Model.GetEntityTypes())
            {
                if (entityType.ClrType.GetCustomAttributes(typeof(AuditTableAttribute), true).Length > 0)
                {
                    Builder.Entity(entityType.Name).Property<DateTime>("InsertTime");
                    Builder.Entity(entityType.Name).Property<DateTime?>("UpdateTime");
                    Builder.Entity(entityType.Name).Property<DateTime?>("RemoveTime");
                    Builder.Entity(entityType.Name).Property<bool>("IsRemoved");
                }
            }
        }

        #region Seed Data
        public void SeedData(ModelBuilder Builder)
        {
            Builder.Entity<Status>().HasData(new Status { Id = 1, Name = nameof(CartInStatus.Suspension) });
            Builder.Entity<Status>().HasData(new Status { Id = 2, Name = nameof(CartInStatus.NoStarted) });
            Builder.Entity<Status>().HasData(new Status { Id = 3, Name = nameof(CartInStatus.InProgress) });
            Builder.Entity<Status>().HasData(new Status { Id = 4, Name = nameof(CartInStatus.InComplete) });
            Builder.Entity<Priority>().HasData(new Priority { Id = 1, Name = nameof(CartInPriority.General) });
            Builder.Entity<Priority>().HasData(new Priority { Id = 2, Name = nameof(CartInPriority.Urgent) });
            Builder.Entity<Priority>().HasData(new Priority { Id = 3, Name = nameof(CartInPriority.Critical) });
        }
        #endregion
        #region Save Change
        public override int SaveChanges()
        {
            var modifiedEntires = ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Added ||
                p.State == EntityState.Modified ||
                p.State == EntityState.Deleted
                );
            foreach (var item in modifiedEntires)
            {
                var entityType = item.Context.Model.FindEntityType(item.Entity.GetType());

                var insertTime = entityType.FindProperty("InsertTime");
                var updateTime = entityType.FindProperty("UpdateTime");
                var isRemoved = entityType.FindProperty("IsRemoved");
                var removedTime = entityType.FindProperty("RemoveTime");
                if (item.State == EntityState.Added && insertTime != null)
                {
                    item.Property("InsertTime").CurrentValue = DateTime.Now;
                }
                if (item.State == EntityState.Modified && updateTime != null)
                {
                    item.Property("UpdateTime").CurrentValue = DateTime.Now;
                }
                if (item.State == EntityState.Deleted && isRemoved != null && removedTime != null)
                {
                    item.Property("IsRemoved").CurrentValue = true;
                    item.Property("RemoveTime").CurrentValue = DateTime.Now;
                }

            }
            return base.SaveChanges();
        }

        #endregion
    }
}
