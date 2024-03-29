﻿using Microsoft.EntityFrameworkCore;
using SteamStorage.SteamStorageDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace SteamStorage.ApplicationLogic
{
    public class AdvancedRemain : Remain
    {
        public string Title { get; set; }
        public double Amount { get; set; }
        public new DateTime DatePurchase { get; set; }
        public string DatePurchaseString { get => DatePurchase.ToString(Constants.DateFormat); }
        public double LastCost { get; set; }
        public double Percent { get; set; }
        public DateTime LastUpdate { get; set; }
        public string LastUpdateString { get => LastUpdate.ToString(Constants.DateFormat); }
        public Dictionary<DateTime, double> UpdateCosts { get; set; }
        public Brush PercentForeground { get; set; }
        public string PercentString { get => (CostPurchase <= LastCost ? "+" : "") + Percent + "%"; }
        public AdvancedRemain(Remain remain)
        {
            SteamStorageDbContext db = GeneralMethods.db;
            db.Skins.LoadAsync();
            db.PriceDynamics.LoadAsync();

            Id = remain.Id;
            IdSkin = remain.IdSkin;
            Title = remain.IdSkinNavigation.Title;
            Count = remain.Count;
            CostPurchase = remain.CostPurchase;
            Amount = Count * CostPurchase;
            DatePurchase = DateTime.ParseExact(remain.DatePurchase, Constants.DateFormat, null);

            UpdateCosts = remain.PriceDynamics.Where(x => x.IdRemain == Id).ToDictionary(x => DateTime.ParseExact(x.DateUpdate, Constants.DateFormat, null), x => x.CostUpdate);
            UpdateCosts.Add(DatePurchase, CostPurchase);
            if (UpdateCosts.Count == 1) UpdateCosts.Add(DatePurchase.AddMilliseconds(1), CostPurchase);
            UpdateCosts = UpdateCosts.OrderBy(x => x.Key.Ticks).ToDictionary(x => x.Key, x => x.Value);

            LastCost = UpdateCosts.Last().Value;
            LastUpdate = UpdateCosts.Last().Key;
            Percent = Math.Round((LastCost - CostPurchase) / CostPurchase * 100, 2);
            BrushConverter bc = new();
            PercentForeground = (Brush)bc.ConvertFrom(Percent >= 0 ? "#02b478" : "#fd4534");
        }
    }
}
