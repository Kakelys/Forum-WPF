using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using FoxLife.Models;
using FoxLife.Models.DBInfo.SectionInfo;
using FoxLife.Models.DBInfo.Forum;
using Microsoft.Toolkit.Mvvm.Input;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace FoxLife.ViewModels.MainPage
{
    internal class ForumViewModel : ViewModelBase
    {
        public ForumDb ForumDb { get; }

        //Forum info
        public int Id => ForumDb.Id;
        public string Name => ForumDb.Name;
        public int CountOfTopics => ForumDb.CountOfTopics;
        public int CountOfMsg => ForumDb.CountOfMsg;
        public ImageSource Img { get; set; }

        //last topic info
        public string TopicText { get; set; } = null;
        public int LastId { get; set; }
        public string LastName { get; set; }
        public string LastUserName { get; set; }
        public int LastUserId => ForumDb.LastMsgUsrId;
        public string LastTopicTime { get; set; }

        public ForumViewModel(ForumDb forumDb)
        {
            ForumDb = forumDb;
            if (ForumDb.LastTopicDb != null)
            {
                TopicText = $"{Application.Current.Resources["TopicText"]}:";
                LastId = ForumDb.LastTopicDb.Id;
                LastName = ForumDb.LastTopicDb.Name;
                LastUserName = ForumDb.LastTopicDb.LastUserDb.Name;
                LastTopicTime = ForumDb.LastTopicDb.LastMsgTime.ToString(1);
            }
        }

        public RelayCommand OpenTopic => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.TopicList, Id);
        });

        public RelayCommand OpenLastTopic => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Topic, LastId);
        });

        public RelayCommand OpenLastProfile => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.UserProfile,LastUserId);
        });
    }
}
