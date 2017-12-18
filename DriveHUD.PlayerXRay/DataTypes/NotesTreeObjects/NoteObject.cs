//-----------------------------------------------------------------------
// <copyright file="NoteObject.cs" company="Ace Poker Solutions">
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
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects.ActionsObjects;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects.TextureObjects;
using ReactiveUI;
using System;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects
{
    public class NoteObject : NoteTreeEditableObject
    {
        public NoteObject()
        {
            Settings = new NoteSettingsObject();
            DisplayedNote = "Unknown";
        }

        private int id;

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref id, value);
            }
        }

        private string description;

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref description, value);
            }
        }

        private string displayedNote;

        public string DisplayedNote
        {
            get
            {
                return displayedNote;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref displayedNote, value);
            }
        }

        private NoteSettingsObject settings;

        public NoteSettingsObject Settings
        {
            get
            {
                return settings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref settings, value);
            }
        }

        private bool modified;

        [XmlIgnore]
        public bool Modified
        {
            get
            {
                return modified;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref modified, value);
            }
        }

        private CompositeDisposable disposableList = new CompositeDisposable();

        // initialize notes to track changes
        public void TrackChanges(bool enabled)
        {
            if (!enabled)
            {
                disposableList.Dispose();
                disposableList = new CompositeDisposable();
                return;
            }

            disposableList.Add(
                Changed
                    .Where(x => x.PropertyName != nameof(Modified) && x.PropertyName != nameof(IsSelected))
                    .Subscribe(x => SetModified()));

            if (Settings == null)
            {
                return;
            }

            disposableList.Add(
                Settings
                    .Changed
                    .Subscribe(x => SetModified()));

            if (Settings.SelectedFilters != null)
            {
                disposableList.Add(
                    Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => Settings.SelectedFilters.CollectionChanged += h,
                        h => Settings.SelectedFilters.CollectionChanged -= h).Subscribe(x => SetModified()));
            }

            if (Settings.SelectedFiltersComparison != null)
            {
                disposableList.Add(
                    Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => Settings.SelectedFiltersComparison.CollectionChanged += h,
                        h => Settings.SelectedFiltersComparison.CollectionChanged -= h).Subscribe(x => SetModified()));
            }

            TrackHandValueSettings(Settings.FlopHvSettings);
            TrackHandValueSettings(Settings.TurnHvSettings);
            TrackHandValueSettings(Settings.RiverHvSettings);

            if (Settings.FlopTextureSettings != null)
            {
                disposableList.Add(
                   Settings.FlopTextureSettings
                       .Changed
                       .Where(x => x.PropertyName != nameof(TextureSettings.BoardTextureFilterType))
                       .Subscribe(x => SetModified()));
            }

            if (Settings.TurnTextureSettings != null)
            {
                disposableList.Add(
                   Settings.TurnTextureSettings
                       .Changed
                       .Where(x => x.PropertyName != nameof(TextureSettings.BoardTextureFilterType))
                       .Subscribe(x => SetModified()));
            }

            if (Settings.RiverTextureSettings != null)
            {
                disposableList.Add(
                   Settings.RiverTextureSettings
                       .Changed
                       .Where(x => x.PropertyName != nameof(TextureSettings.BoardTextureFilterType))
                       .Subscribe(x => SetModified()));
            }

            TrackActions(Settings.PreflopActions);
            TrackActions(Settings.FlopActions);
            TrackActions(Settings.TurnActions);
            TrackActions(Settings.RiverActions);
        }

        private void TrackHandValueSettings(HandValueSettings handValueSettings)
        {
            if (handValueSettings == null)
            {
                return;
            }

            disposableList.Add(
                handValueSettings.Changed.Subscribe(x => SetModified()));

            if (handValueSettings.SelectedFlushDraws != null)
            {
                disposableList.Add(
                      Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                          h => handValueSettings.SelectedFlushDraws.CollectionChanged += h,
                          h => handValueSettings.SelectedFlushDraws.CollectionChanged -= h).Subscribe(x => SetModified()));
            }

            if (handValueSettings.SelectedHv != null)
            {
                disposableList.Add(
                      Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                          h => handValueSettings.SelectedHv.CollectionChanged += h,
                          h => handValueSettings.SelectedHv.CollectionChanged -= h).Subscribe(x => SetModified()));
            }

            if (handValueSettings.SelectedStraighDraws != null)
            {
                disposableList.Add(
                      Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                          h => handValueSettings.SelectedStraighDraws.CollectionChanged += h,
                          h => handValueSettings.SelectedStraighDraws.CollectionChanged -= h).Subscribe(x => SetModified()));
            }
        }

        private void TrackActions(ActionSettings actionSettings)
        {
            if (actionSettings != null)
            {
                disposableList.Add(
                  actionSettings
                      .Changed
                      .Subscribe(x => SetModified()));
            }
        }

        private void SetModified()
        {            
            Modified = true;
        }

        public NoteObject CopyTo(NoteObject existingNote = null)
        {
            if (existingNote == null)
            {
                existingNote = new NoteObject();
            }

            existingNote.ID = ID;
            existingNote.Description = Description;
            existingNote.DisplayedNote = DisplayedNote;

            existingNote.Settings.Cash = Settings.Cash;
            existingNote.Settings.Tournament = Settings.Tournament;
            existingNote.Settings.FacingUnopened = Settings.FacingUnopened;
            existingNote.Settings.Facing2PlusLimpers = Settings.Facing2PlusLimpers;
            existingNote.Settings.FacingRaisersCallers = Settings.FacingRaisersCallers;
            existingNote.Settings.Facing1Limper = Settings.Facing1Limper;
            existingNote.Settings.Facing1Raiser = Settings.Facing1Raiser;
            existingNote.Settings.Facing2Raisers = Settings.Facing2Raisers;
            existingNote.Settings.TypeNoLimit = Settings.TypeNoLimit;
            existingNote.Settings.TypePotLimit = Settings.TypePotLimit;
            existingNote.Settings.TypeLimit = Settings.TypeLimit;
            existingNote.Settings.PlayersNo34 = Settings.PlayersNo34;
            existingNote.Settings.PlayersNo56 = Settings.PlayersNo56;
            existingNote.Settings.PlayersNoHeadsUp = Settings.PlayersNoHeadsUp;
            existingNote.Settings.PlayersNoMax = Settings.PlayersNoMax;
            existingNote.Settings.PlayersNoMinVal = Settings.PlayersNoMinVal;
            existingNote.Settings.PlayersNoMaxVal = Settings.PlayersNoMaxVal;
            existingNote.Settings.PositionBB = Settings.PositionBB;
            existingNote.Settings.PositionButton = Settings.PositionButton;
            existingNote.Settings.PositionCutoff = Settings.PositionCutoff;
            existingNote.Settings.PositionEarly = Settings.PositionEarly;
            existingNote.Settings.PositionMiddle = Settings.PositionMiddle;
            existingNote.Settings.PositionSB = Settings.PositionSB;

            if (Settings.ExcludedCardsList != null)
            {
                existingNote.Settings.ExcludedCardsList.AddRange(Settings.ExcludedCardsList);
            }

            if (Settings.SelectedFilters != null)
            {
                existingNote.Settings.SelectedFilters?.Clear();
                Settings.SelectedFilters.ForEach(x => existingNote.Settings.SelectedFilters.Add(x));
            };

            if (Settings.SelectedFiltersComparison != null)
            {
                existingNote.Settings.SelectedFiltersComparison?.Clear();
                Settings.SelectedFiltersComparison.ForEach(x => existingNote.Settings.SelectedFiltersComparison.Add(x));
            };

            if (Settings.FlopHvSettings != null)
            {
                existingNote.Settings.FlopHvSettings = Settings.FlopHvSettings.Copy();
            }

            if (Settings.TurnHvSettings != null)
            {
                existingNote.Settings.TurnHvSettings = Settings.TurnHvSettings.Copy();
            }

            if (Settings.RiverHvSettings != null)
            {
                existingNote.Settings.RiverHvSettings = Settings.RiverHvSettings.Copy();
            }

            if (Settings.FlopTextureSettings != null)
            {
                existingNote.Settings.FlopTextureSettings = (FlopTextureSettings)Settings.FlopTextureSettings.Copy();
            }

            if (Settings.TurnTextureSettings != null)
            {
                existingNote.Settings.TurnTextureSettings = (TurnTextureSettings)Settings.TurnTextureSettings.Copy();
            }

            if (Settings.RiverTextureSettings != null)
            {
                existingNote.Settings.RiverTextureSettings = (RiverTextureSettings)Settings.RiverTextureSettings.Copy();
            }

            if (Settings.PreflopActions != null)
            {
                existingNote.Settings.PreflopActions = Settings.PreflopActions.Copy();
            }

            if (Settings.FlopActions != null)
            {
                existingNote.Settings.FlopActions = Settings.FlopActions.Copy();
            }

            if (Settings.TurnActions != null)
            {
                existingNote.Settings.TurnActions = Settings.TurnActions.Copy();
            }

            if (Settings.RiverActions != null)
            {
                existingNote.Settings.RiverActions = Settings.RiverActions.Copy();
            }

            return existingNote;
        }
    }
}