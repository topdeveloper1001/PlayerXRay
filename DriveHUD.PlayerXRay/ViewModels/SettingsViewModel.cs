//-----------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="Ace Poker Solutions">
// Copyright © 2017 Ace Poker Solutions. All Rights Reserved.
// Unless otherwise noted, all materials contained in this Site are copyrights, 
// trademarks, trade dress and/or other intellectual properties, owned, 
// controlled or licensed by Ace Poker Solutions and may not be used without 
// written consent except as provided in these terms and conditions or in the 
// copyright notice (documents and software) or other proprietary notices 
// provided with the relevant materials.
// </copyright>
//----------------------------------------------------------------------

using DriveHUD.Common.Resources;
using DriveHUD.PlayerXRay.BusinessHelper.ApplicationSettings;
using DriveHUD.PlayerXRay.Events;
using DriveHUD.PlayerXRay.Services;
using DriveHUD.PlayerXRay.ViewModels.PopupViewModels;
using DriveHUD.PlayerXRay.Views.PopupViews;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ReactiveUI;
using System;
using System.Linq;

namespace DriveHUD.PlayerXRay.ViewModels
{
    public class SettingsViewModel : WorkspaceViewModel
    {
        private readonly IEventAggregator eventAggregator;

        private readonly SubscriptionToken refreshSettingsSubscriptionToken;

        public SettingsViewModel()
        {
            eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            DeleteAllNotesCommand = ReactiveCommand.Create();
            DeleteAllNotesCommand.Subscribe(x => DeleteNotes(null));

            DeleteNotesCommand = ReactiveCommand.Create();
            DeleteNotesCommand.Subscribe(x => DeleteNotes(OlderThanDate));

            RestoreDefaultsCommand = ReactiveCommand.Create();
            RestoreDefaultsCommand.Subscribe(x => RestoreDefaults());

            olderThanDate = DateTime.Today;

            profiles = new ReactiveList<AutoNoteProfile>();

            RefreshSettings();

            refreshSettingsSubscriptionToken = eventAggregator.GetEvent<RefreshSettingsEvent>().Subscribe(x => RefreshSettings());
        }

        private void RefreshSettings()
        {
            profiles.Clear();

            profiles.Add(
                new AutoNoteProfile(CommonResourceManager.Instance.GetResourceString("XRay_SettingsView_AllNotes"))
                {
                    IsAll = true
                }
            );

            NoteService.CurrentNotesAppSettings.Profiles.ForEach(x => profiles.Add(new AutoNoteProfile(x.Name)));

            selectedProfile = profiles.FirstOrDefault(x => x.Name == NoteService.CurrentNotesAppSettings.AutoNoteProfile ||
                (string.IsNullOrEmpty(NoteService.CurrentNotesAppSettings.AutoNoteProfile) && x.IsAll));
        }

        #region Properties 

        public bool AutoNotesEnabled
        {
            get
            {
                return NoteService.CurrentNotesAppSettings.AutoNotesEnabled;
            }
            set
            {
                if (NoteService.CurrentNotesAppSettings.AutoNotesEnabled == value)
                {
                    return;
                }

                NoteService.CurrentNotesAppSettings.AutoNotesEnabled = value;

                this.RaisePropertyChanged(nameof(AutoNotesEnabled));

                NoteService.SaveAppSettings();
            }
        }

        public bool TakesNotesOnHero
        {
            get
            {
                return NoteService.CurrentNotesAppSettings.TakesNotesOnHero;
            }
            set
            {
                if (NoteService.CurrentNotesAppSettings.TakesNotesOnHero == value)
                {
                    return;
                }

                NoteService.CurrentNotesAppSettings.TakesNotesOnHero = value;

                this.RaisePropertyChanged(nameof(TakesNotesOnHero));

                NoteService.SaveAppSettings();
            }
        }

        public bool IsAdvancedLogEnabled
        {
            get
            {
                return NoteService.CurrentNotesAppSettings.IsAdvancedLogEnabled;
            }
            set
            {
                if (NoteService.CurrentNotesAppSettings.IsAdvancedLogEnabled == value)
                {
                    return;
                }

                NoteService.CurrentNotesAppSettings.IsAdvancedLogEnabled = value;

                this.RaisePropertyChanged(nameof(IsAdvancedLogEnabled));

                NoteService.SaveAppSettings();
            }
        }

