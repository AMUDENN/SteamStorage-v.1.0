using System;
using Microsoft.EntityFrameworkCore;

namespace SteamStorageDB
{
    public class RemainGroups
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public RemainGroups() { }
        public RemainGroups(string title)
        {
            Title = title;
        }
    }
    public class ArchiveGroups
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public ArchiveGroups() { }
        public ArchiveGroups(string title)
        {
            Title = title;
        }
    }
    public class Skin
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public Skin() { }
        public Skin(string title, string url)
        {
            Title = title;
            Url = url;
        }
    }
    public class ArchiveElement
    {
        public int Id { get; set; }
        public int Id_skin { get; set; }
        public int Id_group { get; set; }
        public string Date_purchase { get; set; }
        public string Date_sold { get; set; }
        public int Count { get; set; }
        public double Cost_purchase { get; set; }  
        public double Cost_sold { get; set; }
        public DateTime DatePurchase { get => DateTime.ParseExact(Date_purchase, StorageAPI.dateFormat, null); }
        public DateTime DateSold { get => DateTime.ParseExact(Date_sold, StorageAPI.dateFormat, null); }
        public ArchiveElement() { }
        public ArchiveElement(int id_skin, int id_group, DateTime date_purchase, DateTime date_sold, int count, double cost_purchase, double cost_sold)
        {
            Id_skin = id_skin;
            Id_group = id_group;
            Date_purchase = date_purchase.ToString(StorageAPI.dateFormat);
            Date_sold = date_sold.ToString(StorageAPI.dateFormat);
            Count = count;
            Cost_purchase = cost_purchase;
            Cost_sold = cost_sold;
        }
    }
    public class RemainElement
    {
        public int Id { get; set; }
        public int Id_skin { get; set; }
        public int Id_group { get; set; }
        public string Date_purchase { get; set; }
        public double Cost_purchase { get; set; }
        public int Count { get; set; }
        public DateTime DatePurchase { get => DateTime.ParseExact(Date_purchase, StorageAPI.dateFormat, null); }
        public RemainElement() { }
        public RemainElement(int id_skin, int id_group, DateTime date_purchase, double cost_purchase, int count)
        {
            Id_skin = id_skin;
            Id_group = id_group;
            this.Date_purchase = date_purchase.ToString(StorageAPI.dateFormat);
            Cost_purchase = cost_purchase;
            Count = count;
        }
    }
    public class PriceDynamicsElement
    {
        public int Id { get; set; }
        public int Id_remain { get; set; }
        public string Date_update { get; set; }
        public double Cost_update { get; set; }
        public DateTime DateUpdate { get => DateTime.ParseExact(Date_update, StorageAPI.dateFormat, null); }
        public PriceDynamicsElement() { }
        public PriceDynamicsElement(int id_remain, DateTime date_update, double cost_update)
        {
            Id_remain = id_remain;
            Date_update = date_update.ToString(StorageAPI.dateFormat);
            Cost_update = cost_update;
        }
        
    }
    public class ApplicationContext : DbContext
    {
        public DbSet<RemainGroups> RemainGroups { get; set; } = null!;
        public DbSet<ArchiveGroups> ArchiveGroups { get; set; } = null!;
        public DbSet<Skin> Skins { get; set; } = null!;
        public DbSet<ArchiveElement> Archive { get; set; } = null!;
        public DbSet<RemainElement> Remains { get; set; } = null!;
        public DbSet<PriceDynamicsElement> PriceDynamics { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={StorageAPI.DBpath}");
        }
    }
}
