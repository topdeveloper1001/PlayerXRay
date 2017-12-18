//-----------------------------------------------------------------------
// <copyright file="NoteSettingsObject.cs" company="Ace Poker Solutions">
// Copyright © 2017 Ace Poker Solutions. All Rights Reserved.
// Unless otherwise noted, all materials contained in this Site are copyrights, 
// trademarks, trade dress and/or other intellectual properties, owned, 
// controlled or licensed by Ace Poker Solutions and may not be used without 
// written consent except as provided in these terms and conditions or in the 
// copyright notice (documents and software) or other proprietary notices 
// provided with the relevant materials.
// </copyright>
//----------------------------------------------------------------------

using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects.ActionsObjects;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects.TextureObjects;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects
{
    public class NoteSettingsObject : ReactiveObject
    {
        public NoteSettingsObject()
        {
            Cash = true;
            Tournament = true;

            FacingUnopened = true;
            Facing2PlusLimpers = true;
            FacingRaisersCallers = true;
            Facing1Limper = true;
            Facing1Raiser = true;
            Facing2Raisers = true;

            TypeNoLimit = true;
            TypePotLimit = true;
            TypeLimit = true;
            PlayersNo34 = true;
            PlayersNo56 = true;
            PlayersNoHeadsUp = true;
            PlayersNoMax = true;
            PlayersNoMinVal = 2;
            PlayersNoMaxVal = 10;

            PositionBB = true;
            PositionButton = true;
            PositionCutoff = true;
            PositionEarly = true;
            PositionMiddle = true;
            PositionSB = true;

            ExcludedStakes = new List<Stake>();
            ExcludedCardsList = new List<string>();
            SelectedFilters = new ObservableCollection<FilterObject>();
            SelectedFiltersComparison = new ObservableCollection<FilterObject>();

            FlopHvSettings = new HandValueSettings();
            TurnHvSettings = new HandValueSettings();
            RiverHvSettings = new HandValueSettings();

            FlopTextureSettings = new FlopTextureSettings();
            TurnTextureSettings = new TurnTextureSettings();
            RiverTextureSettings = new RiverTextureSettings();

            FlopActions = new ActionSettings();
            TurnActions = new ActionSettings();
            RiverActions = new ActionSettings();
            PreflopActions = new ActionSettings();
        }

        private ObservableCollection<FilterObject> selectedFilters;

        public ObservableCollection<FilterObject> SelectedFilters
        {
            get
            {
                return selectedFilters;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedFilters, value);
            }
        }

        private ObservableCollection<FilterObject> selectedFiltersComparison;

        public ObservableCollection<FilterObject> SelectedFiltersComparison
        {
            get
            {
                return selectedFiltersComparison;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedFiltersComparison, value);
            }
        }

        private string excludedCards;

        public string ExcludedCards
        {
            get
            {
                return excludedCards;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref excludedCards, value);
            }
        }

        private List<Stake> excludedStakes;

        public List<Stake> ExcludedStakes
        {
            get
            {
                return excludedStakes;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref excludedStakes, value);
            }
        }

        [XmlIgnore]
        public List<string> ExcludedCardsList
        {
            get
            {
                return ExcludedCards.Contains(",")
                           ? new List<string>(ExcludedCards.Split(','))
                           : string.IsNullOrEmpty(ExcludedCards)
                                 ? new List<string>()
                                 : new List<string> { ExcludedCards };
            }
            set
            {
                var excludedCards = string.Empty;

                foreach (string card in value)
                {
                    excludedCards += card + ',';
                }

                ExcludedCards = excludedCards.Contains(",") ?
                    excludedCards.Remove(excludedCards.LastIndexOf(','), 1) :
                    excludedCards;
            }
        }

        private bool typeNoLimit;

        public bool TypeNoLimit
        {
            get
            {
                return typeNoLimit;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref typeNoLimit, value);
            }
        }

        private bool typePotLimit;

        public bool TypePotLimit
        {
            get
            {
                return typePotLimit;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref typePotLimit, value);
            }
        }

        private bool typeLimit;

        public bool TypeLimit
        {
            get
            {
                return typeLimit;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref typeLimit, value);
            }
        }

        private bool cash;

        public bool Cash
        {
            get
            {
                return cash;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref cash, value);
            }
        }

        private bool tournament;

        public bool Tournament
        {
            get
            {
                return tournament;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref tournament, value);
            }
        }

        private bool includeBoard;

        public bool IncludeBoard
        {
            get
            {
                return includeBoard;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref includeBoard, value);
            }
        }

        #region Number of players

        private bool playersNoHeadsUp;

        public bool PlayersNoHeadsUp
        {
            get
            {
                return playersNoHeadsUp;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref playersNoHeadsUp, value);
            }
        }

        private bool playersNo34;

        public bool PlayersNo34
        {
            get
            {
                return playersNo34;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref playersNo34, value);
            }
        }

        private bool playersNo56;

        public bool PlayersNo56
        {
            get
            {
                return playersNo56;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref playersNo56, value);
            }
        }

        private bool playersNoMax;

        public bool PlayersNoMax
        {
            get
            {
                return playersNoMax;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref playersNoMax, value);
            }
        }

        private bool playersNoCustom;

        public bool PlayersNoCustom
        {
            get
            {
                return playersNoCustom;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref playersNoCustom, value);
                PlayersNoHeadsUp = PlayersNo34 = PlayersNo56 = PlayersNoMax = !playersNoCustom;
            }
        }

        private int playersNoMinVal;

        public int PlayersNoMinVal
        {
            get
            {
                return playersNoMinVal;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref playersNoMinVal, value);
            }
        }

        private int playersNoMaxVal;

        public int PlayersNoMaxVal
        {
            get
            {
                return playersNoMaxVal;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref playersNoMaxVal, value);
            }
        }

        #endregion

        private bool positionSB;

        public bool PositionSB
        {
            get
            {
                return positionSB;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionSB, value);
            }
        }

        private bool positionEarly;

        public bool PositionEarly
        {
            get
            {
                return positionEarly;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionEarly, value);
            }
        }

        private bool positionCutoff;

        public bool PositionCutoff
        {
            get
            {
                return positionCutoff;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionCutoff, value);
            }
        }

        private bool positionBB;

        public bool PositionBB
        {
            get
            {
                return positionBB;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionBB, value);
            }
        }

        private bool positionMiddle;

        public bool PositionMiddle
        {
            get
            {
                return positionMiddle;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionMiddle, value);
            }
        }

        private bool positionButton;

        public bool PositionButton
        {
            get
            {
                return positionButton;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionButton, value);
            }
        }

        private bool facingUnopened;

        public bool FacingUnopened
        {
            get
            {
                return facingUnopened;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref facingUnopened, value);
            }
        }

        private bool facing2PlusLimpers;

        public bool Facing2PlusLimpers
        {
            get
            {
                return facing2PlusLimpers;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref facing2PlusLimpers, value);
            }
        }

        private bool facingRaisersCallers;

        public bool FacingRaisersCallers
        {
            get
            {
                return facingRaisersCallers;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref facingRaisersCallers, value);
            }
        }

        private bool facing1Limper;

        public bool Facing1Limper
        {
            get
            {
                return facing1Limper;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref facing1Limper, value);
            }
        }

        private bool facing1Raiser;

        public bool Facing1Raiser
        {
            get
            {
                return facing1Raiser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref facing1Raiser, value);
            }
        }

        private bool facing2Raisers;

        public bool Facing2Raisers
        {
            get
            {
                return facing2Raisers;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref facing2Raisers, value);
            }
        }

        private bool positionSBRaiser;

        public bool PositionSBRaiser
        {
            get
            {
                return positionSBRaiser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionSBRaiser, value);
            }
        }

        private bool positionEarlyRaiser;

        public bool PositionEarlyRaiser
        {
            get
            {
                return positionEarlyRaiser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionEarlyRaiser, value);
            }
        }

        private bool positionCutoffRaiser;

        public bool PositionCutoffRaiser
        {
            get
            {
                return positionCutoffRaiser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionCutoffRaiser, value);
            }
        }

        private bool positionBBRaiser;

        public bool PositionBBRaiser
        {
            get
            {
                return positionBBRaiser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionBBRaiser, value);
            }
        }

        private bool positionMiddleRaiser;

        public bool PositionMiddleRaiser
        {
            get
            {
                return positionMiddleRaiser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionMiddleRaiser, value);
            }
        }

        private bool positionButtonRaiser;

        public bool PositionButtonRaiser
        {
            get
            {
                return positionButtonRaiser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionButtonRaiser, value);
            }
        }

        private bool positionSB3Bet;

        public bool PositionSB3Bet
        {
            get
            {
                return positionSB3Bet;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionSB3Bet, value);
            }
        }

        private bool positionEarly3Bet;

        public bool PositionEarly3Bet
        {
            get
            {
                return positionEarly3Bet;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionEarly3Bet, value);
            }
        }

        private bool positionCutoff3Bet;

        public bool PositionCutoff3Bet
        {
            get
            {
                return positionCutoff3Bet;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionCutoff3Bet, value);
            }
        }

        private bool positionBB3Bet;

        public bool PositionBB3Bet
        {
            get
            {
                return positionBB3Bet;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionBB3Bet, value);
            }
        }

        private bool positionMiddle3Bet;

        public bool PositionMiddle3Bet
        {
            get
            {
                return positionMiddle3Bet;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionMiddle3Bet, value);
            }
        }

        private bool positionButton3Bet;

        public bool PositionButton3Bet
        {
            get
            {
                return positionButton3Bet;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref positionButton3Bet, value);
            }
        }

        [XmlIgnore]
        public double MBCMinSizeOfPot
        {
            get
            {
                var filter = SelectedFilters.FirstOrDefault(p => p.Filter == FilterEnum.FinalPotSizeinBBsisBiggerThan);

                return filter != null && filter.Value.HasValue ?
                    filter.Value.Value : 0d;
            }
        }

        [XmlIgnore]
        public double MBCMaxSizeOfPot
        {
            get
            {
                var filter = SelectedFilters.FirstOrDefault(p => p.Filter == FilterEnum.FinalPotSizeinBBsisLessThan);

                return filter != null && filter.Value.HasValue ?
                    filter.Value.Value : 0d;
            }
        }

        [XmlIgnore]
        public bool MBCWentToShowdown
        {
            get { return SelectedFilters.Any(p => p.Filter == FilterEnum.SawShowdown); }
        }

        [XmlIgnore]
        public bool MBCAllInPreFlop
        {
            get { return SelectedFilters.Any(p => p.Filter == FilterEnum.AllinPreflop); }
        }

        #region Actions 

        private ActionSettings preflopActions;

        public ActionSettings PreflopActions
        {
            get
            {
                return preflopActions;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref preflopActions, value);
            }
        }

        private ActionSettings flopActions;

        public ActionSettings FlopActions
        {
            get
            {
                return flopActions;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref flopActions, value);
            }
        }

        private ActionSettings turnActions;

        public ActionSettings TurnActions
        {
            get
            {
                return turnActions;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref turnActions, value);
            }
        }

        private ActionSettings riverActions;

        public ActionSettings RiverActions
        {
            get
            {
                return riverActions;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref riverActions, value);
            }
        }

        #endregion

        #region Hand values

        private HandValueSettings flopHvSettings;

        public HandValueSettings FlopHvSettings
        {
            get
            {
                return flopHvSettings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref flopHvSettings, value);
            }
        }

        private HandValueSettings riverHvSettings;

        public HandValueSettings RiverHvSettings
        {
            get
            {
                return riverHvSettings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref riverHvSettings, value);
            }
        }

        private HandValueSettings turnHvSettings;

        public HandValueSettings TurnHvSettings
        {
            get
            {
                return turnHvSettings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref turnHvSettings, value);
            }
        }

        #endregion

        #region Texture settings

        private FlopTextureSettings flopTextureSettings;

        public FlopTextureSettings FlopTextureSettings
        {
            get
            {
                return flopTextureSettings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref flopTextureSettings, value);
            }
        }

        private TurnTextureSettings turnTextureSettings;

        public TurnTextureSettings TurnTextureSettings
        {
            get
            {
                return turnTextureSettings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref turnTextureSettings, value);
            }
        }

        private RiverTextureSettings riverTextureSettings;

        public RiverTextureSettings RiverTextureSettings
        {
            get
            {
                return riverTextureSettings;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref riverTextureSettings, value);
            }
        }

        #endregion

        public override bool Equals(object x)
        {
            NoteSettingsObject x1 = (NoteSettingsObject)x;
            NoteSettingsObject x2 = this;

            return x1.MBCAllInPreFlop == x2.MBCAllInPreFlop &&
                   x1.MBCMinSizeOfPot == x2.MBCMinSizeOfPot &&
                   x2.MBCMaxSizeOfPot == x2.MBCMaxSizeOfPot &&
                   x1.MBCWentToShowdown == x2.MBCWentToShowdown &&
                   x1.PlayersNo34 == x2.PlayersNo34 &&
                   x1.PlayersNo56 == x2.PlayersNo56 &&
                   x1.PlayersNoCustom == x2.PlayersNoCustom &&
                   x1.PlayersNoHeadsUp == x2.PlayersNoHeadsUp &&
                   x1.PlayersNoMax == x2.PlayersNoMax &&
                   x1.PlayersNoMaxVal == x2.PlayersNoMaxVal &&
                   x1.PlayersNoMinVal == x2.PlayersNoMinVal &&
                   x1.PositionBB == x2.PositionBB &&
                   x1.PositionButton == x2.PositionButton &&
                   x1.PositionCutoff == x2.PositionCutoff &&
                   x1.PositionEarly == x2.PositionEarly &&
                   x1.PositionMiddle == x2.PositionMiddle &&
                   x1.PositionSB == x2.PositionSB &&
                   x1.TypeLimit == x2.TypeLimit &&
                   x1.TypeNoLimit == x2.TypeNoLimit &&
                   x1.TypePotLimit == x2.TypePotLimit && SelectedStakesEquality(x1.ExcludedStakes) &&
                   SelectedCardsEquality(x1.ExcludedCardsList) &&
                   CompareSelectedFilters(x1.SelectedFilters) &&
                   x1.FlopHvSettings.Equals(x2.FlopHvSettings) &&
                   x1.TurnHvSettings.Equals(x2.TurnHvSettings) &&
                   x1.RiverHvSettings.Equals(x2.RiverHvSettings) &&
                   x1.FlopTextureSettings.Equals(x2.FlopTextureSettings) &&
                   x1.TurnTextureSettings.Equals(x2.TurnTextureSettings) &&
                   x1.RiverTextureSettings.Equals(x2.RiverTextureSettings) &&
                   x1.FlopActions.Equals(x2.FlopActions) &&
                   x1.TurnActions.Equals(x2.TurnActions) &&
                   x1.RiverActions.Equals(x2.RiverActions) &&
                   x1.PreflopActions.Equals(x2.PreflopActions) &&
                   x1.PositionBBRaiser == x2.PositionBBRaiser &&
                   x1.PositionButtonRaiser == x2.PositionButtonRaiser &&
                   x1.PositionCutoffRaiser == x2.PositionCutoffRaiser &&
                   x1.PositionEarlyRaiser == x2.PositionEarlyRaiser &&
                   x1.PositionMiddleRaiser == x2.PositionMiddleRaiser &&
                   x1.PositionSBRaiser == x2.PositionSBRaiser &&
                   x1.PositionBB3Bet == x2.PositionBB3Bet &&
                   x1.PositionButton3Bet == x2.PositionButton3Bet &&
                   x1.PositionCutoff3Bet == x2.PositionCutoff3Bet &&
                   x1.PositionEarly3Bet == x2.PositionEarly3Bet &&
                   x1.PositionMiddle3Bet == x2.PositionMiddle3Bet &&
                   x1.PositionSB3Bet == x2.PositionSB3Bet &&
                   x1.Facing1Limper == x2.Facing1Limper &&
                   x1.Facing1Raiser == x2.Facing1Raiser &&
                   x1.Facing2PlusLimpers == x2.Facing2PlusLimpers &&
                   x1.Facing2Raisers == x2.Facing2Raisers &&
                   x1.FacingRaisersCallers == x2.FacingRaisersCallers &&
                   x1.FacingUnopened == x2.FacingUnopened;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private bool CompareSelectedFilters(ICollection<FilterObject> newList)
        {
            if (newList.Count != SelectedFilters.Count)
                return false;

            foreach (FilterObject filter in newList)
            {
                FilterObject existingFilter = SelectedFilters.FirstOrDefault(p => p.Tag == filter.Tag);

                if (existingFilter == null)
                    return false;
                if (existingFilter.Value != filter.Value)
                    return false;
            }

            foreach (FilterObject filter in SelectedFilters)
            {
                FilterObject existingFilter = newList.FirstOrDefault(p => p.Tag == filter.Tag);

                if (existingFilter == null)
                    return false;
                if (existingFilter.Value != filter.Value)
                    return false;
            }

            return true;
        }

        private bool SelectedStakesEquality(List<Stake> newList)
        {
            if (newList.Count != ExcludedStakes.Count)
                return false;

            foreach (Stake stake in newList)
            {
                if (ExcludedStakes.FindAll(p => p.Name == stake.Name).Count == 0)
                    return false;
            }

            foreach (Stake stake in ExcludedStakes)
            {
                if (newList.FindAll(p => p.Name == stake.Name).Count == 0)
                    return false;
            }

            return true;
        }

        private bool SelectedCardsEquality(ICollection<string> newList)
        {
            if (newList.Count != ExcludedCardsList.Count)
                return false;

            foreach (string card in newList)
            {
                if (!ExcludedCardsList.Contains(card))
                    return false;
            }

            foreach (string card in ExcludedCardsList)
            {
                if (!newList.Contains(card))
                    return false;
            }

            return true;
        }

        public List<long> GetPreflopFacingValues(ClientType client)
        {
            List<long> result = new List<long>();

            if (client == ClientType.HoldemManager)
            {
                if (Facing1Limper)
                    result.Add(1);
                if (FacingUnopened)
                    result.Add(0);
                if (Facing2PlusLimpers)
                    result.Add(2);
                if (Facing1Raiser)
                    result.Add(3);
                if (FacingRaisersCallers)
                    result.Add(4);
                if (Facing2Raisers)
                    result.Add(5);
            }

            return result;
        }
    }
}