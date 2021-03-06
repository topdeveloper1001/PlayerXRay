﻿//-----------------------------------------------------------------------
// <copyright file="ProfilesViewModel.cs" company="Ace Poker Solutions">
// Copyright © 2017 Ace Poker Solutions. All Rights Reserved.
// Unless otherwise noted, all materials contained in this Site are copyrights, 
// trademarks, trade dress and/or other intellectual properties, owned, 
// controlled or licensed by Ace Poker Solutions and may not be used without 
// written consent except as provided in these terms and conditions or in the 
// copyright notice (documents and software) or other proprietary notices 
// provided with the relevant materials.
// </copyright>
//----------------------------------------------------------------------

using DriveHUD.Common.Linq;
using DriveHUD.Common.Resources;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects;
using DriveHUD.PlayerXRay.Events;
using DriveHUD.PlayerXRay.ViewModels.PopupViewModels;
using DriveHUD.PlayerXRay.Views.PopupViews;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ReactiveUI;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace DriveHUD.PlayerXRay.ViewModels
{
    public class ProfilesViewModel : WorkspaceViewModel
    {
        private readonly IEventAggregator eventAggregator;

        public ProfilesViewModel()
        {
            eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            profiles = new ReactiveList<ProfileObject>(NoteService.CurrentNotesAppSettings.Profiles);
            profiles.Changed.Subscribe(x =>
            {
                if (x.Action == NotifyCollectionChangedAction.Add && x.NewItems != null)
                {
                    var addedItems = x.NewItems.OfType<ProfileObject>();

                    if (addedItems != null)
                    {
                        NoteService.CurrentNotesAppSettings.Profiles.AddRange(addedItems);
                    }
                }
                else if (x.Action == NotifyCollectionChangedAction.Remove && x.OldItems != null)
                {
                    var removedItems = x.OldItems.OfType<ProfileObject>();

                    if (removedItems != null)
                    {
                        removedItems.ForEach(p => NoteService.CurrentNotesAppSettings.Profiles.Remove(p));
                    }
                }
            });

            selectedProfileNotes = new ReactiveList<NoteObject>();
            selectedProfileNotes.Changed.Subscribe(x =>
            {
                if (x.Action == NotifyCollectionChangedAction.Add && x.NewItems != null)
                {
                    var addedItems = x.NewItems.OfType<NoteObject>();

                    if (addedItems != null)
                    {
                        addedItems.ForEach(p =>
                        {
                            if (!SelectedProfile.ContainingNotes.Contains(p.ID))
                            {
                                SelectedProfile.ContainingNotes.Add(p.ID);
                            }
                        });
                    }
                }
                else if (x.Action == NotifyCollectionChangedAction.Remove && x.OldItems != null)
                {
                    var removedItems = x.OldItems.OfType<NoteObject>();

                    if (removedItems != null)
                    {
                        removedItems.ForEach(p => SelectedProfile.ContainingNotes.Remove(p.ID));
                    }
                }
            });

            stages = new ReactiveList<StageObject>(NoteService.CurrentNotesAppSettings.StagesList);

            InitializeCommands();
        }

        #region Properties

        private ReactiveList<StageObject> stages;

        public ReactiveList<StageObject> Stages
        {
            get
            {
                return stages;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref stages, value);
            }
        }

        private NoteTreeObjectBase selectedStage;

        public NoteTreeObjectBase SelectedStage
        {
            get
            {
                return selectedStage;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedStage, value);
                SelectedNote = SelectedStage as NoteObject;
            }
        }

        private NoteObject selectedNote;

        public NoteObject SelectedNote
        {
            get
            {
                return selectedNote;
            }
            private set
            {

                this.RaiseAndSetIfChanged(ref selectedNote, value);
            }
        }

        private NoteObject selectedProfileNote;

        public NoteObject SelectedProfileNote
        {
            get
            {
                return selectedProfileNote;
            }
            set
            {

                this.RaiseAndSetIfChanged(ref selectedProfileNote, value);
            }
        }

        private ReactiveList<NoteObject> selectedProfileNotes;

        public ReactiveList<NoteObject> SelectedProfileNotes
        {
            get
            {
                return selectedProfileNotes;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref selectedProfileNotes, value);
            }
        }

        private ReactiveList<ProfileObject> profiles;

        public ReactiveList<ProfileObject> Profiles
        {
            get
            {
                return profiles;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref profiles, value);
            }
        }

        private ProfileObject selectedProfile;

        public ProfileObject SelectedProfile
        {
            get
            {
                return selectedProfile;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedProfile, value);

                selectedProfileNotes.Clear();

                if (selectedProfile != null)
                {
                    NoteService.CurrentNotesAppSettings.AllNotes.ForEach(x =>
                    {
                        if (selectedProfile.ContainingNotes.Contains(x.ID))
                        {
                            selectedProfileNotes.Add(x);
                        }
                    });
                }
            }
        }

        public override WorkspaceType WorkspaceType
        {
            get
            {
                return WorkspaceType.Profiles;
            }
        }

        #endregion

        #region Commands

        public ReactiveCommand AddProfileCommand { get; private set; }

        public ReactiveCommand EditProfileCommand { get; private set; }

        public ReactiveCommand RemoveProfileCommand { get; private set; }

        public ReactiveCommand AddToSelectedNotesCommand { get; private set; }

        public ReactiveCommand RemoveFromSelectedNotesCommand { get; private set; }

        #endregion

        private void InitializeCommands()
        {
            AddProfileCommand = ReactiveCommand.Create(AddProfile);

            var canEdit = this.WhenAny(x => x.SelectedProfile, x => x.Value != null);

            EditProfileCommand = ReactiveCommand.Create(EditProfile, canEdit);
            RemoveProfileCommand = ReactiveCommand.Create(RemoveProfile, canEdit);

            var canAddToSelectedNotes = this.WhenAny(x => x.SelectedNote, x => x.Value != null);

            AddToSelectedNotesCommand = ReactiveCommand.Create(() =>
            {
                var existingNote = selectedProfileNotes.FirstOrDefault(p => p.ID == SelectedNote.ID);

                if (existingNote != null)
                {
                    return;
                }

                selectedProfileNotes.Add(SelectedNote);
                NoteService.SaveAppSettings();
            }, canAddToSelectedNotes);

            var canRemoveFromSelectedNotes = this.WhenAny(x => x.SelectedProfileNote, x => x.Value != null);

            RemoveFromSelectedNotesCommand = ReactiveCommand.Create(() =>
            {
                selectedProfileNotes.Remove(SelectedProfileNote);
                NoteService.SaveAppSettings();
            }, canRemoveFromSelectedNotes);
        }

        private void AddProfile()
        {
            var addNoteViewModel = new AddEditNoteViewModel();

            addNoteViewModel.OnSaveAction = () =>
            {
                var profile = new ProfileObject
                {
                    Name = addNoteViewModel.Name,
                };

                profiles.Add(profile);

                SelectedProfile = profile;

                NoteService.SaveAppSettings();

                eventAggregator.GetEvent<RefreshSettingsEvent>().Publish(new RefreshSettingsEventArgs());
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_AddEditNoteView_AddProfileTitle"),
                Content = new AddEditNoteView(addNoteViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        private void EditProfile()
        {
            var addNoteViewModel = new AddEditNoteViewModel
            {
                Name = SelectedProfile.Name
            };

            addNoteViewModel.OnSaveAction = () =>
            {
                if (NoteService.CurrentNotesAppSettings.AutoNoteProfile == SelectedProfile.Name)
                {
                    NoteService.CurrentNotesAppSettings.AutoNoteProfile = addNoteViewModel.Name;
                }

                SelectedProfile.Name = addNoteViewModel.Name;

                NoteService.SaveAppSettings();

                eventAggregator.GetEvent<RefreshSettingsEvent>().Publish(new RefreshSettingsEventArgs());
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_AddEditNoteView_EditProfileTitle"),
                Content = new AddEditNoteView(addNoteViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        private void RemoveProfile()
        {
            var confirmationViewModel = new YesNoConfirmationViewModel
            {
                ConfirmationMessage = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_DeleteItemMessage")
            };

            confirmationViewModel.OnYesAction = () =>
            {
                profiles.Remove(SelectedProfile);
                SelectedProfile = profiles.FirstOrDefault();
                NoteService.SaveAppSettings();
                eventAggregator.GetEvent<RefreshSettingsEvent>().Publish(new RefreshSettingsEventArgs());
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_DeleteTitle"),
                Content = new YesNoConfirmationView(confirmationViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }
    }
}