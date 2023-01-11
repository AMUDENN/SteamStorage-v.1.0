using Microsoft.EntityFrameworkCore;
using SteamStorageDB;
using System;
using Parser;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Threading;

namespace SteamStorage.ApplicationLogic
{
    internal class GeneralMethods
    {
        public enum ApplicationPages { Remains, Archive };
        public static readonly ApplicationContext db = new();
        public static ApplicationPages CurrentPage = ApplicationPages.Remains;
        public static void UndoChanges(ApplicationContext db)
        {
            foreach (var entry in db.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }
        public static Exception? AddGroup(string title)
        {
            try
            {
                if (CurrentPage == ApplicationPages.Remains) db.RemainGroups.Add(new RemainGroups(title));
                else if (CurrentPage == ApplicationPages.Archive) db.ArchiveGroups.Add(new ArchiveGroups(title));
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                UndoChanges(db);
                return ex;
            }
        }
        public static Exception? ChangeGroup(string OldTitle, string NewTitle)
        {
            try
            {
                if (CurrentPage == ApplicationPages.Remains)
                {
                    RemainGroups group = db.RemainGroups.First(x => x.Title == OldTitle);
                    group.Title = NewTitle;
                }
                else if (CurrentPage == ApplicationPages.Archive)
                {
                    ArchiveGroups group = db.ArchiveGroups.First(x => x.Title == OldTitle);
                    group.Title = NewTitle;
                }
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                UndoChanges(db);
                return ex;
            }
        }
        public static void AddSkinInDB(string url)
        {
            string title = ParserMethods.GetTitle(url).Result;
            db.Skins.Add(new(title, url));
            db.SaveChanges();
        }
        public static Skin? GetSkin(int id)
        {
            db.Skins.Load();
            Skin? skin = db.Skins.Local.Where(x => x.Id == id).First();
            return skin;
        }
        public static void ChangeCurrentPage(ApplicationPages page)
        {
            CurrentPage = page;
        }
    }
    public class RemainsMethods
    {
        public static BackgroundWorker BackgroundWorker = new();
        private static readonly Dictionary<string, Func<RemainElementFull, object>> RemainsOrderParameters = new()
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
        private static readonly ApplicationContext db = GeneralMethods.db;
        public static int? CurrentGroupId = null;
        static RemainsMethods()
        {
            BackgroundWorker.DoWork += UpdateInfoWork;
            BackgroundWorker.WorkerSupportsCancellation = true;
            BackgroundWorker.RunWorkerCompleted += UpdateComplete;
            BackgroundWorker.WorkerReportsProgress = true;
            BackgroundWorker.ProgressChanged += UpdateProgress;
        }
        public static void ChangeCurrentGroupId(int? group_id)
        {
            CurrentGroupId = group_id;
        }
        public static List<RemainGroups> GetRemainGroups()
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
            RemainGroups group = db.RemainGroups.Local.Where(x => x.Title == title).First();
            var remainsDel = db.Remains.Local.Where(x => x.Id_group == group.Id);
            foreach (var item in remainsDel)
            {
                db.Remains.Remove(item);
                db.SaveChanges();
            }
            db.RemainGroups.Remove(group);
            db.SaveChanges();
        }
        public static List<RemainElementFull> GetRemainElements(int? group_id)
        {
            db.Remains.LoadAsync();
            db.Skins.LoadAsync();
            db.PriceDynamics.LoadAsync();

            var remains = db.Remains.Local;
            var skins = db.Skins.Local;
            var priceDynamics = db.PriceDynamics.Local;

            var elements = remains.Where(x => group_id == null || x.Id_group == group_id);
            List<RemainElementFull> remainElements = new();
            foreach (var item in elements)
            {
                Skin skin = skins.Where(x => x.Id == item.Id_skin).First();

                var priceDynamicId = priceDynamics.Where(x => x.Id_remain == item.Id);

                Dictionary<DateTime, double> updateCosts = priceDynamicId.ToDictionary(x => x.DateUpdate, x => x.Cost_update);

                updateCosts.Add(item.DatePurchase, item.Cost_purchase);

                if (updateCosts.Count == 1) updateCosts.Add(item.DatePurchase.AddMilliseconds(1), item.Cost_purchase);

                remainElements.Add(new(item.Id, item.Id_skin, skin.Title, item.Count, item.Cost_purchase, item.DatePurchase, updateCosts));
            }
            return remainElements;
        }
        public static List<RemainElementFull> GetOrderedRemainElements(string parameter)
        {
            List<RemainElementFull> remainElements = GetRemainElements(CurrentGroupId);
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
                db.Remains.Add(new(db.Skins.Local.Where(x => x.Url == url).First().Id, CurrentGroupId is null ? 1 : (int)CurrentGroupId, date, cost, count));
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Неверно указана ссылка!");
            }
        }
        public static Exception? ChangeRemainElement(RemainElementFull remain, string url, int count, double cost, DateTime date, string group_title)
        {
            try
            {
                db.Skins.LoadAsync();
                db.RemainGroups.LoadAsync();
                db.Remains.LoadAsync();
                RemainElement remainElement = db.Remains.Local.Where(x => x.Id == remain.Id).First();
                if (!db.Skins.Local.Where(x => x.Url == url).Any())
                {
                    GeneralMethods.AddSkinInDB(url);
                    db.Skins.Load();
                }
                remainElement.Id_skin = db.Skins.Local.Where(x => x.Url == url).First().Id;
                remainElement.Count = count;
                remainElement.Cost_purchase = cost;
                remainElement.Date_purchase = date.ToString(StorageAPI.dateFormat);
                remainElement.Id_group = db.RemainGroups.Local.Where(x => x.Title == group_title).First().Id;
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Введены некорректные данные!");
            }
        }
        public static void DeleteRemainElement(RemainElementFull remain)
        {
            db.Remains.LoadAsync();
            RemainElement remainElement = db.Remains.Local.Where(x => x.Id == remain.Id).First();
            db.Remains.Local.Remove(remainElement);
            db.SaveChanges();
        }
        public static Exception? UpdateInfo(List<RemainElementFull> remainElementFull)
        {
            db.PriceDynamics.LoadAsync();
            db.Skins.LoadAsync();
            db.Remains.LoadAsync();
            try
            {
                int count = remainElementFull.Count;
                int i = 1;
                Random random = new();
                foreach (var item in remainElementFull)
                {
                    if (i % 15 == 0) Thread.Sleep(random.Next(23000, 30000));
                    else Thread.Sleep(random.Next(1500, 2650));
                    var (DateUpdate, Price) = ParserMethods.GetPrice(db.Skins.Local.Where(x => x.Id == item.Id_skin).First().Url).Result;
                    if (Price != -1 && !db.PriceDynamics.Local.Where(x => x.Id_remain == item.Id && x.DateUpdate == DateUpdate).Any())
                    {
                        db.PriceDynamics.Add(new(item.Id, DateUpdate, Price));
                        db.SaveChanges();
                        BackgroundWorker.ReportProgress(100 / count);
                        if (BackgroundWorker.CancellationPending) break;
                    }
                    i++;
                }
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("При обновлении данных произошла ошибка");
            }
        }
        public static void UpdateInfoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker helperBW = sender as BackgroundWorker;
            int? arg = (int?)e.Argument;
            e.Result = UpdateInfo(GetRemainElements(arg));
            if (helperBW.CancellationPending)
            {
                e.Cancel = true;
            }
        }
        public static void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            ((ProgressBar)MainWindow.RemainsPageInstance.UpdateUIElements.Children[1]).Value += e.ProgressPercentage;
            MainWindow.RemainsPageInstance.RefreshElements(CurrentGroupId);
        }
        public static void UpdateComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            MainWindow.RemainsPageInstance.UpdateComplete();
        }
    }
    public class ArchiveMethods
    {
        private static readonly Dictionary<string, Func<ArchiveElementFull, object>> ArchiveOrderParameters = new()
        {
            {"Название", x => x.Title },
            {"Количество", x => x.Count },
            {"Цена", x => x.CostPurchase },
            {"Сумма", x=>x.Amount },
            {"Дата покупки", x => x.DatePurchase },
            {"Цена продажи", x => x.CostPurchase },
            {"Изменение", x => x.Percent },
            {"Дата продажи", x => x.DateSold }
        };
        private static readonly Dictionary<string, bool> ArchiveIsDesc = new()
        {
            {"Название", false},
            {"Количество", true},
            {"Цена", true},
            {"Сумма",true},
            {"Дата покупки", false},
            {"Цена продажи", true},
            {"Изменение", true},
            {"Дата продажи", false}
        };
        private static readonly ApplicationContext db = GeneralMethods.db;
        public static int? CurrentGroupId = null;
        public static void ChangeCurrentGroupId(int? group_id)
        {
            CurrentGroupId = group_id;
        }
        public static List<ArchiveGroups> GetArchiveGroups()
        {
            return db.ArchiveGroups.ToList();
        }
        public static void DeleteGroupArchive(string title)
        {
            db.ArchiveGroups.Load();
            db.ArchiveGroups.Remove(db.ArchiveGroups.Local.Where(x => x.Title == title).First());
            db.SaveChanges();
        }
        public static void DeleteGroupArchiveAndArchiveElements(string title)
        {
            db.ArchiveGroups.Load();
            db.Archive.Load();
            ArchiveGroups group = db.ArchiveGroups.Local.Where(x => x.Title == title).First();
            var archiveDel = db.Archive.Local.Where(x => x.Id_group == group.Id);
            foreach (var item in archiveDel)
            {
                db.Archive.Remove(item);
                db.SaveChanges();
            }
            db.ArchiveGroups.Remove(group);
            db.SaveChanges();
        }
        public static List<ArchiveElementFull> GetArchiveElements(int? group_id)
        {
            db.Archive.Load();
            db.Skins.Load();

            var archive = db.Archive.Local;
            var skins = db.Skins.Local;

            var elements = archive.Where(x => group_id == null || x.Id_group == group_id);
            List<ArchiveElementFull> archiveElements = new();
            foreach (var item in elements)
            {
                Skin skin = skins.Where(x => x.Id == item.Id_skin).First();

                archiveElements.Add(new(item.Id, item.Id_skin, skin.Title, item.Count, item.Cost_purchase, item.DatePurchase, item.Cost_sold, item.DateSold));
            }
            return archiveElements;
        }
        public static List<ArchiveElementFull> GetOrderedArchiveElements(string parameter)
        {
            List<ArchiveElementFull> archiveElements = GetArchiveElements(CurrentGroupId);
            var archive = ArchiveIsDesc[parameter] ? archiveElements.OrderByDescending(ArchiveOrderParameters[parameter]) : archiveElements.OrderBy(ArchiveOrderParameters[parameter]);
            ArchiveIsDesc[parameter] = !ArchiveIsDesc[parameter];
            return archive.ToList();
        }
        public static Exception? AddNewArchiveElement(string url, int count, double cost_purchase, double cost_sold, DateTime date_purchase, DateTime date_sold, int group_id = -1)
        {
            try
            {
                int IdGroup = group_id == -1 ? (CurrentGroupId is null ? 1 : (int)CurrentGroupId) : (int)group_id;

                db.Skins.Load();
                if (!db.Skins.Local.Where(x => x.Url == url).Any())
                {
                    GeneralMethods.AddSkinInDB(url);
                }
                db.Skins.Load();
                db.Archive.Load();
                db.Archive.Add(new(db.Skins.Local.Where(x => x.Url == url).First().Id, IdGroup, date_purchase, date_sold, count, cost_purchase, cost_sold));
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Неверно указана ссылка!");
            }
        }
        public static Exception? ChangeArchiveElement(ArchiveElementFull archive, string url, int count, double cost_purchase, double cost_sold, DateTime date_purchase, DateTime date_sold, string group_title)
        {
            try
            {
                db.Skins.Load();
                db.ArchiveGroups.Load();
                db.Archive.Load();
                ArchiveElement archiveElement = db.Archive.Local.Where(x => x.Id == archive.Id).First();
                if (!db.Skins.Local.Where(x => x.Url == url).Any())
                {
                    GeneralMethods.AddSkinInDB(url);
                    db.Skins.Load();
                }
                archiveElement.Id_skin = db.Skins.Local.Where(x => x.Url == url).First().Id;
                archiveElement.Count = count;
                archiveElement.Cost_purchase = cost_purchase;
                archiveElement.Cost_sold = cost_sold;
                archiveElement.Date_purchase = date_purchase.ToString(StorageAPI.dateFormat);
                archiveElement.Date_sold = date_sold.ToString(StorageAPI.dateFormat);
                archiveElement.Id_group = db.ArchiveGroups.Local.Where(x => x.Title == group_title).First().Id;
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Введены некорректные данные!");
            }
        }
        public static void DeleteArchiveElement(ArchiveElementFull archive)
        {
            db.Archive.Load();
            ArchiveElement archiveElement = db.Archive.Local.Where(x => x.Id == archive.Id).First();
            db.Archive.Local.Remove(archiveElement);
            db.SaveChanges();
        }
    }
    public class RemainElementFull
    {
        public int Id { get; set; }
        public int Id_skin { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
        public double CostPurchase { get; set; }
        public double Amount { get; set; }
        public DateTime DatePurchase { get; set; }
        public string DatePurchaseString { get => DatePurchase.ToString(StorageAPI.dateFormat); }
        public double LastCost { get; set; }
        public double Percent { get; set; }
        public DateTime LastUpdate { get; set; }
        public string LastUpdateString { get => LastUpdate.ToString(StorageAPI.dateFormat); }
        public Dictionary<DateTime, double> UpdateCosts { get; set; } 
        public Brush PercentForeground { get; set; }
        public string PercentString { get => (CostPurchase <= LastCost ? "+" : "") + Percent + "%"; }
        public RemainElementFull(int id, int id_skin, string title, int count, double cost_purchase, DateTime date_purchase, Dictionary<DateTime, double> updateCosts)
        {
            Id = id;
            Id_skin = id_skin;
            Title = title;
            Count = count;
            CostPurchase = cost_purchase;
            Amount = count * cost_purchase;
            DatePurchase = date_purchase;
            UpdateCosts = updateCosts.OrderBy(x => x.Key.Ticks).ToDictionary(x => x.Key, x => x.Value);
            LastCost = UpdateCosts.Last().Value;
            LastUpdate = UpdateCosts.Last().Key;
            Percent = Math.Round((LastCost - cost_purchase) / cost_purchase * 100, 2);
            BrushConverter bc = new();
            PercentForeground = (Brush)bc.ConvertFrom(Percent >= 0 ? "#02b478" : "#fd4534");
        }
    }
    public class ArchiveElementFull
    {
        public int Id { get; set; }
        public int Id_skin { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
        public double CostPurchase { get; set; }
        public double Amount { get; set; }
        public DateTime DatePurchase { get; set; }
        public string DatePurchaseString { get => DatePurchase.ToString(StorageAPI.dateFormat); }
        public double CostSold { get; set; }
        public double Percent { get; set; }
        public DateTime DateSold { get; set; }
        public string DateSoldString { get => DateSold.ToString(StorageAPI.dateFormat); }
        public Brush PercentForeground { get; set; }
        public string PercentString { get => (CostPurchase <= CostSold ? "+" : "") + Percent + "%"; }
        public ArchiveElementFull(int id, int id_skin, string title, int count, double cost_purchase, DateTime date_purchase, double cost_sold, DateTime date_sold)
        {
            Id = id;
            Id_skin = id_skin;
            Title = title;
            Count = count;
            CostPurchase = cost_purchase;
            Amount = count * cost_purchase;
            DatePurchase = date_purchase;
            CostSold = cost_sold;
            Percent = Math.Round((cost_sold - cost_purchase) / cost_purchase * 100, 2);
            DateSold = date_sold;
            PercentForeground = Percent >= 0 ? Brushes.Green : Brushes.Red;
        }
    }
}
