//-----------------------------------------------------------------------
// <copyright file="NotesViewModel.cs" company="Ace Poker Solutions">
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
using DriveHUD.Common.Reflection;
using DriveHUD.Common.Resources;
using DriveHUD.Common.Wpf.AttachedBehaviors;
using DriveHUD.PlayerXRay.DataTypes;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects.ActionsObjects;
using DriveHUD.PlayerXRay.Events;
using DriveHUD.PlayerXRay.ViewModels.PopupViewModels;
using DriveHUD.PlayerXRay.Views.PopupViews;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;

namespace DriveHUD.PlayerXRay.ViewModels
{
    public class NotesViewModel : WorkspaceViewModel
    {
        private readonly IEventAggregator eventAggregator;

        public NotesViewModel()
        {
            eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            InitializeStages();
            InitializeHoleCardsCollection();
            InitializeCommands();
            InitializeActions();
            InitializeFilters();
            InitializeHandValues();
        }

        private void InitializeStages()
        {
            stages = new ReactiveList<StageObject>();
            ReloadStages();
        }

        private void InitializeActions()
        {
            var actions = Enum.GetValues(typeof(ActionTypeEnum)).Cast<ActionTypeEnum>();

            firstActions = new ObservableCollection<ActionTypeEnum>(actions.Where(x => x != ActionTypeEnum.Fold));
            secondActions = new ObservableCollection<ActionTypeEnum>(actions);
            thirdActions = new ObservableCollection<ActionTypeEnum>(actions);
            fourthActions = new ObservableCollection<ActionTypeEnum>(actions);
        }

