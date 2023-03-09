using Microsoft.EntityFrameworkCore;
using Parser;
using SteamStorage.SteamStorageDB;
using SteamStorageDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

namespace SteamStorage.ApplicationLogic
{
    public class RemainsMethods
    {
        public static BackgroundWorker BackgroundWorker = new();
        private static readonly Dictionary<string, Func<AdvancedRemain, object>> RemainsOrderParameters = new()
        {
            {"Название", x => x.Title },
            {"Количество", x => x.Count },
            {"Цена", x => x.CostPurchase },
            {"Сумма", x=>x.Amount },
            {"Дата покупки", x => x.DatePurchase },
            {"Текущая цена", x => x.LastCost },
            {"Изменение", x => x.Percent },
            {"Последнее обновление", x => x.LastUpdate }
        };
        private static readonly Dictionary<string, bool> RemainsIsDesc = new()
        {
            {"Название", false},
            {"Количество", true},
            {"Цена", true},
            {"Сумма",true},
            {"Дата покупки", false},
            {"Текущая цена", true},
            {"Изменение", true},
            {"Последнее обновление", false}
        };
        private static readonly SteamStorageDbContext db = GeneralMethods.db;
        private static readonly SteamStorageDbContext AdditionalDB = new();
        public static RemainGroup? CurrentGroup = null;
        static RemainsMethods()
        {
            BackgroundWorker.DoWork += UpdateInfoWork;
            BackgroundWorker.WorkerSupportsCancellation = true;
            BackgroundWorker.RunWorkerCompleted += UpdateComplete;
            BackgroundWorker.WorkerReportsProgress = true;
            BackgroundWorker.ProgressChanged += UpdateProgress;
        }
        public static void ChangeCurrentGroup(string title)
        {
            db.RemainGroups.LoadAsync();
            CurrentGroup = title == "Все" ? null : db.RemainGroups.Local.Where(x => x.Title == title).First();
        }
        public static List<RemainGroup> GetRemainGroups()
        {
            return db.RemainGroups.ToList();
        }
        public static void DeleteGroupRemain(string title)
        {
            db.RemainGroups.LoadAsync();
            db.RemainGroups.Remove(db.RemainGroups.Local.Where(x => x.Title == title).First());
            db.SaveChanges();
        }
        public static void DeleteGroupRemainAndRemainElements(string title)
        {
            db.RemainGroups.LoadAsync();
            db.Remains.LoadAsync();
            RemainGroup group = db.RemainGroups.Local.Where(x => x.Title == title).First();
            var remainsDel = db.Remains.Local.Where(x => x.IdGroup == group.Id);
            foreach (var item in remainsDel)
            {
                db.Remains.Remove(item);
                db.SaveChanges();
            }
            db.RemainGroups.Remove(group);
            db.SaveChanges();
        }
        public static List<AdvancedRemain> GetRemainElements(RemainGroup? Group)
        {
            db.Remains.LoadAsync();

            var remains = db.Remains.Local;

            List<Remain> elements = remains.Where(x => Group == null || x.IdGroup == Group.Id).ToList();
            List<AdvancedRemain> remainElements = elements.Select(x => new AdvancedRemain(x)).ToList();
            return remainElements;
        }
        public static List<AdvancedRemain> GetOrderedRemainElements(string parameter)
        {
            List<AdvancedRemain> remainElements = GetRemainElements(CurrentGroup);
            var remains = RemainsIsDesc[parameter] ? remainElements.OrderByDescending(RemainsOrderParameters[parameter]) : remainElements.OrderBy(RemainsOrderParameters[parameter]);
            RemainsIsDesc[parameter] = !RemainsIsDesc[parameter];
            return remains.ToList();
        }
        public static Exception? AddNewRemainElement(string url, int count, double cost, DateTime date)
        {
            try
            {
                db.Skins.LoadAsync();
                if (!db.Skins.Local.Where(x => x.Url == url).Any())
                {
                    GeneralMethods.AddSkinInDB(url);
                }
                db.Skins.LoadAsync();
                db.Remains.LoadAsync();
                Remain remain = new()
                {
                    IdSkin = db.Skins.Local.Where(x => x.Url == url).First().Id,
                    IdGroup = CurrentGroup is null ? 1 : (int)CurrentGroup.Id,
                    DatePurchase = date.ToString(Constants.DateFormat),
                    CostPurchase = cost,
                    Count = count
                };
                db.Remains.Add(remain);
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Неверно указана ссылка!");
            }
        }
        public static Exception? ChangeRemainElement(AdvancedRemain remain, string url, int count, double cost, DateTime date, string group_title)
        {
            try
            {
                db.Skins.LoadAsync();
                db.RemainGroups.LoadAsync();
                db.Remains.LoadAsync();
                Remain remainElement = db.Remains.Local.Where(x => x.Id == remain.Id).First();
                if (!db.Skins.Local.Where(x => x.Url == url).Any())
                {
                    GeneralMethods.AddSkinInDB(url);
                    db.Skins.Load();
                }
                remainElement.IdSkin = db.Skins.Local.Where(x => x.Url == url).First().Id;
                remainElement.Count = count;
                remainElement.CostPurchase = cost;
                remainElement.DatePurchase = date.ToString(Constants.DateFormat);
                remainElement.IdGroup = db.RemainGroups.Local.Where(x => x.Title == group_title).First().Id;
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Введены некорректные данные!");
            }
        }
        public static void DeleteRemainElement(AdvancedRemain remain)
        {
            db.Remains.LoadAsync();
            Remain remainElement = db.Remains.Local.Where(x => x.Id == remain.Id).First();
            db.Remains.Local.Remove(remainElement);
            db.SaveChanges();
        }
        public static Exception? UpdateInfo(List<AdvancedRemain> remainElementFull)
        {
            AdditionalDB.PriceDynamics.LoadAsync();
            AdditionalDB.Skins.LoadAsync();
            AdditionalDB.Remains.LoadAsync();
            try
            {
                int count = remainElementFull.Count;
                int i = 1;
                Random random = new();
                foreach (var item in remainElementFull)
                {
                    if (i % 15 == 0) Thread.Sleep(random.Next(23000, 30000));
                    else Thread.Sleep(random.Next(1500, 2650));
                    var (DateUpdate, Price) = ParserMethods.GetPrice(AdditionalDB.Skins.Local.Where(x => x.Id == item.IdSkin).First().Url).Result;
                    if (Price != -1 && !AdditionalDB.PriceDynamics.Local.Where(x => x.IdRemain == item.Id && DateTime.ParseExact(x.DateUpdate, Constants.DateFormat, null) == DateUpdate).Any())
                    {
                        PriceDynamic priceDynamic = new()
                        {
                            IdRemain = item.Id,
                            DateUpdate = DateUpdate.ToString(Constants.DateFormat),
                            CostUpdate = Price
                        };
                        AdditionalDB.PriceDynamics.Add(priceDynamic);
                        AdditionalDB.SaveChanges();
                        BackgroundWorker.ReportProgress(100 / count);
                        if (BackgroundWorker.CancellationPending) break;
                    }
                    i++;
                }
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(AdditionalDB);
                return new Exception("При обновлении данных произошла ошибка");
            }
        }
        public static void UpdateInfoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker helperBW = sender as BackgroundWorker;
            RemainGroup? arg = (RemainGroup?)e.Argument;
            e.Result = UpdateInfo(GetRemainElements(arg));
            if (helperBW.CancellationPending)
            {
                e.Cancel = true;
            }
        }
        public static void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            ((ProgressBar)MainWindow.RemainsPageInstance.UpdateUIElements.Children[1]).Value += e.ProgressPercentage;
            MainWindow.RemainsPageInstance.RefreshElements();
        }
        public static void UpdateComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            MainWindow.RemainsPageInstance.UpdateComplete();
        }
    }
}
