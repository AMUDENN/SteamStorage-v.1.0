using Microsoft.EntityFrameworkCore;
using SteamStorage.SteamStorageDB;
using SteamStorageDB;
using System;
using System.Windows.Media;

namespace SteamStorage.ApplicationLogic
{
    public class AdvancedArchive
    {
        public int Id { get; set; }
        public int IdSkin { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
        public double CostPurchase { get; set; }
        public double Amount { get; set; }
        public DateTime DatePurchase { get; set; }
        public string DatePurchaseString { get => DatePurchase.ToString(Constants.DateFormat); }
        public double CostSold { get; set; }
        public double Percent { get; set; }
        public DateTime DateSold { get; set; }
        public string DateSoldString { get => DateSold.ToString(Constants.DateFormat); }
        public Brush PercentForeground { get; set; }
        public string PercentString { get => (CostPurchase <= CostSold ? "+" : "") + Percent + "%"; }
        public AdvancedArchive(Archive archive)
        {
            SteamStorageDbContext db = GeneralMethods.db;
            db.Skins.LoadAsync();
            Id = (int)archive.Id;
            IdSkin = (int)archive.IdSkin;
            Title = archive.IdSkinNavigation.Title;
            Count = (int)archive.Count;
            CostPurchase = archive.CostPurchase;
            Amount = Count * CostPurchase;
            DatePurchase = DateTime.ParseExact(archive.DatePurchase, Constants.DateFormat, null);
            CostSold = archive.CostSold;
            Percent = Math.Round((CostSold - CostPurchase) / CostPurchase * 100, 2);
            DateSold = DateTime.ParseExact(archive.DateSold, Constants.DateFormat, null);
            PercentForeground = Percent >= 0 ? Brushes.Green : Brushes.Red;
        }
    }
}
