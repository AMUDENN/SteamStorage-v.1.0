using Microsoft.EntityFrameworkCore;
using SteamStorage.SteamStorageDB;
using System;
using System.Windows.Media;

namespace SteamStorage.ApplicationLogic
{
    public class AdvancedArchive: Archive
    {
        public string Title { get; set; }
        public double Amount { get; set; }
        public new DateTime DatePurchase { get; set; }
        public string DatePurchaseString { get => DatePurchase.ToString(Constants.DateFormat); }
        public double Percent { get; set; }
        public new DateTime DateSold { get; set; }
        public string DateSoldString { get => DateSold.ToString(Constants.DateFormat); }
        public Brush PercentForeground { get; set; }
        public string PercentString { get => (CostPurchase <= CostSold ? "+" : "") + Percent + "%"; }
        public AdvancedArchive(Archive archive)
        {
            SteamStorageDbContext db = GeneralMethods.db;
            db.Skins.LoadAsync();

            Id = archive.Id;
            IdSkin = archive.IdSkin;
            Title = archive.IdSkinNavigation.Title;
            Count = archive.Count;
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
