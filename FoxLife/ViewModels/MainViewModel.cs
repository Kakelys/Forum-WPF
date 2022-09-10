using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Mvvm;
using FoxLife.Models;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.User;
using FoxLife.View.Pages;
using FoxLife.ViewModels.ForumPage;
using FoxLife.ViewModels.MainPage;
using FoxLife.ViewModels.SignPage;
using FoxLife.ViewModels.TopicPage;
using FoxLife.ViewModels.UserPage;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;
using Topic = FoxLife.View.Pages.Topic;

namespace FoxLife.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public ObservableCollection<MessageViewModel> MessageList { get; set; } = new();

        //for changing page
        private List<Page> _oldPage { get; set; } = new();
        private Page _newPage { get; set; }
        private Page _clear { get; set; }
        private static MainViewModel main;

        //user info
        public ImageSource UserImage { get; set; }
        public string UserName { get; set; }

        //for login
        public Visibility SignVisibility { get; set; } = Visibility.Visible;
        public Visibility UserVisibility { get; set; } = Visibility.Hidden;
        public Visibility AdminMenuVisibility { get; set; } = Visibility.Collapsed;
        private Timer? _authTimer = null;

        //frame
        public Page CurrentPage { get; set; }
        private static readonly object _pageLocker = new();

        public WindowState WindowState { get; set; } = WindowState.Normal;

        public enum PagesEnum
        {
            Register,
            Login,
            Main,
            TopicList, // add.params: forumdId, userId, set forumdId to -2 to load user Topics, forumId required
            TopicCreate,
            ForumCreate, // add.params: section Id, required
            Topic, // add.params: topic Id, required
            UserProfile, // add.params: user Id, required
            AdminMenu,
            Last
        }

        #region Languages

        public ObservableCollection<Language> Languages { get; set; }

        private Language? _language;

        public Language? Language
        {
            get => _language;
            set
            {

                if (value != null)
                {
                    if (value == _language) return;
                    global::App.Language = value.Culture;
                    Task.Run(UpdateCurrentPage).ConfigureAwait(false);
                }

                _language = value;
                RaisePropertyChanged();
            }
        }

       

        #endregion

        public MainViewModel()
        {
            main = this;
            _clear = new Clear();
            CurrentPage = _clear;
            Languages = new ObservableCollection<Language>(Language.Load());
            _language = Languages[0];

            ChangePage(PagesEnum.Main);
        }

        public static void ChangePage(PagesEnum page, params object[] props)
        {
            Task.Run(() =>
            {
                lock (_pageLocker)
                {
                    Page curPage = main.CurrentPage;

                    Application.Current.Dispatcher.Invoke(() => { main.CurrentPage = main._clear; });

                    //creating object of selected page
                    switch (page)
                    {
                        case PagesEnum.Login:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new View.Pages.LogIn(); });
                            break;
                        case PagesEnum.Register:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new Register(); });
                            break;
                        case PagesEnum.Main:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new View.Pages.MainPage(); });
                            main._oldPage.Clear();
                            break;
                        case PagesEnum.TopicList:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new TopicList(); });
                            if (props != null && props.Length >= 2)
                                TopicListViewModel.SetUpdate((int)props[0], (int)props[1]);
                            else
                                TopicListViewModel.SetUpdate((int)props[0]);
                            break;
                        case PagesEnum.TopicCreate:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new TopicListCreate(); });
                            break;
                        case PagesEnum.Topic:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new Topic(); });
                            PostListViewModel.SetTopic((int)props[0]);
                            break;
                        case PagesEnum.UserProfile:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new UserProfile(); });
                            UserProfileViewModel.SetUser((int)props[0]);
                            break;
                        case PagesEnum.ForumCreate:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new ForumCreate(); });
                            ForumCreateViewModel.SetSectionId((int)props[0]);
                            break;
                        case PagesEnum.AdminMenu:
                            Application.Current.Dispatcher.Invoke(() => { main._newPage = new View.Pages.AdminMenu(); });
                            break;
                        case PagesEnum.Last:
                            if (main._oldPage.Count>0)
                            {
                                while(main._oldPage.Last().GetType() == curPage.GetType())
                                {
                                    main._oldPage.RemoveAt(main._oldPage.Count - 1);
                                }
                                MethodInfo? update = null;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    update = main._oldPage.Last().DataContext.GetType().GetMethod("Update");
                                });
                                if (update != null) update.Invoke(null, null);
                                main.CurrentPage = main._oldPage.Last();

                                BeforeLastPageUpdate(main._oldPage.Last());

                                Type? type = null;
                                Application.Current.Dispatcher.Invoke(() => { type = main._oldPage.Last().DataContext.GetType(); });
                                if (type!=null && type == typeof(MainPageView))
                                {
                                    main._oldPage.Clear();
                                }
                                else
                                    main._oldPage.RemoveAt(main._oldPage.Count-1);
                            }
                            else
                                ChangePage(PagesEnum.Main);
                            break;
                    }

                    if (page is not PagesEnum.Last)
                    {
                        MethodInfo? update = null;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            update = main._newPage.DataContext.GetType().GetMethod("Update");
                        });
                        if (update != null) update.Invoke(null, null);
                        main.CurrentPage = main._newPage;

                        main._newPage = null;
                    }

                    main._oldPage.Add(main.CurrentPage);
                }
            }).ConfigureAwait(false);
        }

        public static void BeforeLastPageUpdate(Page page)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    ((dynamic)page.DataContext).BeforeUpdate();
                }
                catch
                {
                }
            });
        }

        public static void UpdateCurrentPage()
        {
            lock (_pageLocker)
            {
                MethodInfo? temp = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (main._oldPage.Count > 0)
                    {
                        temp = main._oldPage.Last().DataContext.GetType().GetMethod("Update");
                        BeforeLastPageUpdate(main._oldPage.Last());
                    }
                    else
                    {
                        temp = main.CurrentPage.DataContext.GetType().GetMethod("Update");
                        BeforeLastPageUpdate(main.CurrentPage);
                    }
                });
                if (temp != null)
                {
                    temp.Invoke(null, null);
                    if (main._oldPage.Count > 0)
                    {
                        main.CurrentPage = main._oldPage.Last();
                    }
                    else
                    {
                        ChangePage(PagesEnum.Main);
                    }
                }
            }
        }

        #region Auth

        private static void ReLogin(object? obj)
        {
            if (User.IsLogin && !User.ReLogin())
            {
                Message("a", MessageViewModel.MessageType.Error, 60000);
                main.SignOut.Execute(null);
            }
        }

        public static void AfterLogin()
        {
            if(main._authTimer != null)
                main._authTimer.Change(Timeout.Infinite, Timeout.Infinite);
            else
                main._authTimer = new Timer(ReLogin, null, FoxLifeParameters.ReLoginTimeMs, FoxLifeParameters.ReLoginTimeMs);

            main.UserName = User.UserName;

            main.SignVisibility = Visibility.Collapsed;
            main.UserVisibility = Visibility.Visible;
            if (User.RoleId >= FoxLifeParameters.MaxAdminMenuRole && User.RoleId <= FoxLifeParameters.MinAdminMenuRole)
            {
                main.AdminMenuVisibility = Visibility.Visible;
            }

            Task.Run(() =>
            {
                Type? type = null;
                Type? preLast = null;
                if(main._oldPage.Count>=2)
                    Application.Current.Dispatcher.Invoke(() => { preLast = main._oldPage[^2].GetType(); });
                Application.Current.Dispatcher.Invoke(() => { type = main._oldPage.Last().GetType(); });
                if (type == typeof(LogIn)&&preLast!=null&&preLast==typeof(Register))
                    main._oldPage.RemoveAt(main._oldPage.Count - 1);

                if (type !=typeof(UserProfile))
                {
                    main._oldPage.RemoveAt(main._oldPage.Count - 1);
                    ChangePage(PagesEnum.Last);
                }
                else
                {
                    UpdateCurrentPage();
                }

                var temp = ImgContext.GetImage(User.ImgId);
                Application.Current.Dispatcher.Invoke(() => { main.UserImage = temp; });
            }).ConfigureAwait(false);
        }

        public static void AfterSignOut()
        {
            Task.Run(() =>
            {
                if(main._authTimer!=null)
                    main._authTimer.Change(Timeout.Infinite, Timeout.Infinite);
                main.SignVisibility = Visibility.Visible;
                main.UserVisibility = Visibility.Collapsed;
                main.AdminMenuVisibility = Visibility.Collapsed;
                main.UserImage = null;
                main.UserName = null;
                UpdateCurrentPage();
            });
        }

        public RelayCommand SignOut => new(() =>
        {
            User.SignOut();
            AfterSignOut();
        });
        public RelayCommand Profile => new(() =>
        {
            ChangePage(PagesEnum.UserProfile,User.Id);
        });

        #endregion

        #region MessageFieldMethods

        public Models.RelayCommand MessageRemoveCommand => new((obj) =>
        {
            MessageRemove((int)obj,0);
        });
        
        public static void Message(string message, MessageViewModel.MessageType type, bool fromString, int msDelay = 5000)
        {
            MessageViewModel? messageView = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                messageView = new MessageViewModel(FoxLifeParameters.MessageCounter++, message, type, msDelay);
                main.MessageList.Add(messageView);
            });
            if (messageView == null) return;
            MessageRemove(messageView.Id, msDelay);
        }

        /// <summary>
        /// Use msDelay -1 to infinity
        /// </summary>
        public static void Message(string resourceName, MessageViewModel.MessageType type, int msDelay = 5000)
        {
            MessageViewModel? messageView = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                messageView = new MessageViewModel(FoxLifeParameters.MessageCounter++, (string)Application.Current.Resources[$"{resourceName}"], type, msDelay);
                main.MessageList.Add(messageView);
            });
            if (messageView == null) return;
            MessageRemove(messageView.Id, msDelay);
        }

        private static void MessageRemove(int id, int msDelay)
        {
            if (msDelay == -1) return;
            Task.Run(async ()=>
            {
                try
                {
                    await Task.Delay(msDelay);
                    var temp = main.MessageList.FirstOrDefault(m => m.Id == id);
                    if (temp == null) return;
                    Application.Current.Dispatcher.Invoke(() => { main.MessageList.Remove(temp); });
                }
                catch
                {
                }
            }).ConfigureAwait(false);
        }

        private static void DeleteMessage(object obj)
        {
            var id = (int)obj;
            
        }

        #endregion

        public RelayCommand LoginMenu => new(() =>
        {
            ChangePage(PagesEnum.Login);
        });

        public RelayCommand RegisterMenu => new(() =>
        {
            ChangePage(PagesEnum.Register);
        });

        public RelayCommand AdminMenu => new(() =>
        {
            ChangePage(PagesEnum.AdminMenu);
        });

        public RelayCommand OpenOwnTopics => new(() =>
        {
            ChangePage(PagesEnum.TopicList, -2, User.Id);
        });

        public RelayCommand CloseWindow => new RelayCommand(() =>
        {
            Application.Current.Shutdown();
        });

        public RelayCommand MinimizeWindow => new RelayCommand(() =>
        {
            WindowState = WindowState.Minimized;
        });
    }
}
