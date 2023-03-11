using Microsoft.EntityFrameworkCore;
using Parser;
using SteamStorage.SteamStorageDB;
using System;
using System.Linq;

namespace SteamStorage.ApplicationLogic
{
    internal class GeneralMethods
    {
        public enum ApplicationPages { Remains, Archive };
        public static readonly SteamStorageDbContext db = new();
        public static ApplicationPages CurrentPage = ApplicationPages.Remains;
        public static void UndoChanges(SteamStorageDbContext db)
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
                if (CurrentPage == ApplicationPages.Remains)
                {
                    RemainGroup remaingroup = new();
                    remaingroup.Title = title;
                    db.RemainGroups.Add(remaingroup);
                }
                else if (CurrentPage == ApplicationPages.Archive)
                {
                    ArchiveGroup archivegroup = new();
                    archivegroup.Title = title;
                    db.ArchiveGroups.Add(archivegroup);
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
        public static Exception? ChangeGroup(string OldTitle, string NewTitle)
        {
            try
            {
                if (CurrentPage == ApplicationPages.Remains)
                {
                    RemainGroup group = db.RemainGroups.First(x => x.Title == OldTitle);
                    group.Title = NewTitle;
                }
                else if (CurrentPage == ApplicationPages.Archive)
                {
                    ArchiveGroup group = db.ArchiveGroups.First(x => x.Title == OldTitle);
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
            Skin skin = new()
            {
                Title = title,
                Url = url
            };
            db.Skins.Add(skin);
            db.SaveChanges();
        }
        public static Skin? GetSkin(long id)
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
}
