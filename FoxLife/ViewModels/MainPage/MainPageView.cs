using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using FoxLife.Models.DBInfo.Forum;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.SectionInfo;
using FoxLife.Models.DBInfo.User;
using Microsoft.Toolkit.Mvvm.Input;


namespace FoxLife.ViewModels.MainPage
{
    internal class MainPageView : ViewModelBase
    {
        public ObservableCollection<SectionViewModel> Sections { get; set; }
        public static bool CanUpdate { get; private set; } = false;
        public static MainPageView?  Page { get; private set; }

        //Add section/topic buttons
        public Visibility ButtonsVisibility { get; private set; } = Visibility.Collapsed;

        public Visibility DeleteVisibility { get; private set; } = Visibility.Collapsed;


        private bool _forumDeletePopUpState = false;
        private int _idForumToDo = -1;

        private bool _forumChangePopUpState = false;
        public string? NewForumName { get; set; }
        //private byte[]? NewImgData { get; set; }

        private bool _sectionDeletePopUpState = false;
        private int _idSectionToDo =-1;
        private bool _sectionChangePopUpState = false;
        public string? NewSectionName { get; set; }

        #region SectionAdd
        public Visibility SectionNameVisibility { get; private set; } = Visibility.Visible;
        public bool AddSectionPopUpState { get; set; } = false;
        private string _sectionName;
        private bool _canAddSection = false;
        #endregion


        public bool SectionDeletePopUpState
        {
            get => _sectionDeletePopUpState;
            set
            {
                if (value == false) _idSectionToDo = -1;
                _sectionDeletePopUpState = value;

                RaisePropertyChanged();
            }
        }

        public bool SectionChangePopUpState
        {
            get => _sectionChangePopUpState;
            set
            {
                if (value == false)
                {
                    NewSectionName = null;
                }
                _sectionChangePopUpState = value;

                RaisePropertyChanged();
            }
        }

        public bool ForumDeletePopUpState
        {
            get => _forumDeletePopUpState;
            set
            {
                if (value == false) _idForumToDo = -1;
                _forumDeletePopUpState = value;

                RaisePropertyChanged();
            }
        }

        public bool ForumChangePopUpState
        {
            get => _forumChangePopUpState;
            set
            {
                if (value == false)
                {
                    NewForumName = null;
                }
                _forumChangePopUpState = value;

                RaisePropertyChanged();
            }
        }

