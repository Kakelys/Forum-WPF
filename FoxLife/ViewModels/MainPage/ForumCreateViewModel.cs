using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.Forum;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.User;
using FoxLife.View;
using Microsoft.Toolkit.Mvvm.Input;

namespace FoxLife.ViewModels.MainPage
{
    internal class ForumCreateViewModel:ViewModelBase
    {
        private bool _canAddForum = false;
        private string _forumName;
        private byte[]? imgData = null;
        private int _sectionId;
        public Visibility ForumNameVisibility { get; private set; } = Visibility.Visible;
        private static ForumCreateViewModel Page;

        public string ForumName
        {
            get => _forumName;
            set
            {
                if (value == "")
                {
                    ForumNameVisibility = Visibility.Visible;
                    _canAddForum = false;
                }
                else
                {
                    ForumNameVisibility = Visibility.Hidden;
                    _canAddForum = true;
                }

                _forumName = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand LoadImage => new(() =>
        {
            var temp = ImgHelp.OpenImage();
            if (temp == null)
            {
                MainViewModel.Message("ImageLoadError", MessageViewModel.MessageType.Error);
                return;
            }

            imgData = temp;
        });

        public RelayCommand AddTopicConfirm => new RelayCommand(() =>
        {
            if (imgData==null)
            {
                MainViewModel.Message("NoImageError", MessageViewModel.MessageType.Error);
                return;
            }

            if (string.IsNullOrEmpty(ForumName))
            {
                MainViewModel.Message("MaxSectionNameError", MessageViewModel.MessageType.Error);
                return;
            }

            if (ForumName.Length > 50)
            {
                MainViewModel.Message("EmptyForumNameError", MessageViewModel.MessageType.Error);
                return;
            }

            try
            {
                var topic = new ForumDb(ForumName, _sectionId);
                Task.Run(() =>
                {
                    if (!ForumContext.Add(topic, imgData)) 
                        MainViewModel.Message("CreateTopicError", MessageViewModel.MessageType.Error);

                    MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
                    MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
                }).ConfigureAwait(false);
            }
            catch
            {
                MainViewModel.Message("CreateTopicInternetError", MessageViewModel.MessageType.Error);
            }
        }, CanAddTopic);

        public RelayCommand Back => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
        });

        public ForumCreateViewModel()
        {
            Page = this;
        }

        public static void SetSectionId(int id)
        {
            Page._sectionId = id;
        }

        private bool CanAddTopic()
        {
            return _canAddForum;
        }

        public void BeforeUpdate()
        {
            Page = this;
        }

        public static void Update()
        {
            if (User.RoleId is > 2 or < 0)
            {
                MainViewModel.Message("NoRightsError", MessageViewModel.MessageType.Error);
                MainViewModel.ChangePage(MainViewModel.PagesEnum.Main);
            }
        }
    }
}
