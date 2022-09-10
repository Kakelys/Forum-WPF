using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.Native;
using FoxLife.Models.DBInfo.User;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.Ban
{
    internal class BanContext : ForumDbContext
    {
        public static bool Add(string reason,int userId, int adminId, DateTime unbanTime, bool isPerm = false )
        {
            var db = new BanContext();
            try
            {
                var trans = db.Database.BeginTransaction();

                try
                {
                    var ban = new BanDb()
                    {
                        Reason = reason,
                        AdminId = adminId,
                        UserId = userId,
                        UnbanTime = unbanTime,
                        BanTime = DateTime.UtcNow,
                        IsPerm = isPerm,
                        IsActive = true
                    };
                    db.Ban.Add(ban);

                    var user = db.User.FirstOrDefault(u=>u.Id == userId);
                    if (user == null) throw new Exception();
                    user.IsBanned = true;

                    db.SaveChanges();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checking if user ban is already over
        /// </summary>
        /// <returns>returns true if unbanned</returns>
        public static bool TryUnban(int userId)
        {
            var db = new BanContext();
            try
            {
                var ban = db.Ban.AsNoTracking()
                    .Where(b => b.UserId == userId && b.IsActive)
                    .OrderByDescending(b => b.UnbanTime)
                    .ThenByDescending(b => b.IsPerm).First();

                if (ban.UnbanTime >= DateTime.UtcNow || ban.IsPerm)
                    return false;

                var trans = db.Database.BeginTransaction();
                try
                {
                    db.Ban.AsQueryable()
                        .Where(b => b.UserId == userId && b.IsActive)
                        .ForEach(b => b.IsActive = false);

                    var user = db.User.AsQueryable().FirstOrDefault(u => u.Id == userId);
                    if (user == null) throw new Exception();
                    user.IsBanned = false;

                    db.SaveChanges();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Unban(int userId)
        {
            var db = new BanContext();
            try
            {
                var trans = db.Database.BeginTransaction();
                try
                {
                    db.Ban.AsQueryable()
                        .Where(b => b.UserId == userId && b.IsActive)
                        .ForEachAsync(b => b.IsActive = false);

                    var user = db.User.FirstOrDefault(u=>u.Id == userId);
                    if (user == null) throw new Exception();
                    user.IsBanned = false;

                    db.SaveChanges();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="user">
        /// requires including bans and Admin
        /// </param>
        /// <returns></returns>
        public static string GetBanMessage(UserDb user, bool addTabBefore = false)
        {
            if (!user.IsBanned || user.GetBans.Count == 0)
                return "";

            var ban = user.GetBans
                .Where(b=>b.IsActive)
                .OrderByDescending(b => b.UnbanTime)
                .ThenByDescending(b => b.IsPerm)
                .FirstOrDefault();

            if (ban == null)
                return "";

            if (ban.Admin == null)
                return "";

            return BuildBanMessage(ban, addTabBefore);
        }

        public static string GetBanMessage(int userId, bool addTabBefore = false)
        {
            var user = UserContext.GetUser(userId, true);
            if (user == null)
                return "";

            var ban = user.GetBans
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.UnbanTime)
                .ThenByDescending(b => b.IsPerm)
                .FirstOrDefault();

            if (ban == null)
                return "";

            if (ban.Admin == null)
                return "";

            return BuildBanMessage(ban, addTabBefore);
        }

        private static string BuildBanMessage(BanDb ban, bool addTabBefore)
        {
            return $"{(addTabBefore ? "\t" : "")}{Application.Current.Resources["BanWhoText"]}: {ban.Admin.Name} \n" +
                   $"{(addTabBefore ? "\t" : "")}{Application.Current.Resources["BanTimeText"]}: {ban.BanTime.ToString(2)} \n" +
                   $"{(addTabBefore ? "\t" : "")}{Application.Current.Resources["BanReasonText"]}: {ban.Reason} \n" +
                   $"{(addTabBefore ? "\t" : "")}{Application.Current.Resources["UnbanTimeText"]}: {(ban.IsPerm ? Application.Current.Resources["Never"] : ban.UnbanTime.ToString(2))}";
        }
    }
}
