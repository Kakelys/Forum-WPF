using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxLife.Models
{
    internal static class FoxLifeParameters
    {
        public static readonly int MaxTopicLockRights;
        public static readonly int MinTopicLockRights;

        public static readonly short MaxPinRole;
        public static readonly int MinPinRole;

        public static readonly int MaxAdminMenuRole;
        public static readonly int MinAdminMenuRole;

        public static readonly int MaxBanControlRole;
        public static readonly int MinBanControlRole;

        public static readonly int MaxStaffControlRole;
        public static readonly int MinStaffControlRole;

        public static int MessageCounter { get; set; } = 0;

        public static readonly int ReLoginTimeMs;

        static FoxLifeParameters()
        {
            MaxTopicLockRights = 0;
            MinTopicLockRights = 2;

            MaxAdminMenuRole = 0;
            MinAdminMenuRole = 2;

            MaxBanControlRole = 0;
            MinBanControlRole = 2;

            MaxStaffControlRole = 0;
            MinStaffControlRole = 0;

            MaxPinRole = 0;
            MinPinRole = 2;

            ReLoginTimeMs = 600000;
        }

        public static void ChangeTimeZone(string str)
        {
            //for settings
            //write some code later...
        }
    }
}
