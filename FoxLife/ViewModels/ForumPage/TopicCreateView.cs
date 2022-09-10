using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.Topic;
using FoxLife.Models.DBInfo.User;
using Microsoft.Toolkit.Mvvm.Input;

namespace FoxLife.ViewModels.ForumPage
{
    internal class TopicCreateView : ViewModelBase
    {
        private string _name;
        private string _message;

        public string Name
        {
            get => _name;
            set
            {
                NamePlaceholder = string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Hidden;
                _name = value;
                RaisePropertyChanged();
            }
        }

        public Visibility NamePlaceholder { get; set; } = Visibility.Visible;

        public string Message
        {
            get => _message;
            set
            {
                MessagePlaceholder = string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Hidden;
                _message = value;
                RaisePropertyChanged();
            }
        }

        public Visibility MessagePlaceholder { get; set; } = Visibility.Visible;

        public RelayCommand Confirm => new RelayCommand(() =>
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

                if (string.IsNullOrEmpty(Name))
                {
                    MainViewModel.Message("EmptyNameFiledError", MessageViewModel.MessageType.Error);
                    return;
                }

                if (Name.Length > 50)
                {
                    MainViewModel.Message("MaxSectionNameError", MessageViewModel.MessageType.Error);
                    return;
                }

                if (!TopicContext.Add(new TopicDb(Name, Message, TopicListViewModel.ForumId)))
                {
                    MainViewModel.Message("CreateTopicError", MessageViewModel.MessageType.Error);
                    return;
                }

                MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
            }).ConfigureAwait(false);
        });

        public RelayCommand Back => new RelayCommand(() =>
        {
            MainViewModel.ChangePage(MainViewModel.PagesEnum.Last);
        });

        public static void Update()
        {
            if (!User.IsLogin)
            {
                MainViewModel.Message("NoRightsError", MessageViewModel.MessageType.Error);
                MainViewModel.ChangePage(MainViewModel.PagesEnum.Main);
            }
        }
    }
}