        public string SectionName
        {
            get => _sectionName;
            set
            {
                if (value == "")
                {
                    SectionNameVisibility = Visibility.Visible;
                    _canAddSection = false;
                }
                else
                {
                    SectionNameVisibility = Visibility.Hidden;
                    _canAddSection = true;
                }

                _sectionName = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand ConfirmForumDelete => new(() =>
        {
            Task.Run(() =>
            {
                if (_idForumToDo == -1 || !ForumContext.Delete(_idForumToDo))
                {
                    MainViewModel.Message("DeleteError", MessageViewModel.MessageType.Error);
                }
                else
                {
                    MainViewModel.Message("DeleteSuccess", MessageViewModel.MessageType.Success);
                    Update();
                }
                ForumDeletePopUpState = false;
            }).ConfigureAwait(false);
        });

        public Models.RelayCommand SetForumToDelete => new(obj =>
        {
            if (obj == null)
            {
                MainViewModel.Message("FailedError", MessageViewModel.MessageType.Error);
                return;
            }

            ForumDeletePopUpState = true;
            _idForumToDo = (int) obj;
        });

        public Models.RelayCommand SetForumToChange => new(obj =>
        {
            if (obj == null)
            {
                MainViewModel.Message("FailedError", MessageViewModel.MessageType.Error);
                return;
            }

            var forum = obj as ForumDb;

            ForumChangePopUpState = true;
            _idForumToDo = forum.Id;
            NewForumName = forum.Name;
        });

        public RelayCommand ChoosePictureToChange => new(() =>
        {
            var NewImgData = ImgHelp.OpenImage();
            Task.Run(() =>
            {
                if (NewImgData == null || _idForumToDo == -1)
                {
                    MainViewModel.Message("NoImageError", MessageViewModel.MessageType.Error);
                    NewImgData = null;
                    return;
                }

                if (!ForumContext.Update(_idForumToDo, null, NewImgData))
                {
                    MainViewModel.Message("UpdateImageError", MessageViewModel.MessageType.Error);
                }

                
                Update();
            }).ConfigureAwait(false);
        });

        public RelayCommand ConfirmChangeForum => new(() =>
        {
            Task.Run(() =>
            {
                if (string.IsNullOrEmpty(NewForumName))
                {
                    MainViewModel.Message("EmptyForumNameError", MessageViewModel.MessageType.Error);
                    return;
                }

                if (NewForumName.Length > 50)
                {
                    MainViewModel.Message("MaxSectionNameError", MessageViewModel.MessageType.Error);
                    return;
                }

                if (_idForumToDo == -1 || !ForumContext.Update(_idForumToDo, NewForumName, null))
                {
                    MainViewModel.Message("ChangeError", MessageViewModel.MessageType.Error);
                    ForumChangePopUpState = false;
                    return;
                }
                else
                {
                    MainViewModel.Message("Success", MessageViewModel.MessageType.Success);
                }


                ForumChangePopUpState = false;
                Update();
            }).ConfigureAwait(false);
        });

        public Models.RelayCommand SetSectionToDelete => new(obj =>
        {
            if (obj == null)
            {
                MainViewModel.Message("FailedError", MessageViewModel.MessageType.Error);
                return;
            }

            SectionDeletePopUpState = true;
            _idSectionToDo = (int)obj;
        });

        public Models.RelayCommand SetSectionToChange => new(obj =>
        {
            if (obj == null)
            {
                MainViewModel.Message("FailedError", MessageViewModel.MessageType.Error);
                return;
            }

            var section = obj as SectionDb;

            SectionChangePopUpState = true;
            _idSectionToDo = section.Id;
            NewSectionName = section.Name;
        });

        public RelayCommand ConfirmChangeSection => new(() =>
        {
            Task.Run(() =>
            {
                if (string.IsNullOrEmpty(NewSectionName))
                {
                    MainViewModel.Message("EmptySectionNameError", MessageViewModel.MessageType.Error);
                    return;
                }
                if (NewSectionName.Length > 50)
                {
                    MainViewModel.Message("MaxSectionNameError", MessageViewModel.MessageType.Error);
                    return;
                }

                if (_idSectionToDo == -1 || !SectionContext.Update(_idSectionToDo,NewSectionName))
                {
                    MainViewModel.Message("ChangeError", MessageViewModel.MessageType.Error);
                    SectionChangePopUpState = false;
                    return;
                }
                else
                {
                    MainViewModel.Message("Success", MessageViewModel.MessageType.Error);
                }


                SectionChangePopUpState = false;
                Update();
            }).ConfigureAwait(false);
        });

        public RelayCommand ConfirmSectionDelete => new(() =>
        {
            Task.Run(() =>
            {
                if (_idSectionToDo == -1 || !SectionContext.Delete(_idSectionToDo))
                {
                    MainViewModel.Message("DeleteError", MessageViewModel.MessageType.Error);
                }
                else
                {
                    MainViewModel.Message("DeleteSuccess", MessageViewModel.MessageType.Success);
                    Update();
                }
                SectionDeletePopUpState = false;
            }).ConfigureAwait(false);
        });

        public MainPageView()
        {
            CanUpdate = true;
            Page = this;

            Page.UpdateButtons();
        }

        public void BeforeUpdate()
        {
            Page = this;
        }

        public static void Update()
        {
            if (!CanUpdate) MainViewModel.Message("UpdateError", MessageViewModel.MessageType.Error);
            if (Page == null) return;
            Page.UpdateButtons();

            var list = SectionContext.GetSections();
            if (list == null) return;
            Page.Sections = new ObservableCollection<SectionViewModel>(list.Select(x => new SectionViewModel(x)));
            Task.Run(() =>
            {
                Page.Sections.AsParallel().
                    ForAll(s=>s.Topics.AsParallel()
                        .ForAll(t=>t.Img=ImgHelp.LoadImage(t.ForumDb.Img.Img)));
            });
        }

        public static void Clear()
        {
            if (!CanUpdate) return;
            if (Page != null)
                Page = null;
            CanUpdate = false;
        }

        private void UpdateButtons()
        {
            if (!User.IsLogin || User.IsBanned)
            {
                DeleteVisibility = Visibility.Collapsed;
                ButtonsVisibility = Visibility.Collapsed;

                return;
            }

            if (User.RoleId == 0 && !User.IsBanned)
                DeleteVisibility = Visibility.Visible;
            else
                DeleteVisibility = Visibility.Collapsed;

            if (User.RoleId < 2 && User.RoleId > -1)
                ButtonsVisibility = Visibility.Visible;
            else
                ButtonsVisibility = Visibility.Collapsed;
        }

        public RelayCommand AddSection => new RelayCommand(() =>
        {
            Page.AddSectionPopUpState = true;
        });

        public RelayCommand AddSectionConfirm => new RelayCommand(() =>
        {
            Page.AddSectionPopUpState = false;
            if (string.IsNullOrEmpty(SectionName))
            {
                MainViewModel.Message("EmptySectionNameError", MessageViewModel.MessageType.Error);
                return;
            }
            if (SectionName.Length>50)
            {
                MainViewModel.Message("MaxSectionNameError", MessageViewModel.MessageType.Error);
                return;
            }

            var section = new SectionDb(SectionName);
            if (!SectionContext.Add(section))
            {
                MainViewModel.Message("SectionAddInternetError", MessageViewModel.MessageType.Error);
                return;
            }

            MainViewModel.UpdateCurrentPage();

        }, CanAddSection);

        private static bool CanAddSection()
        {
            if (Page == null) return false;
            return Page._canAddSection;
        }
    }
}
