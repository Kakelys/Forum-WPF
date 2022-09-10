using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.User;
using Microsoft.Toolkit.Mvvm.Input;

namespace FoxLife.ViewModels.SignPage
{
    internal class SignView : ViewModelBase
    {
        private string _name;
        private SecureString _password;

        public SecureString Password
        {
            private get => _password;
            set
            {
                PasswordVisibility = value.Length > 0 ? Visibility.Hidden : Visibility.Visible;

                _password = value;
                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                NameVisibility = value == "" ? Visibility.Visible : Visibility.Hidden;

                _name = value;
                RaisePropertyChanged();
            }
        }

        public Visibility PasswordVisibility { get; set; } = Visibility.Visible;
        public Visibility NameVisibility { get; set; } = Visibility.Visible;

        public RelayCommand Back => new RelayCommand(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
        });

        private bool CheckData()
        {
            if (string.IsNullOrEmpty(Name))
            {
                MainViewModel.Message("EmptyUsernameError", MessageViewModel.MessageType.Error);
                return false;
            } 
            
            if (Name.Length > 24)
            {
                MainViewModel.Message("MaxLengthUsernameError", MessageViewModel.MessageType.Error);
                return false;
            }

            if (Password == null)
            {
                MainViewModel.Message("EmptyPasswordError", MessageViewModel.MessageType.Error);
                return false;
            }
            
            if (Password.Length is < 8 or >= 24)
            {
                MainViewModel.Message("PasswordLengthError", MessageViewModel.MessageType.Error);
                return false;
            }

            return true;
        }

        public RelayCommand ConfirmLogin => new RelayCommand(() =>
        {
            Task.Run(() =>
            {
                if (!CheckData()) return;

                var user = new User(Name, Password);
                try
                {
                    if (!UserContext.IsExists(Name))
                    {
                        MainViewModel.Message("NoSameUsernameError", MessageViewModel.MessageType.Error);
                        return;
                    }

                    if (user.Login())
                    {
                        MainViewModel.AfterLogin();
                        MainViewModel.Message($"{Application.Current.Resources["WelcomeMessageText"]}[b]{Name}[/b]", MessageViewModel.MessageType.Success, true);
                    }
                    else
                    {
                        MainViewModel.Message("IncorrectUserPasswordText", MessageViewModel.MessageType.Error);
                    }
                }
                catch
                {
                    MainViewModel.Message("LoginInternetError", MessageViewModel.MessageType.Error);
                }
            }).ConfigureAwait(false);
        });

        public RelayCommand ConfirmRegister => new RelayCommand( () =>
        {
            Task.Run(() =>
            {
                if (!CheckData()) return;

                var user = new User(Name, Password);
                if (UserContext.IsExists(Name))
                {
                    MainViewModel.Message("SameUsernameError", MessageViewModel.MessageType.Error);
                    return;
                }
                if (UserContext.Add(Name,Password))
                {
                    MainViewModel.Message("LoginSuccess", MessageViewModel.MessageType.Success);
                    MainViewModel.ChangePage(MainViewModel.PagesEnum.Login);
                }
                else
                {
                    MainViewModel.Message("RegisterFailedError", MessageViewModel.MessageType.Error);
                }
            }).ConfigureAwait(false);

        });
    }
}
