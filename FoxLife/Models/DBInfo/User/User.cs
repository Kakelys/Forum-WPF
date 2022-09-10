using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using FoxLife.Models.DBInfo.Ban;
using FoxLife.View;
using FoxLife.ViewModels;

namespace FoxLife.Models.DBInfo.User
{
    internal class User
    {
        public string Name { get; set; }
        private SecureString? _passwd;

        public static string UserName { get; private set; }
        public static int Id { get; private set; }
        public static int RoleId { get; private set; } = 1000;
        public static int ImgId { get; private set; }
        public static bool IsLogin { get; private set; }
        public static bool IsBanned { get; private set; }

        private static SecureString? _password;

        public User(string name, SecureString password)
        {
            Name = name;
            _passwd = password;
        }

        public bool Login()
        {
            try
            {
                var user = UserContext.CheckLogin(Name, _passwd);
                if (user == null)
                    return false;

                _password = _passwd;
                SignIn(user.Name, user.Id, user.Role, user.Img, user.IsBanned);

                if (IsBanned)
                {
                    if (BanContext.TryUnban(user.Id))
                    {
                        MainViewModel.Message("unbanned", MessageViewModel.MessageType.Success, true);
                    }
                    else
                    {
                        ShowBanMessage();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ReLogin()
        {
            MainViewModel.Message("ReLogin work", MessageViewModel.MessageType.Success);
            try
            {
                var user = UserContext.CheckLogin(Id, _password);
                if (user == null)
                    return false;

                if (!IsBanned && user.IsBanned)
                {
                    if (BanContext.TryUnban(user.Id))
                    {
                        MainViewModel.Message("unbanned", MessageViewModel.MessageType.Success, true);
                        user.IsBanned = false;
                    }
                    else
                    {
                        ShowBanMessage();
                    }
                }

                SignIn(user.Name, user.Id, user.Role, user.Img, user.IsBanned);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ShowBanMessage()
        {
            Task.Run(() =>
            {
                MainViewModel.Message(
                    $"{Application.Current.Resources["BanText"]}\n{Application.Current.Resources["BanWarningText"]}\n{BanContext.GetBanMessage(Id)}",
                    MessageViewModel.MessageType.Warning,
                    true
                    ,-1);
            }).ConfigureAwait(false);
        }

        public static void SignOut()
        {
            UserName = "";
            Id = -1;
            RoleId = 1000;
            IsLogin = false;
            IsBanned = false;
        }

        public static void Update(int newImgId)
        {
            ImgId = newImgId;
        }

        private static void SignIn(string name, int id, int roleId, int imgId, bool isBanned)
        {
            UserName = name;
            Id = id;
            RoleId = roleId;
            ImgId = imgId;
            IsLogin = true;
            IsBanned = isBanned;
        }
    }
}
