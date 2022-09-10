using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.Forum;
using FoxLife.Models.DBInfo.Topic;
using FoxLife.Models.DBInfo.User;
using FoxLife.View;
using Microsoft.Toolkit.Mvvm.Input;

namespace FoxLife.ViewModels.ForumPage
{
    internal class TopicListViewModel : ViewModelBase
    {
        public ObservableCollection<TopicViewModel> Topics { get; set; }
        public ObservableCollection<CounterPageViewModel> Pages { get; set; }

        public static TopicListViewModel Page;

        public static int ForumId { get; private set; }
        public int forumId = -1;
        public int userId = -1;

        private int _currentPage = 1;
        private static int _takeAmount = 5;
        private int _topicToDo = -1;
        private bool _topicDeletePopUpState = false;
        private bool _topicChangePopUpState = false;

        private string _newTopicName;
        public Visibility ButtonsVisibility { get; set; } = Visibility.Collapsed;
        public Visibility FirstLastVisibility { get; set; } = Visibility.Collapsed;


        public string NewTopicName
        {
            get => _newTopicName;
            set
            {
                _newTopicName = value;

                RaisePropertyChanged();
            }
        }

        public bool TopicChangePopUpState
        {
            get => _topicChangePopUpState;
            set
            {
                if (value == false) _topicToDo = -1;
                _topicChangePopUpState = value;

                RaisePropertyChanged();
            }
        }

        public bool TopicDeletePopUpState
        {
            get => _topicDeletePopUpState;
            set
            {
                if (value == false)
                    _topicToDo = -1;

                _topicDeletePopUpState = value;

                RaisePropertyChanged();
            }
        }

        public Models.RelayCommand SetTopicToChange => new(obj =>
        {
            if (obj == null)
            {
                MainViewModel.Message("LoadError", MessageViewModel.MessageType.Error);
                return;
            }

            var topic = obj as TopicDb;
            _topicToDo = topic.Id;
            NewTopicName = topic.Name;

            TopicChangePopUpState = true;
        });

        public RelayCommand ConfirmTopicChange => new(() =>
        {
            Task.Run(()=>
            {

            }).ConfigureAwait(false);
            if (string.IsNullOrEmpty(NewTopicName))
            {
                MainViewModel.Message("EmptyTopicNameError", MessageViewModel.MessageType.Error);
                return;
            }

            if (NewTopicName.Length > 50)
            {
                MainViewModel.Message("MaxSectionNameError", MessageViewModel.MessageType.Error);
                return;
            }

            if (!TopicContext.UpdateName(_topicToDo, NewTopicName))
            {
                MainViewModel.Message("ChangeNameError", MessageViewModel.MessageType.Error);
            }
            else
            {
                MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
                TopicChangePopUpState = false;
            }

            Update();
        });

        public Models.RelayCommand SetTopicToDelete => new(obj =>
        {
            if (obj == null)
            {
                MainViewModel.Message("LoadError", MessageViewModel.MessageType.Error);
                return;
            }

            _topicToDo = (int) obj;
            TopicDeletePopUpState = true;
        });

        public RelayCommand ConfirmTopicDelete => new(() =>
        {
            Task.Run(() =>
            {
                if (_topicToDo == -1 || !TopicContext.Delete(_topicToDo))
                {
                    MainViewModel.Message("DeleteError", MessageViewModel.MessageType.Error);
                }
                else
                {
                    MainViewModel.Message("DeleteSuccess", MessageViewModel.MessageType.Error);
                    Task.Run(Update);
                }

                TopicDeletePopUpState = false;
            }).ConfigureAwait(false);
        });

        public RelayCommand CreateTopic => new RelayCommand(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.TopicCreate);
        });

        public RelayCommand BackToMain => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Main);
        });

        public RelayCommand Back => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
        });

        public Models.RelayCommand ChangePage => new(obj =>
        {
            Task.Run(() =>
            {
                if (obj == null)
                {
                    MainViewModel.Message("LoadError", MessageViewModel.MessageType.Error);
                    return;
                }

                _currentPage = (int)obj;
                Update();
            }).ConfigureAwait(false);
        });

        public RelayCommand LastPage => new(() =>
        {
            Task.Run(() =>
            {
                var temp = (TopicContext.GetTopicCount(Page.forumId, Page.userId) / (double)_takeAmount);
                if (temp == 0)
                {
                    MainViewModel.Message("LoadError", MessageViewModel.MessageType.Error);
                    return;
                }

                var topicCount = temp % 1 == 0 ? temp : temp + 1;

                _currentPage = (int)topicCount;
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

        public TopicListViewModel()
        {
            _currentPage = 1;
            Page = this;
        }

        //set forumId -2 to Update for user
        public static void SetUpdate(int forumId, int userId = -1)
        {
            Page.forumId = forumId;
            Page.userId = userId;
        }

        public void BeforeUpdate()
        {
            Page = this;
        }

        public static void Update()
        {
            if (Page.forumId == -1 || Page == null || Page.forumId==-1 && Page.userId ==-1) 
                MainViewModel.Message("UpdateError", MessageViewModel.MessageType.Error);

            ForumId = Page.forumId;
            var temp = (TopicContext.GetTopicCount(Page.forumId, Page.userId) / (double) _takeAmount);
            var topicCount = temp%1==0?temp:temp+1;
            var pageNumbers = new List<int>();
            for (var i = Page._currentPage - 3; i < Page._currentPage + 3; i++)
            {
                if (i > 0 && i <= topicCount)
                {
                    pageNumbers.Add(i);
                }
            }

            if (pageNumbers.Count > 4)
            {
                Page.FirstLastVisibility = Visibility.Visible;
            }
            else
            {
                Page.FirstLastVisibility = Visibility.Collapsed;
            }

            if (User.IsLogin && !User.IsBanned && ForumId > 0)
                Page.ButtonsVisibility = Visibility.Visible;
            else
                Page.ButtonsVisibility = Visibility.Collapsed;

            var list = TopicContext.GetTopicList(Page.forumId, Page.userId, Page._currentPage,_takeAmount);
            if (list == null) return;
            Page.Pages = new ObservableCollection<CounterPageViewModel>(pageNumbers.Select(x => new CounterPageViewModel(x,Page._currentPage)));
            Page.Topics = new ObservableCollection<TopicViewModel>(list.Select(x => new TopicViewModel(x)));
        }

        public static void Clear()
        {
            Page = null;
        }
    }
}
