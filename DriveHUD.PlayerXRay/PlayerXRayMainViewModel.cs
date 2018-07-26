//-----------------------------------------------------------------------
// <copyright file="AppStoreViewModel.cs" company="Ace Poker Solutions">
// Copyright © 2017 Ace Poker Solutions. All Rights Reserved.
// Unless otherwise noted, all materials contained in this Site are copyrights, 
// trademarks, trade dress and/or other intellectual properties, owned, 
// controlled or licensed by Ace Poker Solutions and may not be used without 
// written consent except as provided in these terms and conditions or in the 
// copyright notice (documents and software) or other proprietary notices 
// provided with the relevant materials.
// </copyright>
//----------------------------------------------------------------------

using DriveHUD.Common.Infrastructure.CustomServices;
using DriveHUD.Common.Linq;
using DriveHUD.Common.Resources;
using DriveHUD.Common.Utils;
using DriveHUD.Common.Wpf.Mvvm;
using DriveHUD.PlayerXRay.BusinessHelper.ApplicationSettings;
using DriveHUD.PlayerXRay.Events;
using DriveHUD.PlayerXRay.Licensing;
using DriveHUD.PlayerXRay.Services;
using DriveHUD.PlayerXRay.ViewModels;
using DriveHUD.PlayerXRay.ViewModels.PopupViewModels;
using DriveHUD.PlayerXRay.Views;
using DriveHUD.PlayerXRay.Views.PopupViews;
using Microsoft.Practices.ServiceLocation;
using Model;
using Prism.Events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DriveHUD.PlayerXRay
{
    public class PlayerXRayMainViewModel : WindowViewModelBase<PlayerXRayMainViewModel>, IPlayerXRayMainViewModel
    {
        private readonly Dictionary<WorkspaceType, WorkspaceViewModel> workspaces;

        private readonly SingletonStorageModel storageModel;

        private readonly IEventAggregator eventAggregator;

        private readonly SubscriptionToken raisePopupSubscriptionToken;

        private readonly SubscriptionToken restoreDefaultsSubscriptionToken;

        public PlayerXRayMainViewModel()
        {
            eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            workspaces = new Dictionary<WorkspaceType, WorkspaceViewModel>();
            storageModel = ServiceLocator.Current.TryResolve<SingletonStorageModel>();

            raisePopupSubscriptionToken = eventAggregator.GetEvent<RaisePopupEvent>().Subscribe(RaisePopup, false);
            restoreDefaultsSubscriptionToken = eventAggregator.GetEvent<RestoreDefaultsEvent>().Subscribe(OnRestoreDefaults, false);
        }

        public override void Configure(object viewModelInfo)
        {
            NavigateCommand = ReactiveCommand.Create<WorkspaceType>(x => Navigate(x));
            UpgradeCommand = ReactiveCommand.Create(() => Upgrade());
            ManualCommand = ReactiveCommand.Create(() => BrowserHelper.OpenLinkInBrowser(CommonResourceManager.Instance.GetResourceString("XRay_MainView_Help_ManualLink")));
            ForumCommand = ReactiveCommand.Create(() => BrowserHelper.OpenLinkInBrowser(CommonResourceManager.Instance.GetResourceString("XRay_MainView_Help_ForumLink")));
            SupportCommand = ReactiveCommand.Create(() => BrowserHelper.OpenLinkInBrowser(CommonResourceManager.Instance.GetResourceString("XRay_MainView_Help_SupportLink")));

            StaticStorage.CurrentPlayer = StorageModel.PlayerSelectedItem?.PlayerId.ToString();
            StaticStorage.CurrentPlayerName = StorageModel.PlayerSelectedItem?.Name;

            Navigate(WorkspaceType.Run);

            OnInitialized();

            var licenseService = ServiceLocator.Current.GetInstance<ILicenseService>();

            if (licenseService.IsTrial ||
                     (licenseService.IsRegistered && licenseService.IsExpiringSoon) ||
                     !licenseService.IsRegistered)
            {
                RaiseRegistrationPopup(false);
                return;
            }

            IsUpgradable = licenseService.IsUpgradable;
            IsTrial = licenseService.IsTrial;
        }

        #region Properties        

        private Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(PlayerXRayMainViewModel));
            }
        }

        public Version Version
        {
            get
            {
                return Assembly.GetName().Version;
            }
        }

        public DateTime BuildDate
        {
            get
            {
                try
                {
                    var fileInfo = new FileInfo(Assembly.Location);
                    return fileInfo.LastWriteTimeUtc;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        public SingletonStorageModel StorageModel
        {
            get
            {
                return storageModel;
            }
        }

        private WorkspaceViewModel workspace;

        public WorkspaceViewModel Workspace
        {
            get
            {
                return workspace;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref workspace, value);
            }
        }

        private string popupTitle;

        public string PopupTitle
        {
            get
            {
                return popupTitle;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref popupTitle, value);
            }
        }

        private object popupContent;

        public object PopupContent
        {
            get
            {
                return popupContent;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref popupContent, value);
            }
        }

        private bool popupIsOpen;

        public bool PopupIsOpen
        {
            get
            {
                return popupIsOpen;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref popupIsOpen, value);
            }

        }

        public string LicenseType
        {
            get
            {
                var licenseService = ServiceLocator.Current.GetInstance<ILicenseService>();

                IEnumerable<string> licenseStrings;

                if (licenseService.LicenseInfos.Any(x => x.IsRegistered && !x.IsTrial))
                {
                    licenseStrings = licenseService.LicenseInfos.Where(x => x.IsRegistered && !x.IsTrial).Select(x => x.License.Edition);
                }
                else
                {
                    licenseStrings = licenseService.LicenseInfos.Where(x => x.IsRegistered).Select(x => x.License.Edition);
                }

                if (licenseStrings == null || licenseStrings.Count() == 0)
                {
                    return CommonResourceManager.Instance.GetResourceString("XRay_LicenseType_None");
                }

                return string.Join(" & ", licenseStrings);
            }
        }

        private bool isTrial;

        public bool IsTrial
        {
            get
            {
                return isTrial;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref isTrial, value);
            }
        }

        private bool isUpgradable;

        public bool IsUpgradable
        {
            get
            {
                return isUpgradable;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref isUpgradable, value);
            }
        }

        #endregion

        #region Commands

        public ReactiveCommand NavigateCommand { get; private set; }

        public ReactiveCommand UpgradeCommand { get; private set; }

        public ReactiveCommand ManualCommand { get; private set; }

        public ReactiveCommand ForumCommand { get; private set; }

        public ReactiveCommand SupportCommand { get; private set; }

        #endregion        

        #region IDisposable implementation

        protected override void Disposing()
        {
            var playerNotesService = ServiceLocator.Current.GetInstance<IPlayerNotesService>() as IPlayerXRayNoteService;
            playerNotesService.SaveAppSettings();

            eventAggregator.GetEvent<RaisePopupEvent>().Unsubscribe(raisePopupSubscriptionToken);
            eventAggregator.GetEvent<RestoreDefaultsEvent>().Unsubscribe(restoreDefaultsSubscriptionToken);

            workspaces.Values.ForEach(workspace => workspace.Dispose());

            base.Disposing();
        }

        #endregion

        #region Infrastructure      

        private void Navigate(WorkspaceType workspaceType)
        {
            if (Workspace != null)
            {
                Workspace.IsClosing = true;
            }

            if (workspaces.ContainsKey(workspaceType))
            {
                Workspace = workspaces[workspaceType];
                Workspace.IsClosing = false;
                return;
            }

            WorkspaceViewModel workspace = null;

            switch (workspaceType)
            {
                case WorkspaceType.Run:
                    workspace = new RunViewModel();
                    break;
                case WorkspaceType.Notes:
                    workspace = new NotesViewModel();
                    break;
                case WorkspaceType.Profiles:
                    workspace = new ProfilesViewModel();
                    break;
                case WorkspaceType.Settings:
                    workspace = new SettingsViewModel();
                    break;
                case WorkspaceType.Help:
                    workspace = new HelpViewModel();
                    break;
            }

            if (workspace == null)
            {
                return;
            }

            workspaces.Add(workspaceType, workspace);
            Workspace = workspace;
        }

        private bool isClosing = false;

        public override bool OnClosing()
        {
            if (isClosing)
            {
                return true;
            }

            var confirmationViewModel = new YesNoConfirmationViewModel
            {
                ConfirmationMessage = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_ExitText"),
                OnYesAction = () =>
                {
                    isClosing = true;
                    OnClosed();
                }
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_Exit"),
                Content = new YesNoConfirmationView(confirmationViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);

            return isClosing;
        }

        private void Upgrade()
        {
            RaiseRegistrationPopup(true);
        }

        private void OnRestoreDefaults(RestoreDefaultsEventArgs e)
        {
            var selectedType = Workspace.WorkspaceType;

            if (workspaces != null)
            {
                workspaces.Values.ForEach(x => x.Dispose());
                workspaces.Clear();
            }

            Navigate(selectedType);
        }

        private void RaiseRegistrationPopup(bool showRegister)
        {
            var registrationViewModel = new RegistrationViewModel(showRegister)
            {
                Callback = () =>
                {
                    var licenseService = ServiceLocator.Current.GetInstance<ILicenseService>();

                    IsTrial = licenseService.IsTrial;
                    IsUpgradable = licenseService.IsUpgradable;

                    this.RaisePropertyChanged(nameof(LicenseType));
                }
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_RegistrationView_Title"),
                Content = new RegistrationView(registrationViewModel)
            };

            RaisePopup(popupEventArgs);
        }

        private void RaisePopup(RaisePopupEventArgs e)
        {
            var containerView = e.Content as IPopupContainerView;

            if (containerView != null && containerView.ViewModel != null)
            {
                containerView.ViewModel.FinishInteraction = () => ClosePopup();
            }

            PopupTitle = e.Title;
            PopupContent = containerView;
            PopupIsOpen = true;
        }

        private void ClosePopup()
        {
            PopupIsOpen = false;
            PopupContent = null;
            PopupTitle = null;
        }

        #endregion
    }
}