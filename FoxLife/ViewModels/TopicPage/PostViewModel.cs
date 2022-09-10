using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.Post;
using FoxLife.Models.DBInfo.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Mvvm.Input;
using FoxLife.Models;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;


namespace FoxLife.ViewModels.TopicPage
{
    internal class PostViewModel : ViewModelBase
    {
        public PostDb Post { get; }
        public bool AdditionallyPopUpState { get; set; } = false;

        public ObservableCollection<PostViewModel> SubPosts { get; set; }

        public int Id => Post.Id;
        public int SenderId => Post.SenderId;
        public string UserName => Post.UserDb.Name;
        public string RoleName => Post.UserDb.RoleObj.RoleName;
        public string Message => Post.MsgTxt;
        public string MsgTime => Post.MsgTime.ToString(2);
        public ImageSource Img { get; set; }

        public bool IsExpanded { get; set; }

        public Visibility ButtonEdit { get; set; } = Visibility.Collapsed;
        public Visibility ButtonDelete { get; set; } = Visibility.Collapsed;
        public Visibility ButtonAnswer { get; set; } = Visibility.Collapsed;

        public RelayCommand OpenAdditionally => new(() =>
        {
            if(User.IsLogin &&( ButtonEdit==Visibility.Visible
                            || ButtonDelete == Visibility.Visible
                            || ButtonAnswer == Visibility.Visible))
                AdditionallyPopUpState = true;
        });

        public RelayCommand DeletePost => new(() =>
        {
            if (!PostContext.Delete(Id))
            {
                MainViewModel.Message("DeleteError", MessageViewModel.MessageType.Error);
            }
            MainViewModel.Message("DeleteSuccess", MessageViewModel.MessageType.Success);
            PostListViewModel.Update();
        });

        public RelayCommand OpenProfile => new(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.UserProfile,SenderId);
        });

        public RelayCommand StartEdit => new(() =>
        {
            AdditionallyPopUpState = false;
            PostListViewModel.StartEdit(Post,false);
        });

        public RelayCommand ToggleExpander => new(() =>
        {
            IsExpanded = !IsExpanded;
        });

        public PostViewModel(PostDb post, bool isClosed)
        {
            Post = post;

            if (!User.IsLogin)
            {
                return;
            }

            if (isClosed || User.IsBanned) return;

            ButtonAnswer = Visibility.Visible;

            if (Post.SenderId == User.Id)
            {
                ButtonAnswer = Visibility.Collapsed;
                ButtonEdit = Visibility.Visible;
                ButtonDelete = Visibility.Visible;
            }
            else
            {
                ButtonEdit = Visibility.Collapsed;
                if (User.RoleId is >= 0 and <= 1)
                    ButtonDelete = Visibility.Visible;
                else
                    ButtonDelete = Visibility.Collapsed;
            }
        }

        public Models.RelayCommand Answer => new(obj =>
        {
            if (obj == null || obj is not PostDb )
            {
                MainViewModel.Message("AnswerError", MessageViewModel.MessageType.Error);
                return;
            }

            PostListViewModel.Answer(Post.UserDb.Name,Post.Id);
            AdditionallyPopUpState = false;
        });
    }
}
