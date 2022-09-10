using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using FoxLife.Models;
using FoxLife.Models.DBInfo.Ban;
using FoxLife.Models.DBInfo.Role;
using FoxLife.Models.DBInfo.User;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace FoxLife.ViewModels.AdminMenu
{
    internal class AdminMenuViewModel:ViewModelBase
    {
        private static AdminMenuViewModel Page;

        public ObservableCollection<RoleDb> Roles { get; set; }
        public RoleDb? SelectedRole { get; set; }

        public Visibility BanMenuVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ChooseRoleVisibility { get; set; } = Visibility.Collapsed;

        public Visibility SearchVisibility { get; set; } = Visibility.Collapsed;

        public Visibility NamePlaceholder { get; set; } = Visibility.Visible;
        private string _userName;
        private int _roleId;
        private int _userId;

        public string SearchUserInfo { get; set; } = "";
        public Visibility SearchUserInfoVisibility { get; set; } = Visibility.Collapsed;

        public string ActionName { get; set; }
        public Visibility ActionNameVisibility { get; set; } = Visibility.Collapsed;
        private Action _action;

        private string _banTime;
        public Visibility BanTimePlaceholderVisibility { get; set; } = Visibility.Visible;
        private string _banReason;
        public Visibility BanReasonPlaceholderVisibility { get; set; } = Visibility.Visible;

        public enum Action
        {
            StaffControl = 0,
            BansControl = 1,
        }

        public string BanTime
        {
            get => _banTime;
            set
            {
                BanTimePlaceholderVisibility = value == "" ?  Visibility.Visible : Visibility.Collapsed;
                _banTime = value;

                RaisePropertyChanged();
            }
        }

        public string BanReason
        {
            get =>  _banReason;
            set
            {
                BanReasonPlaceholderVisibility = value == "" ? Visibility.Visible : Visibility.Collapsed;
                _banReason = value;

                RaisePropertyChanged();
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                NamePlaceholder = value == "" ? Visibility.Visible : Visibility.Hidden;
                _userName = value;

                RaisePropertyChanged();
            }
        }

        public RelayCommand Search => new(() =>
        {
            Task.Run(() =>
                {
                    var user = UserContext.GetUser(UserName, true);

                    if (user == null)
                    {
                        MainViewModel.Message("NoSameUsernameError", MessageViewModel.MessageType.Error);
                        return;
                    }

                    var banInfo = "";
                    if (user.IsBanned)
                    {
                        banInfo = $"{BanContext.GetBanMessage(user, true)}";
                    }

                    SearchUserInfo = $"{Application.Current.Resources["UserNameText"]}: {user.Name}\n" +
                                     $"{Application.Current.Resources["GroupText"]}: {user.RoleObj.RoleName}\n" +
                                     $"{Application.Current.Resources["StatusText"]}: {(user.IsBanned ? $"[font color='red']{Application.Current.Resources["Banned"]}[/font]" : $"[font color='green']{Application.Current.Resources["Ok"]}[/font]")}\n" +
                                     $"{banInfo}" +
                                     $"{Application.Current.Resources["BansAmountText"]}: {user.GetBans.Count}";

                    SearchUserInfoVisibility = Visibility.Visible;

                    _roleId = user.Role;
                    _userId = user.Id;

                    switch (_action)
                    {
                        case Action.BansControl:

                            break;

                        case Action.StaffControl:
                            LoadRoles();
                            break;
                    }
                })
                .ConfigureAwait(false);
        });

        public RelayCommand CheckBanTimeFormat => new(() =>
        {
            var reg = new Regex(@"^\s*0\s*$");

            var tempDate = DateTime.UtcNow;
            if(tempDate.TryAddString(BanTime) || reg.IsMatch(BanTime))
                MainViewModel.Message("Ok", MessageViewModel.MessageType.Success);
            else
                MainViewModel.Message("TimeFormatError", MessageViewModel.MessageType.Error);
        });

        public RelayCommand Ban => new(() =>
        {
            if (string.IsNullOrEmpty(UserName))
            {
                MainViewModel.Message("EmptyUsernameError", MessageViewModel.MessageType.Error);
                return;
            }

            if (string.IsNullOrEmpty(BanTime))
            {
                MainViewModel.Message("TimeFormatError", MessageViewModel.MessageType.Error);
                return;
            }

            if (string.IsNullOrEmpty(BanReason))
            {
                MainViewModel.Message("BanReasonError", MessageViewModel.MessageType.Error);
                return;
            }

            var reg = new Regex(@"^\s*0\s*$");
            var unbanTime = DateTime.UtcNow;
            var user = UserContext.GetUser(UserName);
            var banSuccess = false;

            if (user == null)
            {
                MainViewModel.Message("NoSameUsernameError", MessageViewModel.MessageType.Error);
                return;
            }

            if (user.Role > -1 && user.Role <= User.RoleId || !CanDoAction(Action.BansControl))
            {
                MainViewModel.Message("NoRightsError", MessageViewModel.MessageType.Error);
                return;
            }

            if (reg.IsMatch(BanTime))
            {
                banSuccess = BanContext.Add(BanReason, user.Id, User.Id, DateTime.UtcNow, true);
            }
            else if(!unbanTime.TryAddString(BanTime))
            {
                MainViewModel.Message("TimeFormatError", MessageViewModel.MessageType.Error);
                return;
            }
            else
            {
                banSuccess = BanContext.Add(BanReason, user.Id, User.Id, unbanTime);
            }

            if (banSuccess)
            {
                MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
            }
            else
            {
                MainViewModel.Message("Failed", MessageViewModel.MessageType.Error);
            }

        });

        public RelayCommand Unban => new(()=>
        {
            if (string.IsNullOrEmpty(UserName))
            {
                MainViewModel.Message("EmptyUsernameError", MessageViewModel.MessageType.Error);
                return;
            }

            if (!CanDoAction(Action.BansControl))
            {
                MainViewModel.Message("NoRightsError", MessageViewModel.MessageType.Error);
                return;
            }

            var user = UserContext.GetUser(UserName);

            if (user == null)
            {
                MainViewModel.Message("NoSameUsernameError", MessageViewModel.MessageType.Error);
                return;
            }

            if (BanContext.Unban(user.Id))
            {
                MainViewModel.Message( "Success", MessageViewModel.MessageType.Success);
            }
            else
            {
                MainViewModel.Message("Failed", MessageViewModel.MessageType.Error);
            }

        });

        public RelayCommand ConfirmRoleChange => new(() =>
        {
            if (_roleId < User.RoleId)
            {
                MainViewModel.Message("NoRightsError",MessageViewModel.MessageType.Error);
                return;
            }

            if (SelectedRole==null || !UserContext.Update(_userId, SelectedRole.Id))
            {
                MainViewModel.Message("ChangeError", MessageViewModel.MessageType.Error);
                return;
            }

            MainViewModel.Message("ChangeSuccess", MessageViewModel.MessageType.Success);
        });

        public RelayCommand BackToMain => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Main);
        });

        public RelayCommand Back => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
        });

        public Models.RelayCommand ChooseAction => new(obj =>
        {
            
            switch ((Action)int.Parse((string)obj))
            {
                case Action.StaffControl:
                    if (!(CanDoAction(Action.StaffControl)))
                    {
                        MainViewModel.Message("NoRightsError",
                            MessageViewModel.MessageType.Error);
                        return;
                    }

                    SetDefault();

                    _action = Action.StaffControl;
                    SearchVisibility = Visibility.Visible;

                    ActionName = (string)Application.Current.Resources["StaffControlText"];
                    break;

                case Action.BansControl:
                    if (!(CanDoAction(Action.BansControl)))
                    {
                        MainViewModel.Message("NoRightsError",
                            MessageViewModel.MessageType.Error);
                        return;
                    }

                    SetDefault();

                    _action = Action.BansControl;
                    SearchVisibility = Visibility.Visible;
                    BanMenuVisibility = Visibility.Visible;

                    ActionName = (string)Application.Current.Resources["BansControlText"];
                    break;
                    default:
                        return;
            }

            ActionNameVisibility = Visibility.Visible;

        });

        public AdminMenuViewModel()
        {
            Page = this;
        }

        public bool CanDoAction(Action action)
        {
            switch (action)
            {
                case Action.BansControl:
                    return
                        User.IsLogin && !User.IsBanned
                                     && User.RoleId >= FoxLifeParameters.MaxBanControlRole
                                     && User.RoleId <= FoxLifeParameters.MinBanControlRole;
                case Action.StaffControl:
                    return User.IsLogin && !User.IsBanned
                                        && User.RoleId >= FoxLifeParameters.MaxStaffControlRole
                                        && User.RoleId <= FoxLifeParameters.MinStaffControlRole;
            }

            return false;
        }

        public void SetDefault()
        {

            BanMenuVisibility = Visibility.Collapsed;
            ChooseRoleVisibility = Visibility.Collapsed;
            Roles = null;
            SelectedRole = null;

            SearchVisibility = Visibility.Collapsed;
            SearchUserInfoVisibility = Visibility.Collapsed;
            SearchUserInfo = "";

            ActionName = "";
            BanTime = "";
        }

        public void BeforeUpdate()
        {
            Page = this;
        }

        private void LoadRoles()
        {
            var roles = RoleContext.GetRoleList();
            if (roles == null)
            {
                ChooseRoleVisibility = Visibility.Collapsed;
                MainViewModel.Message("RoleLoadError", MessageViewModel.MessageType.Error);
                return;
            }

            SelectedRole = roles.FirstOrDefault(r => r.Id == _roleId);

            Page.Roles = new ObservableCollection<RoleDb>(roles);
            ChooseRoleVisibility = Visibility.Visible;
            MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
        }

        public static void Update() 
        {
            if (!User.IsLogin || User.RoleId > 2 || User.RoleId<0 || User.IsBanned)
            {
                MainViewModel.Message("NoRightsError", MessageViewModel.MessageType.Error);
                MainViewModel.ChangePage(MainViewModel.PagesEnum.Main);

                return;
            }
        }
    }
}
