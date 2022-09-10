using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using DevExpress.Mvvm;
using FoxLife.Models;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.Post;
using FoxLife.Models.DBInfo.Topic;
using FoxLife.Models.DBInfo.User;
using HTMLConverter;
using MaterialDesignThemes.Wpf;
using Microsoft.Toolkit.Mvvm.Input;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace FoxLife.ViewModels.TopicPage
{
    internal class PostListViewModel : ViewModelBase
    {
        public ObservableCollection<PostViewModel> Posts { get; set; }
        public ObservableCollection<CounterPageViewModel> Pages { get; set; }

        private int _topicId;

        public string TopicName { get; set; }
        public string StartPostMsg { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string AuthorRoleName { get; set; } = "";
        public int AuthorId { get; set;}
        public string StartMsgTime { get; set; }
        public ImageSource startPostImg { get; set; }

        private string _message;
        public Visibility MessagePlaceholder { get; set; } = Visibility.Visible;
        public Visibility SendPostVisibility { get; set; } = Visibility.Collapsed;
        public Visibility SendButtonVisibility { get; set; } = Visibility.Visible;
        public Visibility EditMenuVisibility { get; set; } = Visibility.Collapsed;

        public string AnswerHintMessage { get; set; }
        public int AnswerAncestorId { get; set; } = 0;
        public Visibility AnswerHintVisibility { get; set; } = Visibility.Collapsed;

        public Visibility FirstLastVisibility { get; set; } = Visibility.Collapsed;

        //PopUp 
        public bool AdditionallyPopUpState { get; set; } = false;
        public Visibility EditVisibility { get; set; } = Visibility.Collapsed;
        public Visibility AnswerVisibility { get; set; } = Visibility.Collapsed;

        public PackIconKind LockKind { get; set; }
        public Visibility LockVisibility { get; set; }
        public Brush? LockForeground { get; set; }

        public PackIconKind PinKind { get; set; }
        public Visibility PinVisibility { get; set; }
        public Brush? PinForeground { get; set; }

        private static PostListViewModel Page;
        private bool _scrollToDown = false;
        private PostDb? _editable = null;
        private bool _isTopicMessage = false;

        private int _currentPage = 1;
        private static int _postsToTake = 5;

        public bool ScrollToDown
        {
            get => _scrollToDown;
            set
            {
                _scrollToDown = value;
                if (value == true)
                {
                    ScrollToDown = false;
                }

                RaisePropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                MessagePlaceholder = value == "" ? Visibility.Visible : Visibility.Collapsed;

                _message = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand OpenAdditionally => new(() =>
        {
            if(User.IsLogin && (EditVisibility == Visibility.Visible 
                                 || AnswerVisibility==Visibility.Visible))
                AdditionallyPopUpState = true;
        });

        public RelayCommand CancelEdit => new(() =>
        {
            SendButtonVisibility = Visibility.Visible;
            EditMenuVisibility = Visibility.Collapsed;

            Message = "";
            _editable = null;
        });

        public RelayCommand ConfirmEdit => new(() =>
        {
            if (string.IsNullOrEmpty(Message))
            {
                MainViewModel.Message("EmptyMessage",MessageViewModel.MessageType.Error);
                return;
            }

            try
            {
                if (_isTopicMessage)
                {
                    if (TopicContext.UpdateMessage(_topicId, Message) == false) throw new Exception();
                }
                else
                {
                    if (PostContext.Update(_editable.Id, Message) == false) throw new Exception();
                }
                Update();
                _editable = null;
            }
            catch
            {
                MainViewModel.Message("EditError", MessageViewModel.MessageType.Error);
            }
        });

        public Models.RelayCommand AnswerCommand => new(obj =>
        {
            Answer((string)obj);
            AdditionallyPopUpState = false;
        });

        public RelayCommand BackToMain => new(() => { MainViewModel.ChangePage(MainViewModel.PagesEnum.Main); });
        public RelayCommand Back => new(() => { MainViewModel.ChangePage(MainViewModel.PagesEnum.Last); });

        public RelayCommand SendMessage => new(() =>
        {
            Task.Run(() =>
            {
                if (string.IsNullOrEmpty(Message))
                {
                    MainViewModel.Message("EmptyMessage", MessageViewModel.MessageType.Error);
                    return;
                }

                if (Message.Length > 3000)
                {
                    MainViewModel.Message("MessageMaxLengthError", MessageViewModel.MessageType.Error);
                    return;
                }

                if (!PostContext.Add(new PostDb(Message, _topicId, User.Id, AnswerAncestorId)))
                {
                    MainViewModel.Message("SendError", MessageViewModel.MessageType.Error);
                    return;
                }
            

                MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
                Message = "";
                Update();
            }).ConfigureAwait(false);
        });

        public RelayCommand StartEditCommand => new(() =>
        {
            AdditionallyPopUpState = false;
            
            StartEdit(new PostDb(){MsgTxt = StartPostMsg},true);
        });

        public Models.RelayCommand ChangePage => new(obj =>
        {
            Task.Run(() => { 
            if (obj == null)
            {
                MainViewModel.Message("PageLoadError", MessageViewModel.MessageType.Error);
                    return;
            }

            _currentPage = (int)obj;
            Update();
            });
        });

        public RelayCommand LastPage => new(() =>
        {
            Task.Run(() =>
            {
                var temp = (PostContext.GetPostCount(Page._topicId) / (double)_postsToTake);
                if (temp == 0)
                {
                    MainViewModel.Message("PageLoadError", MessageViewModel.MessageType.Error);
                    return;
                }

                var postCount = temp % 1 == 0 ? temp : temp + 1;

                _currentPage = (int)postCount;
                Update();
            }).ConfigureAwait(false);

        });

        public RelayCommand FirstPage => new(() =>
        {
            Task.Run(() =>
            {
                _currentPage = 1;
                Update();
            }).ConfigureAwait(false);
        });

        public RelayCommand ChangeCloseState => new(() =>
        {
            Task.Run(() =>
            {
                if (!(User.IsLogin
                      && User.RoleId >= FoxLifeParameters.MaxTopicLockRights
                      && User.RoleId <= FoxLifeParameters.MinTopicLockRights))
                {
                    
                    MainViewModel.Message("NoRightsError", MessageViewModel.MessageType.Error);
                    return;
                }

                

                if (!TopicContext.Update(_topicId,1))
                {
                    
                    return;
                }
                else
                {
                    MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
                }

                Update();
            }).ConfigureAwait(false);
        });

        public RelayCommand ChangePinState => new(() =>
        {
            Task.Run(() =>
            {
                if (!(User.IsLogin
                      && User.RoleId >= FoxLifeParameters.MaxPinRole
                      && User.RoleId <= FoxLifeParameters.MinPinRole))
                    return;

                if (!TopicContext.Update(_topicId,2))
                {
                    MainViewModel.Message("ChangeError", MessageViewModel.MessageType.Error);
                    return;
                }
                else
                {
                    MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
                }

                Update();
            }).ConfigureAwait(false);
        });

        public RelayCommand OpenAuthorProfile => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.UserProfile,AuthorId);
        });

        public RelayCommand CloseAnswerHint => new(() =>
        {
            AnswerAncestorId = 0;
            AnswerHintVisibility = Visibility.Collapsed;
            AnswerHintMessage = "";
        });

        public PostListViewModel()
        {
            Page = this;
        }

        public static void SetTopic(int id)
        {
            Page._topicId = id;
        }

        public void BeforeUpdate()
        {
            Page = this;
        }

        public static void Update()
        {
            if (Page == null || Page._topicId == -1)
                MainViewModel.Message("UpdateError", MessageViewModel.MessageType.Error);

            var topic = TopicContext.GetTopic(Page._topicId);
            if (topic == null) return;
            Page.AuthorName = topic.StartUserDb.Name;
            Page.AuthorRoleName = topic.StartUserDb.RoleObj.RoleName;
            Page.AuthorId = topic.StartMsgUsrId;

            Page.TopicName = topic.Name;
            Page.StartPostMsg = topic.StartMsgTXT;
            Page.StartMsgTime = topic.StartMsgTime.ToString(2);
            Page.startPostImg = ImgHelp.LoadImage(topic.StartUserDb.Avatar.Img);

            if (User.IsLogin
                && User.RoleId >= FoxLifeParameters.MaxTopicLockRights
                && User.RoleId <= FoxLifeParameters.MinTopicLockRights)
            {
                Page.LockVisibility = Visibility.Visible;
                Page.PinVisibility = Visibility.Visible;
            }
            else
            {
                Page.LockVisibility = topic.IsClosed ? Visibility.Visible : Visibility.Collapsed;
                Page.PinVisibility = topic.IsPinned ? Visibility.Visible : Visibility.Collapsed;
            }

            Page.LockKind = topic.IsClosed ? PackIconKind.LockOutline : PackIconKind.LockOpenOutline;
            Page.LockForeground = topic.IsClosed ? (SolidColorBrush)new BrushConverter().ConvertFromString("red") : (SolidColorBrush)new BrushConverter().ConvertFromString("white");

            Page.PinKind = topic.IsPinned ? PackIconKind.PinOutline : PackIconKind.PinOffOutline;
            Page.PinForeground = topic.IsPinned ? (SolidColorBrush)new BrushConverter().ConvertFromString("red") : (SolidColorBrush)new BrushConverter().ConvertFromString("white");

            //hide UI when closed 
            if (topic.IsClosed)
            {
                Page.EditVisibility = Visibility.Collapsed;
                Page.AnswerVisibility = Visibility.Collapsed;

                Page.Message = "";

                Page.SendPostVisibility = Visibility.Collapsed;
            }
            else
            {
                //logged user is creator 
                if (User.IsLogin && User.Id == topic.StartUserDb.Id && !User.IsBanned)
                {
                    Page.EditVisibility = Visibility.Visible;
                    Page.AnswerVisibility = Visibility.Collapsed;
                }
                else
                {
                    Page.EditVisibility = Visibility.Collapsed;
                    Page.AnswerVisibility = User.IsLogin ? Visibility.Visible : Visibility.Collapsed;
                }

                //edit message default settings
                Page.Message = "";
                Page.EditMenuVisibility = Visibility.Collapsed;
                Page.SendButtonVisibility = Visibility.Visible;

                Page.SendPostVisibility = User.IsLogin && !User.IsBanned ?
                    Visibility.Visible : Visibility.Collapsed;
            }

            var postCount = PostContext.GetPostCount(Page._topicId);
            if (postCount == 0)
            {
                Page.Posts = null;
                return;
            }

            var pageList = new List<int>();
            var pageCount = postCount / (double) _postsToTake;
            if (pageCount % 1 != 0) pageCount++;
            for (var i = Page._currentPage-3; i <= Page._currentPage+3; i++)
            {
                if (i > 0 && i <= pageCount)
                {
                    pageList.Add(i);
                }
            }

            if (pageCount > 4)
            {
                Page.FirstLastVisibility = Visibility.Visible;
            }
            else
            {
                Page.FirstLastVisibility = Visibility.Collapsed;
            }

            var list = PostContext.NewGetPostList(Page._topicId, Page._currentPage, _postsToTake, topic.IsClosed);
            if (list == null) return;

            Page.Pages = new ObservableCollection<CounterPageViewModel>(pageList.Select(x => new CounterPageViewModel(x, Page._currentPage)));
            Page.Posts = list;
        }

        public static void Clear()
        {
            Page = null;
        }

        public static void StartEdit(PostDb post, bool isTopicMessage)
        {
            Page._isTopicMessage = isTopicMessage;
            Page._editable = post;
            Page.Message = post.MsgTxt;
            Page.SendButtonVisibility = Visibility.Collapsed;
            Page.EditMenuVisibility = Visibility.Visible;
            Page.AdditionallyPopUpState = false;

            Page.ScrollToDown = true;
        }

        public static void Answer(string UserName, int PosId = 0)
        {
            Page.AnswerHintMessage = $"{Application.Current.Resources["Answer"]} {UserName}";
            Page.AnswerAncestorId = PosId;
            Page.AnswerHintVisibility = Visibility.Visible;

            Page.ScrollToDown = true;
        }
    }
}