        public bool IsNoteCreationSinceDate
        {
            get
            {
                return NoteService.CurrentNotesAppSettings.IsNoteCreationSinceDate;
            }
            set
            {
                if (NoteService.CurrentNotesAppSettings.IsNoteCreationSinceDate == value)
                {
                    return;
                }

                NoteService.CurrentNotesAppSettings.IsNoteCreationSinceDate = value;

                this.RaisePropertyChanged(nameof(IsNoteCreationSinceDate));

                NoteService.SaveAppSettings();
            }
        }

        public DateTime NoteCreationSinceDate
        {
            get
            {
                return NoteService.CurrentNotesAppSettings.NoteCreationSinceDate;
            }
            set
            {
                if (NoteService.CurrentNotesAppSettings.NoteCreationSinceDate == value)
                {
                    return;
                }

                NoteService.CurrentNotesAppSettings.NoteCreationSinceDate = value;

                this.RaisePropertyChanged(nameof(NoteCreationSinceDate));

                NoteService.SaveAppSettings();
            }
        }

        private DateTime olderThanDate;

        public DateTime OlderThanDate
        {
            get
            {
                return olderThanDate;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref olderThanDate, value);
            }
        }

        private double progress;

        public double Progress
        {
            get
            {
                return progress;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref progress, value);
            }
        }

        private bool progressEnabled;

        public bool ProgressEnabled
        {
            get
            {
                return progressEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref progressEnabled, value);
            }
        }

        public override WorkspaceType WorkspaceType
        {
            get
            {
                return WorkspaceType.Settings;
            }
        }

        private ReactiveList<AutoNoteProfile> profiles;

        public ReactiveList<AutoNoteProfile> Profiles
        {
            get
            {
                return profiles;
            }
        }

        private AutoNoteProfile selectedProfile;

        public AutoNoteProfile SelectedProfile
        {
            get
            {
                return selectedProfile;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedProfile, value);

                NoteService.CurrentNotesAppSettings.AutoNoteProfile = !selectedProfile.IsAll ?
                    selectedProfile.Name :
                    null;

                NoteService.SaveAppSettings();
            }
        }

        #endregion

        #region Commands

        public ReactiveCommand<object> DeleteAllNotesCommand { get; private set; }

        public ReactiveCommand<object> DeleteNotesCommand { get; private set; }

        public ReactiveCommand<object> RestoreDefaultsCommand { get; private set; }

        #endregion

        #region Infrastructure

        private void RestoreDefaults()
        {
            var confirmationViewModel = new YesNoConfirmationViewModel
            {
                ConfirmationMessage = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_RestoreDefaultsText")
            };

            confirmationViewModel.OnYesAction = () =>
            {
                NoteService.InitializeDefaultNotes();
                eventAggregator.GetEvent<RestoreDefaultsEvent>().Publish(new RestoreDefaultsEventArgs());
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_RestoreDefaults"),
                Content = new YesNoConfirmationView(confirmationViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        private void DeleteNotes(DateTime? date)
        {
            var confirmationViewModel = new YesNoConfirmationViewModel
            {
                ConfirmationMessage = date.HasValue ?
                    string.Format(CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_DeleteNotesMessage"), date.Value) :
                    CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_DeleteAllNotesMessage")
            };

            confirmationViewModel.OnYesAction = () =>
            {
                StartAsyncOperation(() =>
                {
                    Progress = 0;
                    ProgressEnabled = true;

                    var noteProcessingService = ServiceLocator.Current.GetInstance<INoteProcessingService>();

                    noteProcessingService.ProgressChanged += (s, e) =>
                    {
                        Progress = e.Progress;
                    };

                    noteProcessingService.DeletesNotes(date);

                }, () => { ProgressEnabled = false; Progress = 0; });
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_DeleteTitle"),
                Content = new YesNoConfirmationView(confirmationViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        #endregion

        #region IDisposable

        protected override void Disposing()
        {
            base.Disposing();

            if (refreshSettingsSubscriptionToken != null)
            {
                eventAggregator.GetEvent<RefreshSettingsEvent>().Unsubscribe(refreshSettingsSubscriptionToken);
            }
        }

        #endregion

        #region Helpers 

        public class AutoNoteProfile : ReactiveObject
        {
            public AutoNoteProfile(string name)
            {
                Name = name;
            }

            public string Name
            {
                get;
                set;
            }

            public bool IsAll
            {
                get;
                set;
            }
        }

        #endregion
    }
}