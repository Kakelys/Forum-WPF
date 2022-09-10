using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DevExpress.Mvvm;
using FoxLife.Models;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.User;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace FoxLife.ViewModels.UserPage
{
    internal class UserProfileViewModel : ViewModelBase
    {
        private bool _canUpdate { get; set; } = false;
        private int _userId;

        private static UserProfileViewModel Page;

        public string CountOfPosts { get; set; }
        public string CountOfTopics { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public ImageSource Img { get; set; }
        public String RegDate { get; set; }

        public Visibility OptionsVisibility { get; set; } = Visibility.Collapsed;
        public bool AdditionallyPopUpState { get; set; } = false;
        public bool PasswordPopUpState { get; set; } = false;

        public Visibility OldPasswordPlaceholder { get; set; } = Visibility.Visible;
        public Visibility NewPasswordPlaceholder { get; set; } = Visibility.Visible;

        private SecureString _oldPassword;
        private SecureString _newPassword;

        public SecureString OldPassword
        {
            private get => _oldPassword;
            set
            {
                OldPasswordPlaceholder = value.Length > 0 ? Visibility.Hidden : Visibility.Visible;

                _oldPassword = value;
                RaisePropertyChanged();
            }
        }

        public SecureString NewPassword
        {
            private get => _newPassword;
            set
            {
                NewPasswordPlaceholder = value.Length > 0 ? Visibility.Hidden : Visibility.Visible;

                _newPassword = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand ChangePassword => new(() =>
        {
            AdditionallyPopUpState = false;
            PasswordPopUpState = true;
        });

        public RelayCommand ConfirmChangePassword => new(() =>
        {
            if (NewPassword.Length == 0 || OldPassword.Length == 0)
            {
                MainViewModel.Message("EmptyFieldsError", MessageViewModel.MessageType.Error);
            }

            if (UserContext.Update(OldPassword,NewPassword))
            {
                PasswordPopUpState = false;
                MainViewModel.Message("ChangeSuccess", MessageViewModel.MessageType.Success);
            }
        });

        public RelayCommand ChangeImage => new(() =>
        {
            var imgData = ImgHelp.OpenImage();
            if (imgData == null) return;
            if (!UserContext.Update(User.Id, imgData))
            {
                MainViewModel.Message("ImgLoadError", MessageViewModel.MessageType.Error);
            }
            else
            {
                MainViewModel.Message("ChangeSuccess", MessageViewModel.MessageType.Success);
                MainViewModel.AfterLogin();
            }
        });

        public RelayCommand BackToMain => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Main);
        });

        public RelayCommand Back => new(()=>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
        });

        public RelayCommand Options => new(() =>
        {
            AdditionallyPopUpState = true;
        });

        public RelayCommand OpenUserTopics => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.TopicList,-2,_userId);
        });

        public UserProfileViewModel()
        {
            Page = this;
            _canUpdate = true;
        }

        public void BeforeUpdate()
        {
            Page = this;
        }

        public static void Update()
        {
            if (Page == null || Page._canUpdate == false)
            {
                MainViewModel.Message("UpdateError", MessageViewModel.MessageType.Error);
                return;
            }

            var user = UserContext.GetUser(Page._userId);
            if (user == null)
            {
                MainViewModel.Message("PageLoadError", MessageViewModel.MessageType.Error);
                return;
            }

            
            Page.Name = $"{Application.Current.Resources["UserText"]}: {user.Name}" ;
            Page.CountOfPosts = $"{Application.Current.Resources["PostsText"]}: {user.CountOfMsg}";
            Page.CountOfTopics = $"{Application.Current.Resources["TopicsText"]}: {user.CountOfTopics}";
            Page.RegDate = $"{Application.Current.Resources["RegisterDateText"]}: {user.RegDate.ToString(3)}";
            Page.GroupName = $"{Application.Current.Resources["GroupText"]}: {user.RoleObj.RoleName}";
            
            
            if (User.IsLogin && User.Id == Page._userId)
            {
                Page.OptionsVisibility = Visibility.Visible;
            }
            else
            {
                Page.OptionsVisibility = Visibility.Collapsed;
            }

            Task.Run(()=>
            {
                Page.Img = ImgHelp.LoadImage(user.Avatar.Img);
            });

        }

        public static void SetUser(int userId)
        {
            Page._userId = userId;
        }

        public static void Clear()
        {
            Page = null;
        }
    }
}
