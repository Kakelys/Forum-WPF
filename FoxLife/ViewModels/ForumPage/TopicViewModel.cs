using System;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.Topic;
using FoxLife.Models.DBInfo.User;
using Microsoft.Toolkit.Mvvm.Input;
using FoxLife.Models;

using FoxLife.Models.DBInfo.Img;
using MaterialDesignThemes.Wpf;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace FoxLife.ViewModels.ForumPage
{
    internal class TopicViewModel:ViewModelBase
    {
        public TopicDb TopicDb { get; }

        public int Id => TopicDb.Id;
        public string Name => TopicDb.Name;
        public int CountOfMsg => TopicDb.CountOfMsg;
        public string LastMsgTime => TopicDb.LastMsgTime.ToString(1);
        public int LastMsgUsrId => TopicDb.LastMsgUsrId;
        public string LastMsgUsrName { get; private set; }
        public int ForumId => TopicDb.ForumId;
        public int AuthorId { get; private set; }
        public string AuthorName { get; private set; }
        public ImageSource LastMsgImg { get; set; }

        public Visibility LockVisibility { get; set; }
        public Visibility PinVisibility { get; set; }

        public Visibility ChangeableVisibility { get; set; } = Visibility.Collapsed;

        public RelayCommand OpenTopicClick => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Topic, TopicDb.Id);
        });
        public RelayCommand OpenUserProfileClick => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.UserProfile, AuthorId);
        });

        public Models.RelayCommand OpenLastProfile => new(obj =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.UserProfile, LastMsgUsrId);
        });

        public TopicViewModel(TopicDb topicDb)
        {
            TopicDb = topicDb;
            
            AuthorId = topicDb.StartUserDb.Id;
            AuthorName = $"{Application.Current.Resources["AuthorText"]}: {topicDb.StartUserDb.Name}";

            LastMsgUsrName = topicDb.LastUserDb.Name;

            LockVisibility = TopicDb.IsClosed ? Visibility.Visible : Visibility.Collapsed;
            PinVisibility = TopicDb.IsPinned ? Visibility.Visible : Visibility.Collapsed;

            Task.Run(() => { LastMsgImg = ImgHelp.LoadImage(TopicDb.LastUserDb.Avatar.Img); }).ConfigureAwait(false);

            if (!User.IsLogin) return;

            if (AuthorId == User.Id || User.RoleId is >= 0 and <= 2)
                ChangeableVisibility = Visibility.Visible;
        }
    }
}