        private void InitializeFilters()
        {
            filters = new ObservableCollection<FilterObject>(FiltersHelper.GetFiltersObjects());
            filtersCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(filters);
            filtersCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FilterObject.Stage)));

            selectedFilters = new ReactiveList<FilterObject>();
            selectedFilters.Changed.Subscribe(x =>
            {
                if (SelectedNote == null || SelectedNote.Settings == null)
                {
                    return;
                }

                if (x.Action == NotifyCollectionChangedAction.Add)
                {
                    var addedItems = x.NewItems.OfType<FilterObject>();

                    if (addedItems != null)
                    {
                        foreach (var addedItem in addedItems)
                        {
                            if (!SelectedNote.Settings.SelectedFilters.Any(f => f.Filter == addedItem.Filter))
                            {
                                SelectedNote.Settings.SelectedFilters.Add(addedItem);
                            }
                        }
                    }

                }
                else if (x.Action == NotifyCollectionChangedAction.Remove)
                {
                    var removedItems = x.OldItems.OfType<FilterObject>();

                    if (removedItems != null)
                    {
                        foreach (var removeItem in removedItems)
                        {
                            var itemToRemove = SelectedNote.Settings
                                .SelectedFilters
                                .FirstOrDefault(f => f.Filter == removeItem.Filter);

                            if (itemToRemove != null)
                            {
                                SelectedNote.Settings.SelectedFilters.Remove(itemToRemove);
                            }
                        }
                    }
                }

                RaiseFilterBasedPropertyChanged();
            });

            selectedFiltersCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(selectedFilters);
            selectedFiltersCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FilterObject.Stage)));
        }

        private void InitializeCommands()
        {
            var canAdd = this.WhenAny(x => x.SelectedStage, x => x.Value != null);

            AddNoteCommand = ReactiveCommand.Create(AddNote, canAdd);

            var canEdit = this.WhenAny(x => x.SelectedStage, x => x.Value != null && x.Value is NoteTreeEditableObject);

            EditNoteCommand = ReactiveCommand.Create(EditNote, canEdit);

            var canRemove = this.WhenAny(x => x.SelectedStage, x => (x.Value is InnerGroupObject) || (x.Value is NoteObject));

            RemoveNoteCommand = ReactiveCommand.Create(RemoveNote, canRemove);
            SwitchModeCommand = ReactiveCommand.Create(() => IsAdvancedMode = !IsAdvancedMode);

            HoleCardsLeftClickCommand = ReactiveCommand.Create<HoleCardsViewModel>(x => x.IsChecked = true);
            HoleCardsDoubleLeftClickCommand = ReactiveCommand.Create<HoleCardsViewModel>(x => x.IsChecked = false);
            HoleCardsMouseEnterCommand = ReactiveCommand.Create<HoleCardsViewModel>(x => x.IsChecked = true);
            HoleCardsSelectAllCommand = ReactiveCommand.Create(() => HoleCardsCollection.ForEach(x => x.IsChecked = true));
            HoleCardsSelectNoneCommand = ReactiveCommand.Create(() => HoleCardsCollection.ForEach(x => x.IsChecked = false));
            HoleCardsSelectSuitedGappersCommand = ReactiveCommand.Create(SelectSuitedGappers);
            HoleCardsSelectSuitedConnectorsCommand = ReactiveCommand.Create(SelectedSuitedConnectors);
            HoleCardsSelectPocketPairsCommand = ReactiveCommand.Create(SelectPocketPairs);
            HoleCardsSelectOffSuitedGappersCommand = ReactiveCommand.Create(SelectOffSuitedGappers);
            HoleCardsSelectOffSuitedConnectorsCommand = ReactiveCommand.Create(SelectOffSuitedConnectors);

            AddToSelectedFiltersCommand = ReactiveCommand.Create(() =>
            {
                var selectedItem = filters.FirstOrDefault(f => f.IsSelected);

                if (selectedItem != null && !selectedFilters.Any(f => f.Filter == selectedItem.Filter))
                {
                    var filterToAdd = selectedItem.Clone();
                    filterToAdd.IsSelected = false;

                    // if filter requires value
                    if (FiltersHelper.FiltersWithValueRequired.Contains(filterToAdd.Filter))
                    {
                        var setFilterValueViewModel = new SetFilterValueViewModel
                        {
                            Filter = filterToAdd.Filter
                        };

                        setFilterValueViewModel.OnSaveAction = () =>
                        {
                            filterToAdd.Value = setFilterValueViewModel.FilterValue;
                            selectedFilters.Add(filterToAdd);
                        };

                        var popupEventArgs = new RaisePopupEventArgs()
                        {
                            Title = CommonResourceManager.Instance.GetResourceString("XRay_SetFilterValueView_Title"),
                            Content = new SetFilterValueView(setFilterValueViewModel)
                        };

                        eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
                    }
                    else
                    {
                        selectedFilters.Add(filterToAdd);
                    }
                }
            });

            RemoveFromSelectedFiltersCommand = ReactiveCommand.Create(() =>
            {
                var selectedItem = selectedFilters.FirstOrDefault(f => f.IsSelected);

                if (selectedItem != null)
                {
                    selectedFilters.Remove(selectedItem);
                }
            });

            NoteDragDropCommand = ReactiveCommand.Create<DragDropDataObject>(dataObject =>
           {
               if (dataObject == null)
               {
                   return;
               }

               var note = dataObject.DropData as NoteObject;

               if (note == null || ReferenceEquals(note, dataObject.Source))
               {
                   return;
               }

               var noteParent = FindNoteParent(note);

               if (dataObject.Source is InnerGroupObject)
               {
                   (dataObject.Source as InnerGroupObject).Notes.Add(note);
               }
               else if (dataObject.Source is NoteObject)
               {
                   var sourceNoteParent = FindNoteParent(dataObject.Source as NoteObject);

                   if (ReferenceEquals(sourceNoteParent, noteParent))
                   {
                       return;
                   }

                   if (sourceNoteParent is InnerGroupObject)
                   {
                       (sourceNoteParent as InnerGroupObject).Notes.Add(note);
                   }
                   else if (sourceNoteParent is StageObject)
                   {
                       (sourceNoteParent as StageObject).Notes.Add(note);
                   }
               }

               // remove note from parent object
               if (noteParent is InnerGroupObject)
               {
                   (noteParent as InnerGroupObject).Notes.Remove(note);
               }
               else if (noteParent is StageObject)
               {
                   (noteParent as StageObject).Notes.Remove(note);
               }
           });
        }

        private NoteTreeObjectBase FindNoteParent(NoteObject note)
        {
            foreach (var stage in Stages)
            {
                if (stage.Notes.Any(x => ReferenceEquals(x, note)))
                {
                    return stage;
                }

                foreach (var group in stage.InnerGroups)
                {
                    if (group.Notes.Any(x => ReferenceEquals(x, note)))
                    {
                        return group;
                    }
                }
            }

            return null;
        }

        private void InitializeHoleCardsCollection()
        {
            HoleCardsCollection = new ReactiveList<HoleCardsViewModel>();

            var rankValues = HandHistories.Objects.Cards.Card.PossibleRanksHighCardFirst;

            for (int i = 0; i < rankValues.Length; i++)
            {
                var startS = false;

                for (int j = 0; j < rankValues.Length; j++)
                {
                    var card1 = i < j ? rankValues.ElementAt(i) : rankValues.ElementAt(j);
                    var card2 = i < j ? rankValues.ElementAt(j) : rankValues.ElementAt(i);

                    if (startS)
                    {
                        HoleCardsCollection.Add(new HoleCardsViewModel
                        {
                            Name = $"{card1}{card2}s",
                            ItemType = RangeSelectorItemType.Suited,
                            IsChecked = true
                        });
                    }
                    else
                    {
                        if (!card1.Equals(card2))
                        {
                            HoleCardsCollection.Add(new HoleCardsViewModel
                            {
                                Name = $"{card1}{card2}o",
                                ItemType = RangeSelectorItemType.OffSuited,
                                IsChecked = true
                            });
                        }
                        else
                        {
                            HoleCardsCollection.Add(new HoleCardsViewModel
                            {
                                Name = $"{card1}{card2}",
                                ItemType = RangeSelectorItemType.Pair,
                                IsChecked = true
                            });

                            startS = true;
                        }
                    }
                }
            }

            HoleCardsCollection.ChangeTrackingEnabled = true;
            HoleCardsCollection.ItemChanged
                .Where(x => x.PropertyName == nameof(HoleCardsViewModel.IsChecked))
                .Select(x => x.Sender)
                .Subscribe(x =>
                {
                    if (SelectedNote != null && SelectedNote.Settings != null)
                    {
                        var excludedCardsList = SelectedNote.Settings.ExcludedCardsList;

                        if (!x.IsChecked && !excludedCardsList.Contains(x.Name))
                        {
                            excludedCardsList.Add(x.Name);
                            SelectedNote.Settings.ExcludedCardsList = excludedCardsList;
                        }
                        else if (x.IsChecked && excludedCardsList.Contains(x.Name))
                        {
                            excludedCardsList.Remove(x.Name);
                            SelectedNote.Settings.ExcludedCardsList = excludedCardsList;
                        }
                    }
                });
        }

        private void InitializeHandValues()
        {
            flopHandValues = new ReactiveList<HandValueObject>(HandValuesHelper.GetHandValueObjects(NoteStageType.Flop));
            SubscribeOnHandValuesChanges(flopHandValues, ReflectionHelper.GetPath<NotesViewModel>(x => x.SelectedNote.Settings.FlopHvSettings.SelectedHv));

            flopFlushDrawHandValues = new ReactiveList<HandValueObject>(HandValuesHelper.GetFlushHandValueObjects(NoteStageType.Flop));
            SubscribeOnHandValuesChanges(flopFlushDrawHandValues, ReflectionHelper.GetPath<NotesViewModel>(x => x.SelectedNote.Settings.FlopHvSettings.SelectedFlushDraws));

            flopStraightDrawHandValues = new ReactiveList<HandValueObject>(HandValuesHelper.GetStraightHandValueObjects());
            SubscribeOnHandValuesChanges(flopStraightDrawHandValues, ReflectionHelper.GetPath<NotesViewModel>(x => x.SelectedNote.Settings.FlopHvSettings.SelectedStraighDraws));

            turnHandValues = new ReactiveList<HandValueObject>(HandValuesHelper.GetHandValueObjects(NoteStageType.Turn));
            SubscribeOnHandValuesChanges(turnHandValues, ReflectionHelper.GetPath<NotesViewModel>(x => x.SelectedNote.Settings.TurnHvSettings.SelectedHv));

            turnFlushDrawHandValues = new ReactiveList<HandValueObject>(HandValuesHelper.GetFlushHandValueObjects(NoteStageType.Turn));
            SubscribeOnHandValuesChanges(turnFlushDrawHandValues, ReflectionHelper.GetPath<NotesViewModel>(x => x.SelectedNote.Settings.TurnHvSettings.SelectedFlushDraws));

            turnStraightDrawHandValues = new ReactiveList<HandValueObject>(HandValuesHelper.GetStraightHandValueObjects());
            SubscribeOnHandValuesChanges(turnStraightDrawHandValues, ReflectionHelper.GetPath<NotesViewModel>(x => x.SelectedNote.Settings.TurnHvSettings.SelectedStraighDraws));

            riverHandValues = new ReactiveList<HandValueObject>(HandValuesHelper.GetHandValueObjects(NoteStageType.River));
            SubscribeOnHandValuesChanges(riverHandValues, ReflectionHelper.GetPath<NotesViewModel>(x => x.SelectedNote.Settings.RiverHvSettings.SelectedHv));
        }

        private void SubscribeOnHandValuesChanges(ReactiveList<HandValueObject> source, string valuePath)
        {
            source.ChangeTrackingEnabled = true;
            source.ItemChanged
                .Where(x => x.PropertyName == nameof(HandValueObject.IsSelected))
                .Subscribe(x =>
                {
                    var collectionToChange = (ICollection<int>)ReflectionHelper.GetMemberValue(this, valuePath);

                    if (collectionToChange == null)
                    {
                        return;
                    }

                    if (x.Sender.IsSelected)
                    {
                        if (!collectionToChange.Contains(x.Sender.Value))
                        {
                            collectionToChange.Add(x.Sender.Value);
                        }

                        return;
                    }

                    collectionToChange.Remove(x.Sender.Value);
                });
        }

        /// <summary>
        /// Reloads <see cref="Stages"/> collection accordingly to selected <see cref="NoteStageType"/>
        /// </summary>
        private void ReloadStages()
        {
            Stages?.ForEach(x => x.IsSelected = false);

            Stages?.Clear();

            Stages?.AddRange(NoteService
                .CurrentNotesAppSettings
                .StagesList
                .Where(x => x.StageType == NoteStageType));
        }

        private NoteObject noteCopy;

        private void LoadNote()
        {
            if (SelectedNote != null)
            {
                HoleCardsCollection.ForEach(x => x.IsChecked = !SelectedNote.Settings.ExcludedCardsList.Contains(x.Name));
            }

            noteCopy = SelectedNote?.CopyTo();

            RefreshCurrentActionSettings();
            RefreshFiltersSettings();
            RefreshCurrentHandValuesSettings();

            RaiseFilterBasedPropertyChanged();
        }

        private void RaiseFilterBasedPropertyChanged()
        {
            this.RaisePropertyChanged(nameof(MBCMinSizeOfPot));
            this.RaisePropertyChanged(nameof(MBCMaxSizeOfPot));
            this.RaisePropertyChanged(nameof(MBCWentToShowdown));
            this.RaisePropertyChanged(nameof(MBCAllInPreFlop));
        }

        private void SaveNote()
        {
            NoteService.SaveAppSettings();
        }

        private void RefreshFiltersSettings()
        {
            if (SelectedNote == null || SelectedNote.Settings == null)
            {
                return;
            }

            selectedFilters.Clear();
            SelectedNote.Settings.SelectedFilters.ForEach(x =>
            {
                x.IsSelected = false;
                selectedFilters.Add(x);
            });
        }

        private void RefreshCurrentActionSettings()
        {
            if (SelectedNote == null || SelectedNote.Settings == null)
            {
                return;
            }

            switch (ActionStageType)
            {
                case NoteStageType.PreFlop:
                    CurrentActionSettings = SelectedNote.Settings.PreflopActions;
                    return;
                case NoteStageType.Flop:
                    CurrentActionSettings = SelectedNote.Settings.FlopActions;
                    return;
                case NoteStageType.Turn:
                    CurrentActionSettings = SelectedNote.Settings.TurnActions;
                    return;
                case NoteStageType.River:
                    CurrentActionSettings = SelectedNote.Settings.RiverActions;
                    return;
            }
        }

        private void RefreshCurrentHandValuesSettings()
        {
            if (SelectedNote == null || SelectedNote.Settings == null || SelectedNote.Settings.FlopHvSettings == null)
            {
                return;
            }

            SetIsSelectedHandValues(FlopHandValues, SelectedNote.Settings.FlopHvSettings.SelectedHv);
            SetIsSelectedHandValues(FlopFlushDrawHandValues, SelectedNote.Settings.FlopHvSettings.SelectedFlushDraws);
            SetIsSelectedHandValues(FlopStraightDrawHandValues, SelectedNote.Settings.FlopHvSettings.SelectedStraighDraws);
            SetIsSelectedHandValues(TurnHandValues, SelectedNote.Settings.TurnHvSettings.SelectedHv);
            SetIsSelectedHandValues(TurnFlushDrawHandValues, SelectedNote.Settings.TurnHvSettings.SelectedFlushDraws);
            SetIsSelectedHandValues(TurnStraightDrawHandValues, SelectedNote.Settings.TurnHvSettings.SelectedStraighDraws);
            SetIsSelectedHandValues(RiverHandValues, SelectedNote.Settings.RiverHvSettings.SelectedHv);

            this.RaisePropertyChanged(nameof(FlopAnyHandValue));
            this.RaisePropertyChanged(nameof(FlopAnyFlushDrawsHandValue));
            this.RaisePropertyChanged(nameof(FlopAnyStraightDrawsHandValue));
            this.RaisePropertyChanged(nameof(TurnAnyHandValue));
            this.RaisePropertyChanged(nameof(TurnAnyFlushDrawsHandValue));
            this.RaisePropertyChanged(nameof(TurnAnyStraightDrawsHandValue));
            this.RaisePropertyChanged(nameof(RiverAnyHandValue));
        }

        private static void SetIsSelectedHandValues(IEnumerable<HandValueObject> source, IEnumerable<int> selectedValues)
        {
            if (selectedValues == null || source == null)
            {
                return;
            }

            source.ForEach(x => x.IsSelected = selectedValues.Contains(x.Value));
        }

        #region Properties

        public override WorkspaceType WorkspaceType
        {
            get
            {
                return WorkspaceType.Notes;
            }
        }

        private bool isAdvancedMode;

        public bool IsAdvancedMode
        {
            get
            {
                return isAdvancedMode;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref isAdvancedMode, value);
            }
        }

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

        private NoteStageType noteStageType;

        public NoteStageType NoteStageType
        {
            get
            {
                return noteStageType;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref noteStageType, value);
                ReloadStages();
            }
        }

        private NoteStageType actionStageType;

        public NoteStageType ActionStageType
        {
            get
            {
                return actionStageType;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref actionStageType, value);
                RefreshCurrentActionSettings();
            }
        }

        private ReactiveList<HoleCardsViewModel> holeCardsCollection;

        public ReactiveList<HoleCardsViewModel> HoleCardsCollection
        {
            get
            {
                return holeCardsCollection;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref holeCardsCollection, value);
            }
        }

        private ActionTypeEnum firstAction;

        public ActionTypeEnum FirstAction
        {
            get
            {
                return firstAction;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref firstAction, value);
            }
        }

        private ObservableCollection<ActionTypeEnum> firstActions;

        public ObservableCollection<ActionTypeEnum> FirstActions
        {
            get
            {
                return firstActions;
            }
        }

        private ActionTypeEnum secondAction;

        public ActionTypeEnum SecondAction
        {
            get
            {
                return secondAction;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref secondAction, value);
            }
        }

        private ObservableCollection<ActionTypeEnum> secondActions;

        public ObservableCollection<ActionTypeEnum> SecondActions
        {
            get
            {
                return secondActions;
            }
        }

        private ActionTypeEnum thirdAction;

        public ActionTypeEnum ThirdAction
        {
            get
            {
                return thirdAction;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref thirdAction, value);
            }
        }

        private ObservableCollection<ActionTypeEnum> thirdActions;

        public ObservableCollection<ActionTypeEnum> ThirdActions
        {
            get
            {
                return thirdActions;
            }
        }

        private ActionTypeEnum fourthAction;

        public ActionTypeEnum FourthAction
        {
            get
            {
                return fourthAction;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref fourthAction, value);

            }
        }

        private ObservableCollection<ActionTypeEnum> fourthActions;

        public ObservableCollection<ActionTypeEnum> FourthActions
        {
            get
            {
                return fourthActions;
            }
        }

        private ObservableCollection<FilterObject> filters;

        private CollectionView filtersCollectionView;

        public CollectionView FiltersCollectionView
        {
            get
            {
                return filtersCollectionView;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref filtersCollectionView, value);
            }
        }

        private ReactiveList<FilterObject> selectedFilters;

        private CollectionView selectedFiltersCollectionView;

        public CollectionView SelectedFiltersCollectionView
        {
            get
            {
                return selectedFiltersCollectionView;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedFiltersCollectionView, value);
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
                NoteService.CurrentNotesAppSettings.AllNotes.Where(x => x.Modified).ForEach(x => Console.WriteLine(x.Name));

                if (selectedNote != null && selectedNote.Modified && !IsClosing)
                {
                    ShowPendingChangedPopup(value);
                    return;
                }

                this.RaiseAndSetIfChanged(ref selectedNote, value);
                LoadNote();
            }
        }

        private void ShowPendingChangedPopup(NoteObject noteObject)
        {
            var confirmationViewModel = new YesNoConfirmationViewModel
            {
                ConfirmationMessage = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_SavePendingChangesText"),
                OnYesAction = () =>
                {
                    if (selectedNote != null)
                    {
                        selectedNote.Modified = false;
                    }

                    SaveNote();

                    SelectedNote = noteObject;
                },
                OnNoAction = () =>
                {
                    if (selectedNote != null)
                    {
                        noteCopy?.CopyTo(selectedNote);
                        selectedNote.Modified = false;
                    }

                    SelectedNote = noteObject;
                }
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_SavePendingChanges"),
                Content = new YesNoConfirmationView(confirmationViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        public double MBCMinSizeOfPot
        {
            get
            {
                return SelectedNote != null ? SelectedNote.Settings.MBCMinSizeOfPot : 0d;
            }
            set
            {
                if (value != 0)
                {
                    AddFilterItem(FilterEnum.FinalPotSizeinBBsisBiggerThan, value);
                }
                else
                {
                    RemoveFilterItem(FilterEnum.FinalPotSizeinBBsisBiggerThan);
                }

                this.RaisePropertyChanged();
            }
        }

        public double MBCMaxSizeOfPot
        {
            get
            {
                return SelectedNote != null ? SelectedNote.Settings.MBCMaxSizeOfPot : 0d;
            }
            set
            {
                if (value != 0)
                {
                    AddFilterItem(FilterEnum.FinalPotSizeinBBsisLessThan, value);
                }
                else
                {
                    RemoveFilterItem(FilterEnum.FinalPotSizeinBBsisLessThan);
                }

                this.RaisePropertyChanged();
            }
        }

        public bool MBCWentToShowdown
        {
            get
            {
                return SelectedNote != null ? SelectedNote.Settings.MBCWentToShowdown : false;
            }
            set
            {
                if (value)
                {
                    AddFilterItem(FilterEnum.SawShowdown);
                }
                else
                {
                    RemoveFilterItem(FilterEnum.SawShowdown);
                }

                this.RaisePropertyChanged();
            }
        }

        public bool MBCAllInPreFlop
        {
            get
            {
                return SelectedNote != null ? SelectedNote.Settings.MBCAllInPreFlop : false;
            }
            set
            {
                if (value)
                {
                    AddFilterItem(FilterEnum.AllinPreflop);
                }
                else
                {
                    RemoveFilterItem(FilterEnum.AllinPreflop);
                }

                this.RaisePropertyChanged();
            }
        }

        private ActionSettings currentActionSettings;

        public ActionSettings CurrentActionSettings
        {
            get
            {
                return currentActionSettings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref currentActionSettings, value);
            }
        }

        #region Hand Values


        #region Flop

        public bool FlopAnyHandValue
        {
            get
            {
                return SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.FlopHvSettings != null && SelectedNote.Settings.FlopHvSettings.AnyHv;
            }
            set
            {
                if (SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.FlopHvSettings != null)
                {
                    SelectedNote.Settings.FlopHvSettings.AnyHv = value;

                    if (value)
                    {
                        FlopHandValues.ForEach(x => x.IsSelected = false);
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private ReactiveList<HandValueObject> flopHandValues;

        public ReactiveList<HandValueObject> FlopHandValues
        {
            get
            {
                return flopHandValues;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref flopHandValues, value);
            }
        }

        public bool FlopAnyFlushDrawsHandValue
        {
            get
            {
                return SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.FlopHvSettings != null && SelectedNote.Settings.FlopHvSettings.AnyFlushDraws;
            }
            set
            {
                if (SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.FlopHvSettings != null)
                {
                    SelectedNote.Settings.FlopHvSettings.AnyFlushDraws = value;

                    if (value)
                    {
                        FlopFlushDrawHandValues.ForEach(x => x.IsSelected = false);
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private ReactiveList<HandValueObject> flopFlushDrawHandValues;

        public ReactiveList<HandValueObject> FlopFlushDrawHandValues
        {
            get
            {
                return flopFlushDrawHandValues;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref flopFlushDrawHandValues, value);
            }
        }

        public bool FlopAnyStraightDrawsHandValue
        {
            get
            {
                return SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.FlopHvSettings != null && SelectedNote.Settings.FlopHvSettings.AnyStraightDraws;
            }
            set
            {
                if (SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.FlopHvSettings != null)
                {
                    SelectedNote.Settings.FlopHvSettings.AnyStraightDraws = value;

                    if (value)
                    {
                        FlopStraightDrawHandValues.ForEach(x => x.IsSelected = false);
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private ReactiveList<HandValueObject> flopStraightDrawHandValues;

        public ReactiveList<HandValueObject> FlopStraightDrawHandValues
        {
            get
            {
                return flopStraightDrawHandValues;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref flopStraightDrawHandValues, value);
            }
        }

        #endregion

        #region Turn

        public bool TurnAnyHandValue
        {
            get
            {
                return SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.TurnHvSettings != null && SelectedNote.Settings.TurnHvSettings.AnyHv;
            }
            set
            {
                if (SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.TurnHvSettings != null)
                {
                    SelectedNote.Settings.TurnHvSettings.AnyHv = value;

                    if (value)
                    {
                        TurnHandValues.ForEach(x => x.IsSelected = false);
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private ReactiveList<HandValueObject> turnHandValues;

        public ReactiveList<HandValueObject> TurnHandValues
        {
            get
            {
                return turnHandValues;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref turnHandValues, value);
            }
        }

        public bool TurnAnyFlushDrawsHandValue
        {
            get
            {
                return SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.TurnHvSettings != null && SelectedNote.Settings.TurnHvSettings.AnyFlushDraws;
            }
            set
            {
                if (SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.TurnHvSettings != null)
                {
                    SelectedNote.Settings.TurnHvSettings.AnyFlushDraws = value;

                    if (value)
                    {
                        TurnFlushDrawHandValues.ForEach(x => x.IsSelected = false);
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private ReactiveList<HandValueObject> turnFlushDrawHandValues;

        public ReactiveList<HandValueObject> TurnFlushDrawHandValues
        {
            get
            {
                return turnFlushDrawHandValues;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref turnFlushDrawHandValues, value);
            }
        }

        public bool TurnAnyStraightDrawsHandValue
        {
            get
            {
                return SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.TurnHvSettings != null && SelectedNote.Settings.TurnHvSettings.AnyStraightDraws;
            }
            set
            {
                if (SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.TurnHvSettings != null)
                {
                    SelectedNote.Settings.TurnHvSettings.AnyStraightDraws = value;

                    if (value)
                    {
                        TurnStraightDrawHandValues.ForEach(x => x.IsSelected = false);
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private ReactiveList<HandValueObject> turnStraightDrawHandValues;

        public ReactiveList<HandValueObject> TurnStraightDrawHandValues
        {
            get
            {
                return turnStraightDrawHandValues;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref turnStraightDrawHandValues, value);
            }
        }

        #endregion

        #region River

        public bool RiverAnyHandValue
        {
            get
            {
                return SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.RiverHvSettings != null && SelectedNote.Settings.RiverHvSettings.AnyHv;
            }
            set
            {
                if (SelectedNote != null && SelectedNote.Settings != null &&
                    SelectedNote.Settings.RiverHvSettings != null)
                {
                    SelectedNote.Settings.RiverHvSettings.AnyHv = value;

                    if (value)
                    {
                        RiverHandValues.ForEach(x => x.IsSelected = false);
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private ReactiveList<HandValueObject> riverHandValues;

        public ReactiveList<HandValueObject> RiverHandValues
        {
            get
            {
                return riverHandValues;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref riverHandValues, value);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Commands

        public ReactiveCommand AddNoteCommand { get; private set; }

        public ReactiveCommand EditNoteCommand { get; private set; }

        public ReactiveCommand RemoveNoteCommand { get; private set; }

        public ReactiveCommand ExportCommand { get; private set; }

        public ReactiveCommand SwitchModeCommand { get; private set; }

        public ReactiveCommand HoleCardsLeftClickCommand { get; private set; }

        public ReactiveCommand HoleCardsDoubleLeftClickCommand { get; private set; }

        public ReactiveCommand HoleCardsMouseEnterCommand { get; private set; }

        public ReactiveCommand HoleCardsSelectSuitedGappersCommand { get; private set; }

        public ReactiveCommand HoleCardsSelectSuitedConnectorsCommand { get; private set; }

        public ReactiveCommand HoleCardsSelectPocketPairsCommand { get; private set; }

        public ReactiveCommand HoleCardsSelectOffSuitedGappersCommand { get; private set; }

        public ReactiveCommand HoleCardsSelectOffSuitedConnectorsCommand { get; private set; }

        public ReactiveCommand HoleCardsSelectAllCommand { get; private set; }

        public ReactiveCommand HoleCardsSelectNoneCommand { get; private set; }

        public ReactiveCommand AddToSelectedFiltersCommand { get; private set; }

        public ReactiveCommand RemoveFromSelectedFiltersCommand { get; private set; }

        public ReactiveCommand NoteDragDropCommand { get; private set; }

        #endregion

        #region Commands implementation

        private void SelectSuitedGappers()
        {
            var length = HandHistories.Objects.Cards.Card.PossibleRanksHighCardFirst.Length;

            for (var i = 0; i < length - 2; i++)
            {
                HoleCardsCollection.ElementAt(i * length + i + 2).IsChecked = true;
            }
        }

        private void SelectOffSuitedGappers()
        {
            var length = HandHistories.Objects.Cards.Card.PossibleRanksHighCardFirst.Length;

            for (var i = 2; i < length; i++)
            {
                HoleCardsCollection.ElementAt(i * length + i - 2).IsChecked = true;
            }
        }

        private void SelectOffSuitedConnectors()
        {
            var length = HandHistories.Objects.Cards.Card.PossibleRanksHighCardFirst.Length;

            for (int i = 1; i < length; i++)
            {
                HoleCardsCollection.ElementAt(i * length + i - 1).IsChecked = true;
            }
        }

        private void SelectedSuitedConnectors()
        {
            var length = HandHistories.Objects.Cards.Card.PossibleRanksHighCardFirst.Length;

            for (int i = 0; i < length - 1; i++)
            {
                HoleCardsCollection.ElementAt(i * length + i + 1).IsChecked = true;
            }
        }

        private void SelectPocketPairs()
        {
            var length = HandHistories.Objects.Cards.Card.PossibleRanksHighCardFirst.Length;

            for (var i = 0; i < length; i++)
            {
                HoleCardsCollection.ElementAt(i * length + i).IsChecked = true;
            }
        }

        private void AddNote()
        {
            var addNoteViewModel = new AddEditNoteViewModel
            {
                IsGroupPossible = SelectedStage is StageObject
            };

            addNoteViewModel.OnSaveAction = () =>
            {
                ReactiveList<NoteObject> noteList = null;

                if (SelectedStage is StageObject)
                {
                    if (addNoteViewModel.IsGroup)
                    {
                        var group = new InnerGroupObject
                        {
                            Name = addNoteViewModel.Name,
                            IsSelected = true
                        };

                        (SelectedStage as StageObject).InnerGroups.Add(group);
                        SaveNote();
                        return;
                    }

                    noteList = (SelectedStage as StageObject).Notes;
                }
                else if (SelectedStage is InnerGroupObject)
                {
                    noteList = (SelectedStage as InnerGroupObject).Notes;
                }
                else
                {
                    foreach (var stage in stages)
                    {
                        if (stage.Notes.Contains(SelectedStage))
                        {
                            noteList = stage.Notes;
                            break;
                        }

                        foreach (var group in stage.InnerGroups)
                        {
                            if (group.Notes.Contains(SelectedStage))
                            {
                                noteList = group.Notes;
                                break;
                            }
                        }
                    }
                }

                if (noteList == null)
                {
                    return;
                }

                var note = new NoteObject
                {
                    ID = ObjectsHelper.GetNextID(NoteService.CurrentNotesAppSettings.AllNotes),
                    ParentStageType = NoteStageType,
                    Name = addNoteViewModel.Name,
                    DisplayedNote = addNoteViewModel.Name,
                    IsSelected = true
                };

                note.TrackChanges(true);

                noteList.Add(note);

                SaveNote();
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_AddEditNoteView_AddTitle"),
                Content = new AddEditNoteView(addNoteViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        private void EditNote()
        {
            var treeEditableObject = SelectedStage as NoteTreeEditableObject;

            if (treeEditableObject == null)
            {
                return;
            }

            var addNoteViewModel = new AddEditNoteViewModel
            {
                Name = treeEditableObject.Name
            };

            addNoteViewModel.OnSaveAction = () =>
            {
                treeEditableObject.Name = addNoteViewModel.Name;
                SaveNote();
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = treeEditableObject is NoteObject ?
                    CommonResourceManager.Instance.GetResourceString("XRay_AddEditNoteView_EditNoteTitle") :
                    CommonResourceManager.Instance.GetResourceString("XRay_AddEditNoteView_EditNoteGroup"),
                Content = new AddEditNoteView(addNoteViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        private void RemoveNote()
        {
            var confirmationViewModel = new YesNoConfirmationViewModel
            {
                ConfirmationMessage = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_DeleteItemMessage")
            };

            confirmationViewModel.OnYesAction = () =>
            {
                foreach (var stage in Stages)
                {
                    if (SelectedStage is NoteObject)
                    {
                        var noteToRemove = SelectedStage as NoteObject;

                        stage.Notes.Remove(noteToRemove);

                        foreach (var group in stage.InnerGroups)
                        {
                            group.Notes.Remove(noteToRemove);
                        }

                        noteToRemove.TrackChanges(false);
                    }
                    else if (SelectedStage is InnerGroupObject)
                    {
                        var groupToRemove = SelectedStage as InnerGroupObject;

                        stage.InnerGroups.Remove(groupToRemove);
                    }

                    SaveNote();
                }
            };

            var popupEventArgs = new RaisePopupEventArgs()
            {
                Title = CommonResourceManager.Instance.GetResourceString("XRay_YesNoConfirmationView_DeleteTitle"),
                Content = new YesNoConfirmationView(confirmationViewModel)
            };

            eventAggregator.GetEvent<RaisePopupEvent>().Publish(popupEventArgs);
        }

        #endregion

        #region Helpers

        private void AddFilterItem(FilterEnum filter, double? filterValue = null)
        {
            var selectedfilterItem = selectedFilters.FirstOrDefault(x => x.Filter == filter);

            if (selectedfilterItem == null)
            {
                var filterItem = filters.FirstOrDefault(x => x.Filter == filter);

                if (filterItem != null)
                {
                    var selectedFilterItem = filterItem.Clone();

                    if (filterValue.HasValue)
                    {
                        selectedFilterItem.Value = filterValue;
                    }

                    selectedFilters.Add(selectedFilterItem);
                }
            }
            else if (filterValue != null)
            {
                selectedfilterItem.Value = filterValue;
            }
        }

        private void RemoveFilterItem(FilterEnum filter)
        {
            var selectedfilterItem = selectedFilters.FirstOrDefault(x => x.Filter == filter);

            if (selectedfilterItem != null)
            {
                selectedFilters.Remove(selectedfilterItem);
            }
        }

        #endregion
    }
}