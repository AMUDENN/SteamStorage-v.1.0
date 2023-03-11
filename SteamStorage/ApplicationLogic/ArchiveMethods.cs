using Microsoft.EntityFrameworkCore;
using SteamStorage.SteamStorageDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamStorage.ApplicationLogic
{
    public class ArchiveMethods
    {
        private static readonly Dictionary<string, Func<AdvancedArchive, object>> ArchiveOrderParameters = new()
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
        private static readonly SteamStorageDbContext db = GeneralMethods.db;
        public static ArchiveGroup? CurrentGroup = null;
        public static void ChangeCurrentGroupId(string title)
        {
            db.ArchiveGroups.LoadAsync();
            CurrentGroup = title == "Все" ? null : db.ArchiveGroups.Local.Where(x => x.Title == title).First();
        }
        public static List<ArchiveGroup> GetArchiveGroups()
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
            db.Archives.Load();
            ArchiveGroup group = db.ArchiveGroups.Local.Where(x => x.Title == title).First();
            var archiveDel = db.Archives.Local.Where(x => x.IdGroup == group.Id);
            foreach (var item in archiveDel)
            {
                db.Archives.Remove(item);
                db.SaveChanges();
            }
            db.ArchiveGroups.Remove(group);
            db.SaveChanges();
        }
        public static List<AdvancedArchive> GetArchiveElements(ArchiveGroup? Group)
        {
            db.Archives.Load();

            var archive = db.Archives.Local;

            List<Archive> elements = archive.Where(x => Group == null || x.IdGroup == Group.Id).ToList();
            List<AdvancedArchive> archiveElements = elements.Select(x => new AdvancedArchive(x)).ToList();
            return archiveElements;
        }
        public static List<AdvancedArchive> GetOrderedArchiveElements(string parameter)
        {
            List<AdvancedArchive> archiveElements = GetArchiveElements(CurrentGroup);
            var archive = ArchiveIsDesc[parameter] ? archiveElements.OrderByDescending(ArchiveOrderParameters[parameter]) : archiveElements.OrderBy(ArchiveOrderParameters[parameter]);
            ArchiveIsDesc[parameter] = !ArchiveIsDesc[parameter];
            return archive.ToList();
        }
        public static Exception? AddNewArchiveElement(string url, long count, double cost_purchase, double cost_sold, DateTime date_purchase, DateTime date_sold, int group_id = -1)
        {
            try
            {
                db.Skins.Load();
                if (!db.Skins.Local.Where(x => x.Url == url).Any())
                {
                    GeneralMethods.AddSkinInDB(url);
                }
                db.Skins.Load();
                db.Archives.Load();
                Archive archive = new()
                {
                    IdSkin = db.Skins.Local.Where(x => x.Url == url).First().Id,
                    IdGroup = CurrentGroup is null ? 1 : (int)CurrentGroup.Id,
                    DatePurchase = date_purchase.ToString(Constants.DateFormat),
                    DateSold = date_sold.ToString(Constants.DateFormat),
                    Count = count,
                    CostPurchase = cost_purchase,
                    CostSold = cost_sold
                };
                db.Archives.Add(archive);
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Неверно указана ссылка!");
            }
        }
        public static Exception? ChangeArchiveElement(AdvancedArchive archive, string url, long count, double cost_purchase, double cost_sold, DateTime date_purchase, DateTime date_sold, string group_title)
        {
            try
            {
                db.Skins.Load();
                db.ArchiveGroups.Load();
                db.Archives.Load();
                Archive archiveElement = db.Archives.Local.Where(x => x.Id == archive.Id).First();
                if (!db.Skins.Local.Where(x => x.Url == url).Any())
                {
                    GeneralMethods.AddSkinInDB(url);
                    db.Skins.Load();
                }
                archiveElement.IdSkin = db.Skins.Local.Where(x => x.Url == url).First().Id;
                archiveElement.Count = count;
                archiveElement.CostPurchase = cost_purchase;
                archiveElement.CostSold = cost_sold;
                archiveElement.DatePurchase = date_purchase.ToString(Constants.DateFormat);
                archiveElement.DateSold = date_sold.ToString(Constants.DateFormat);
                archiveElement.IdGroup = db.ArchiveGroups.Local.Where(x => x.Title == group_title).First().Id;
                db.SaveChanges();
                return null;
            }
            catch
            {
                GeneralMethods.UndoChanges(db);
                return new Exception("Введены некорректные данные!");
            }
        }
        public static void DeleteArchiveElement(AdvancedArchive archive)
        {
            db.Archives.Load();
            Archive archiveElement = db.Archives.Local.Where(x => x.Id == archive.Id).First();
            db.Archives.Local.Remove(archiveElement);
            db.SaveChanges();
        }
    }
}
