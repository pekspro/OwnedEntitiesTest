using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EFModeling.OwnedEntities
{

    public class Order
    {
        public int Id { get; set; }
        public StreetAddress ShippingAddress { get; set; }
    }

    public class StreetAddress
    {
        public int PostCode { get; set; }
        public string City { get; set; }
    }

    public class OwnedEntityContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFOwnedEntity;Trusted_Connection=True;ConnectRetryCount=0");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().OwnsOne(
                o => o.ShippingAddress,
                sa =>
                {
                    // NOTE: IsRequired() is used here
                    sa.Property(p => p.PostCode).HasColumnName("PostCode").IsRequired();
                    sa.Property(p => p.City).HasColumnName("ShipsToCity").IsRequired();
                });
        }
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            using (var context = new OwnedEntityContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Add(new Order
                {
                    ShippingAddress = new StreetAddress()
                    {
                        City = null,
                        PostCode = 12345
                    }
                });

                // Why does this works when City i required?
                context.SaveChanges();
            }

            using (var context = new OwnedEntityContext())
            {
                var orders = context.Orders.Include(a => a.ShippingAddress).ToList();

                foreach(var order in orders)
                {
                    // One order is printed here, but ShippingAddress is null even if we should get a value for PostCode.
                    Console.WriteLine($"OrderID: {order.Id} PostCode: {order.ShippingAddress?.PostCode} City: {order?.ShippingAddress?.City}");
                }
            }
        }
    }
}



/*
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EFModeling.OwnedEntities
{
    public static class Program
    {
        static void Main(string[] args)
        {
            using (var context = new OwnedEntityContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Add(new DetailedOrder
                {
                    Status = OrderStatus.Pending,
                    OrderDetails = new OrderDetails
                    {
                        ShippingAddress = new StreetAddress { City = "London", Street = "221 B Baker St" },
                        BillingAddress = new StreetAddress { City = "New York", Street = "11 Wall Street" }
                    }
                });

                context.SaveChanges();
            }

            using (var context = new OwnedEntityContext())
            {
                #region DetailedOrderQuery
                var order = context.DetailedOrders.First(o => o.Status == OrderStatus.Pending);
                Console.WriteLine($"First pending order will ship to: {order.OrderDetails.ShippingAddress.City}");
                #endregion
            }
        }
    }

    #region DetailedOrder
    public class DetailedOrder
    {
        public int Id { get; set; }
        public OrderDetails OrderDetails { get; set; }
        public OrderStatus Status { get; set; }
    }
    #endregion

    #region Distributor
    public class Distributor
    {
        public int Id { get; set; }
        public ICollection<StreetAddress> ShippingCenters { get; set; }
    }
    #endregion

    #region Order
    public class Order
    {
        public int Id { get; set; }
        public StreetAddress ShippingAddress { get; set; }
    }
    #endregion
    #region OrderDetails
    public class OrderDetails
    {
        public DetailedOrder Order { get; set; }
        public StreetAddress BillingAddress { get; set; }
        public StreetAddress ShippingAddress { get; set; }
    }
    #endregion

    #region OrderStatus
    public enum OrderStatus
    {
        Pending,
        Shipped
    }
    #endregion

    public class OwnedEntityContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<DetailedOrder> DetailedOrders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFOwnedEntity;Trusted_Connection=True;ConnectRetryCount=0");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region OwnsOne
            modelBuilder.Entity<Order>().OwnsOne(p => p.ShippingAddress);
            #endregion

            #region OwnsOneString
            modelBuilder.Entity<Order>().OwnsOne(typeof(StreetAddress), "ShippingAddress");
            #endregion

            #region ColumnNames
            modelBuilder.Entity<Order>().OwnsOne(
                o => o.ShippingAddress,
                sa =>
                {
                    sa.Property(p => p.Street).HasColumnName("ShipsToStreet");
                    sa.Property(p => p.City).HasColumnName("ShipsToCity");
                });
            #endregion

            #region OwnsOneNested
            modelBuilder.Entity<DetailedOrder>().OwnsOne(p => p.OrderDetails, od =>
            {
                od.WithOwner(d => d.Order);
                //od.Navigation(d => d.Order).UsePropertyAccessMode(PropertyAccessMode.Property);
                od.OwnsOne(c => c.BillingAddress);
                od.OwnsOne(c => c.ShippingAddress);
            });
            #endregion

            #region OwnsOneTable
            modelBuilder.Entity<DetailedOrder>().OwnsOne(p => p.OrderDetails, od =>
            {
                od.ToTable("OrderDetails");
            });
            #endregion

            #region OwnsMany
            modelBuilder.Entity<Distributor>().OwnsMany(p => p.ShippingCenters, a =>
            {
                a.WithOwner().HasForeignKey("OwnerId");
                a.Property<int>("Id");
                a.HasKey("Id");
            });
            #endregion
        }
    }

    #region StreetAddress
    [Owned]
    public class StreetAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
    }
    #endregion

}*/
