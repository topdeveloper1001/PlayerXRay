﻿//-----------------------------------------------------------------------
// <copyright file="NoteManager.cs" company="Ace Poker Solutions">
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
using DriveHUD.Common.Log;
using DriveHUD.Entities;
using DriveHUD.PlayerXRay.BusinessHelper.ApplicationSettings;
using DriveHUD.PlayerXRay.BusinessHelper.TextureAnalyzers.Flush;
using DriveHUD.PlayerXRay.BusinessHelper.TextureAnalyzers.Straight;
using DriveHUD.PlayerXRay.BusinessHelper.TextureHelpers;
using DriveHUD.PlayerXRay.DataTypes;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects.ActionsObjects;
using HandHistories.Objects.Actions;
using HandHistories.Objects.Cards;
using Model;
using Model.Importer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DriveHUD.PlayerXRay.BusinessHelper
{
    public class NoteManager
    {
        /// <summary>
        /// Checks whenever the specified <see cref="PlayerstatisticExtended"/> matches the specified <see cref="NoteObject"/>
        /// </summary>
        /// <param name="note"><see cref="NoteObject"/> to match</param>
        /// <param name="playerstatistic"><see cref="PlayerstatisticExtended"/> to match</param>
        /// <returns>True if matches, otherwise - false</returns>
        public static bool IsMatch(NoteObject note, PlayerstatisticExtended playerstatistic)
        {
            var selectedPlayerStatistics = new List<PlayerstatisticExtended> { playerstatistic };

            selectedPlayerStatistics = FilterByMiscConditions(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByPositionCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByPreflopFacingCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByPositionThreeBetCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByPositionRaiserCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByNoOfPlayerCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByStakeCondition(selectedPlayerStatistics, note.Settings);

            selectedPlayerStatistics = FilterByHoleCardCondition(selectedPlayerStatistics, note.Settings);

            selectedPlayerStatistics = FilterByPreflopActionCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByFlopActionCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByTurnActionCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByRiverActionCondition(selectedPlayerStatistics, note.Settings);

            selectedPlayerStatistics = FilterByFlopTextureCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByTurnTextureCondition(selectedPlayerStatistics, note.Settings);
            selectedPlayerStatistics = FilterByRiverTextureCondition(selectedPlayerStatistics, note.Settings);

            selectedPlayerStatistics = FilterByHandValueConditions(selectedPlayerStatistics, note.Settings.FlopHvSettings, Street.Flop);
            selectedPlayerStatistics = FilterByHandValueConditions(selectedPlayerStatistics, note.Settings.TurnHvSettings, Street.Turn);
            selectedPlayerStatistics = FilterByHandValueConditions(selectedPlayerStatistics, note.Settings.RiverHvSettings, Street.River);

            selectedPlayerStatistics = FilterByAllSelectedFilters(selectedPlayerStatistics, note.Settings.SelectedFilters, note.Settings.SelectedFiltersComparison);

            return selectedPlayerStatistics.Count > 0;
        }

        private static List<PlayerstatisticExtended> FilterByMiscConditions(List<PlayerstatisticExtended> selectedPlayerStatistics, NoteSettingsObject settings)
        {
            if (settings.Cash && settings.Tournament)
            {
                return selectedPlayerStatistics;
            }

            return selectedPlayerStatistics
                .Where(x => settings.Tournament && x.Playerstatistic.IsTourney ||
                        settings.Cash && !x.Playerstatistic.IsTourney)
                .ToList();
        }

        #region hand value analyzers     

        private readonly static Dictionary<Street, List<int>> handValuesCache = new Dictionary<Street, List<int>>();

        private static List<PlayerstatisticExtended> FilterByHandValueConditions(List<PlayerstatisticExtended> playerStatistics, HandValueSettings settings, Street street)
        {
            if (settings == null || settings.AnyHv && settings.AnyFlushDraws && settings.AnyStraightDraws)
            {
                return playerStatistics;
            }

            var filteredList = new List<PlayerstatisticExtended>();

            // check hand values
            if (!settings.AnyHv)
            {
                if (!handValuesCache.ContainsKey(street))
                {
                    lock (handValuesCache)
                    {
                        if (!handValuesCache.ContainsKey(street))
                        {
                            handValuesCache.Add(street, HandValuesHelper.GetHandValueObjects(street).Select(x => x.Value).ToList());
                        }
                    }
                }

                var restrictedHv = handValuesCache[street]
                    .Except(settings.SelectedHv)
                    .ToList();

                foreach (var handValue in settings.SelectedHv)
                {
                    var handValueEnum = (HandValueEnum)handValue;
                    filteredList.AddRange(NoteManagerHelper.HandValueFilterHelper(playerStatistics, handValueEnum, street));
                }

                filteredList = filteredList.Distinct().ToList();

                if (filteredList.Count > 0 && restrictedHv.Count > 0)
                {
                    foreach (var handValue in restrictedHv)
                    {
                        var handValueEnum = (HandValueEnum)handValue;
                        var restrictedList = NoteManagerHelper.HandValueFilterHelper(filteredList, handValueEnum, street);

                        if (restrictedList.Count > 0)
                        {
                            filteredList = filteredList
                                .Except(restrictedList)
                                .ToList();

                            if (filteredList.Count == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            // do not check flush and straight draws for river
            if (street == Street.River)
            {
                return filteredList.Distinct().ToList();
            }

            var extraFilters = new List<Func<List<PlayerstatisticExtended>, List<PlayerstatisticExtended>>>();

            // check flush draw hand values
            if (!settings.AnyFlushDraws && settings.SelectedFlushDraws.Count > 0)
            {
                foreach (var flushDrawHandValue in settings.SelectedFlushDraws)
                {
                    var handValueFlushDrawEnum = (HandValueFlushDrawEnum)flushDrawHandValue;

                    if (handValueFlushDrawEnum == HandValueFlushDrawEnum.NoFlushDraw)
                    {
                        extraFilters.Add(x => NoteManagerHelper.HandValueFilterFlushDrawHelper(x, handValueFlushDrawEnum, street));
                        continue;
                    }

                    filteredList.AddRange(NoteManagerHelper.HandValueFilterFlushDrawHelper(playerStatistics, handValueFlushDrawEnum, street));
                }
            }

            // check straight draw hand values
            if (!settings.AnyStraightDraws && settings.SelectedStraighDraws.Count > 0)
            {
                foreach (var straightDrawHandValue in settings.SelectedStraighDraws)
                {
                    var handValueStraightDrawEnum = (HandValueStraightDraw)straightDrawHandValue;

                    if (handValueStraightDrawEnum == HandValueStraightDraw.NoStraightDraw)
                    {
                        extraFilters.Add(x => NoteManagerHelper.HandValueFilterStraightDrawHelper(x, handValueStraightDrawEnum, street));
                        continue;
                    }

                    filteredList.AddRange(NoteManagerHelper.HandValueFilterStraightDrawHelper(playerStatistics, handValueStraightDrawEnum, street));
                }
            }

            var filteredResult = filteredList.Distinct().ToList();

            foreach (var extraFilter in extraFilters)
            {
                filteredResult = extraFilter(filteredResult);
            }

            return filteredResult;
        }

        #endregion

        #region Board Texture

        private static List<PlayerstatisticExtended> FilterByFlopTextureCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            if (!settings.FlopTextureSettings.IsFlushCardFilter && !settings.FlopTextureSettings.IsOpenEndedStraightDrawsFilter && !settings.FlopTextureSettings.IsPossibleStraightsFilter && !settings.FlopTextureSettings.IsGutshotsFilter && !settings.FlopTextureSettings.IsHighcardFilter && !settings.FlopTextureSettings.IsCardTextureFilter && !settings.FlopTextureSettings.IsPairedFilter)
                return playerStatistics;

            List<PlayerstatisticExtended> fileteredList = new List<PlayerstatisticExtended>();

            foreach (var playerstatistic in playerStatistics)
            {
                if (settings.FlopTextureSettings.IsFlushCardFilter)
                {
                    //filter for rainbow flush
                    if (settings.FlopTextureSettings.FlushCard == FlopFlushCardsEnum.Rainbow && !new NoPossibleFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Flop))
                        continue;
                    //filter for two of one suit
                    if (settings.FlopTextureSettings.FlushCard == FlopFlushCardsEnum.TwoOfOneSuit && !new TwoOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Flop))
                        continue;
                    //filter for three of one suit
                    if (settings.FlopTextureSettings.FlushCard == FlopFlushCardsEnum.ThreeOfOneSuit && !new ThreeOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Flop))
                        continue;
                }

                //filter for open-ended straights
                //if number of open ended straigths is not equal to the number we need, then we skip such playerstatistic
                if (settings.FlopTextureSettings.IsOpenEndedStraightDrawsFilter)
                    if (new OpenEndedStraightTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Flop) != settings.FlopTextureSettings.OpenEndedStraightDraws)
                        continue;

                //filter for possible straights and according < > or ==
                if (settings.FlopTextureSettings.IsPossibleStraightsFilter)
                    if (!BoardTextureAnalyzerHelpers.CheckEquality(settings.FlopTextureSettings.PossibleStraightsCompare, new PossibleStraightTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Flop), settings.FlopTextureSettings.PossibleStraights))
                        continue;

                //filter for gutshot straights
                if (settings.FlopTextureSettings.IsGutshotsFilter)
                    if (new GutShotBeatNutsTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Flop) != settings.FlopTextureSettings.Gutshots)
                        continue;

                //filter for the highest card
                if (settings.FlopTextureSettings.IsHighcardFilter)
                    if (BoardTextureAnalyzerHelpers.HighestBoardCardRank(playerstatistic.Playerstatistic.Board, Street.Flop) != DataTypes.Card.GetCardRank(settings.FlopTextureSettings.HighestCard))
                        continue;

                //filter for exact flop texture
                if (settings.FlopTextureSettings.IsCardTextureFilter)
                    if (!BoardTextureAnalyzerHelpers.BoardContainsExactTextureCards(playerstatistic.Playerstatistic.Board, settings.FlopTextureSettings.SelectedCardTextureList, Street.Flop))
                        continue;

                //filter for flop is Paired
                if (settings.FlopTextureSettings.IsPairedFilter)
                    if (!BoardTextureAnalyzerHelpers.BoardContainsAPair(playerstatistic.Playerstatistic.Board, Street.Flop))
                        continue;

                fileteredList.Add(playerstatistic);
            }

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByTurnTextureCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            if (!settings.TurnTextureSettings.IsFlushCardFilter && !settings.TurnTextureSettings.IsOpenEndedStraightDrawsFilter && !settings.TurnTextureSettings.IsPossibleStraightsFilter && !settings.TurnTextureSettings.IsGutshotsFilter && !settings.TurnTextureSettings.IsHighcardFilter && !settings.TurnTextureSettings.IsCardTextureFilter && !settings.TurnTextureSettings.IsPairedFilter)
                return playerStatistics;

            List<PlayerstatisticExtended> fileteredList = new List<PlayerstatisticExtended>();

            foreach (var playerstatistic in playerStatistics)
            {
                //filters for turn flush
                if (settings.TurnTextureSettings.IsFlushCardFilter)
                {
                    //filter for rainbow flush
                    if (settings.TurnTextureSettings.FlushCard == TurnFlushCardsEnum.Rainbow && !new NoPossibleFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn))
                        continue;
                    //filter for two of two suits
                    if (settings.TurnTextureSettings.FlushCard == TurnFlushCardsEnum.TwoOfTwoSuits && !new TwoOfTwoSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn))
                        continue;
                    //filter for two of one suit
                    if (settings.TurnTextureSettings.FlushCard == TurnFlushCardsEnum.TwoOfOneSuit && !new TwoOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn))
                        continue;
                    //filter for three of one suit
                    if (settings.TurnTextureSettings.FlushCard == TurnFlushCardsEnum.ThreeOfOneSuit && !new ThreeOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn))
                        continue;
                    //filter for four of one suit  
                    if (settings.TurnTextureSettings.FlushCard == TurnFlushCardsEnum.FourOfOneSuit && !new FourOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn))
                        continue;
                }

                //filter for open-ended straights
                //if number of open ended straigths is not equal to the number we need, then we skip such playerstatistic
                if (settings.TurnTextureSettings.IsOpenEndedStraightDrawsFilter)
                    if (new OpenEndedStraightTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn) != settings.TurnTextureSettings.OpenEndedStraightDraws)
                        continue;

                //filter for possible straights and according < > or ==
                if (settings.TurnTextureSettings.IsPossibleStraightsFilter)
                    if (!BoardTextureAnalyzerHelpers.CheckEquality(settings.TurnTextureSettings.PossibleStraightsCompare, new PossibleStraightTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn), settings.TurnTextureSettings.PossibleStraights))
                        continue;

                //filter for gutshot straights
                if (settings.TurnTextureSettings.IsGutshotsFilter)
                    if (new GutShotBeatNutsTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.Turn) != settings.TurnTextureSettings.Gutshots)
                        continue;

                //filter for the highest card
                if (settings.TurnTextureSettings.IsHighcardFilter)
                    if (BoardTextureAnalyzerHelpers.HighestBoardCardRank(playerstatistic.Playerstatistic.Board, Street.Turn) != DataTypes.Card.GetCardRank(settings.TurnTextureSettings.HighestCard))
                        continue;

                //filter for exact turn texture
                if (settings.TurnTextureSettings.IsCardTextureFilter)
                    if (!BoardTextureAnalyzerHelpers.BoardContainsExactTextureCards(playerstatistic.Playerstatistic.Board, settings.TurnTextureSettings.SelectedCardTextureList, Street.Turn))
                        continue;

                //filter for Turn is Paired
                if (settings.TurnTextureSettings.IsPairedFilter)
                    if (!BoardTextureAnalyzerHelpers.BoardContainsAPair(playerstatistic.Playerstatistic.Board, Street.Turn))
                        continue;


                fileteredList.Add(playerstatistic);
            }

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByRiverTextureCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            if (!settings.RiverTextureSettings.IsFlushCardFilter && !settings.RiverTextureSettings.IsPossibleStraightsFilter && !settings.RiverTextureSettings.IsHighcardFilter && !settings.RiverTextureSettings.IsCardTextureFilter && !settings.RiverTextureSettings.IsPairedFilter)
                return playerStatistics;

            List<PlayerstatisticExtended> fileteredList = new List<PlayerstatisticExtended>();

            foreach (var playerstatistic in playerStatistics)
            {
                //filters for river flush
                if (settings.RiverTextureSettings.IsFlushCardFilter)
                {
                    //filter for no possible flush
                    if (settings.RiverTextureSettings.FlushCard == RiverFlushCardsEnum.NoFlush && !new NoPossibleFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.River))
                        continue;
                    //filter for three of one suit
                    if (settings.RiverTextureSettings.FlushCard == RiverFlushCardsEnum.ThreeCardsOneSuit && !new ThreeOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.River))
                        continue;
                    //filter for four of one suit  
                    if (settings.RiverTextureSettings.FlushCard == RiverFlushCardsEnum.FourCardsOneSuit && !new FourOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.River))
                        continue;
                    //filter for five of one suit  
                    if (settings.RiverTextureSettings.FlushCard == RiverFlushCardsEnum.FiveCardsOneSuit && !new FiveOfOneSuitFlushTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.River))
                        continue;
                }

                //filter for possible straights and according < > or ==
                if (settings.RiverTextureSettings.IsPossibleStraightsFilter)
                    if (!BoardTextureAnalyzerHelpers.CheckEquality(settings.RiverTextureSettings.PossibleStraightsCompare, new PossibleStraightTextureAnalyzer().Analyze(playerstatistic.Playerstatistic.Board, Street.River), settings.RiverTextureSettings.PossibleStraights))
                        continue;

                //filter for the highest card
                if (settings.RiverTextureSettings.IsHighcardFilter)
                    if (BoardTextureAnalyzerHelpers.HighestBoardCardRank(playerstatistic.Playerstatistic.Board, Street.River) != DataTypes.Card.GetCardRank(settings.RiverTextureSettings.HighestCard))
                        continue;

                //filter for exact river texture
                if (settings.RiverTextureSettings.IsCardTextureFilter)
                    if (!BoardTextureAnalyzerHelpers.BoardContainsExactTextureCards(playerstatistic.Playerstatistic.Board, settings.RiverTextureSettings.SelectedCardTextureList, Street.River))
                        continue;

                //filter for river is Paired
                if (settings.RiverTextureSettings.IsPairedFilter)
                    if (!BoardTextureAnalyzerHelpers.BoardContainsAPair(playerstatistic.Playerstatistic.Board, Street.River))
                        continue;

                fileteredList.Add(playerstatistic);
            }


            return fileteredList;
        }

        #endregion

        #region ActionCondition

        private static List<PlayerstatisticExtended> FilterByRiverActionCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            var fileteredList = new List<PlayerstatisticExtended>();

            var riverObligatoryActions = GetObligatoryActions(settings.RiverActions);

            foreach (var playerstatistic in playerStatistics)
            {
                var riverActions = playerstatistic.HandHistory.River.ToList();
                var playerRiverActions = riverActions.Where(x => x.PlayerName == playerstatistic.Playerstatistic.PlayerName).ToList();

                if (CompareHandActionsWithObligatoryHandActions(playerRiverActions, riverActions, riverObligatoryActions))
                {
                    fileteredList.Add(playerstatistic);
                }
            }

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByTurnActionCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            var fileteredList = new List<PlayerstatisticExtended>();

            var turnObligatoryActions = GetObligatoryActions(settings.TurnActions);

            foreach (var playerstatistic in playerStatistics)
            {
                var turnActions = playerstatistic.HandHistory.Turn.ToList();
                var playerTurnActions = turnActions.Where(x => x.PlayerName == playerstatistic.Playerstatistic.PlayerName).ToList();

                if (CompareHandActionsWithObligatoryHandActions(playerTurnActions, turnActions, turnObligatoryActions))
                {
                    fileteredList.Add(playerstatistic);
                }
            }

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByFlopActionCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            var fileteredList = new List<PlayerstatisticExtended>();

            var flopObligatoryActions = GetObligatoryActions(settings.FlopActions);

            foreach (var playerstatistic in playerStatistics)
            {
                var flopActions = playerstatistic.HandHistory.Flop.ToList();
                var playerFlopActions = flopActions.Where(x => x.PlayerName == playerstatistic.Playerstatistic.PlayerName).ToList();

                if (CompareHandActionsWithObligatoryHandActions(playerFlopActions, flopActions, flopObligatoryActions))
                {
                    fileteredList.Add(playerstatistic);
                }
            }
            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByPreflopActionCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            var fileteredList = new List<PlayerstatisticExtended>();

            var preflopObligatoryActions = GetObligatoryActions(settings.PreflopActions);

            foreach (var playerstatistic in playerStatistics)
            {
                var preflopActions = playerstatistic.HandHistory.PreFlop.ToList();
                var playerPreFlopActions = preflopActions
                    .Where(x => x.PlayerName == playerstatistic.Playerstatistic.PlayerName &&
                       x.HandActionType != HandActionType.SMALL_BLIND && x.HandActionType != HandActionType.BIG_BLIND).ToList();

                if (CompareHandActionsWithObligatoryHandActions(playerPreFlopActions, preflopActions, preflopObligatoryActions))
                {
                    fileteredList.Add(playerstatistic);
                }
            }

            return fileteredList;
        }

        private static bool CompareHandActionsWithObligatoryHandActions(List<HandAction> heroHandActions, List<HandAction> handActions, List<ObligatoryAction> obligatoryHandActions)
        {
            if (heroHandActions.Count < obligatoryHandActions.Count)
            {
                return false;
            }

            for (int i = 0; i < obligatoryHandActions.Count; i++)
            {
                if (obligatoryHandActions[i].ActionType == ActionTypeEnum.Any ||
                    heroHandActions[i].HandActionType == ToHandActionType(obligatoryHandActions[i].ActionType))
                {
                    if (obligatoryHandActions[i].MinValue != 0 || obligatoryHandActions[i].MaxValue != 0)
                    {
                        var potSize = 0m;

                        foreach (var handAction in handActions)
                        {
                            if (handAction == heroHandActions[i])
                            {
                                var percentOfPotSize = (double)(handAction.Amount / potSize * 100m);

                                if (percentOfPotSize >= obligatoryHandActions[i].MinValue &&
                                    percentOfPotSize <= obligatoryHandActions[i].MaxValue)
                                {
                                    break;
                                }

                                return false;
                            }

                            potSize += handAction.Amount;
                        }
                    }

                    continue;
                }

                return false;
            }

            return true;
        }

        private static HandActionType ToHandActionType(ActionTypeEnum actionTypeEnum)
        {
            switch (actionTypeEnum)
            {
                //we don't consider HandActionType.Any because this case is eliminated before enum conversion is used
                case ActionTypeEnum.Bet:
                    return HandActionType.BET;
                case ActionTypeEnum.Check:
                    return HandActionType.CHECK;
                case ActionTypeEnum.Call:
                    return HandActionType.CALL;
                case ActionTypeEnum.Raise:
                    return HandActionType.RAISE;
                case ActionTypeEnum.Fold:
                    return HandActionType.FOLD;
                default:
                    LogProvider.Log.Error(CustomModulesNames.PlayerXRay, "Current player failed to load");
                    return HandActionType.UNKNOWN;
            }
        }

        private static List<ObligatoryAction> GetObligatoryActions(ActionSettings actionSettings)
        {
            var list = new List<ObligatoryAction>();

            if (actionSettings.FirstType == ActionTypeEnum.Any && actionSettings.SecondType == ActionTypeEnum.Any
                && actionSettings.FirstType == ActionTypeEnum.Any && actionSettings.FourthType == ActionTypeEnum.Any)
            {
                return list;
            }

            var firstAction = new ObligatoryAction
            {
                ActionType = actionSettings.FirstType,
                MaxValue = actionSettings.FirstMaxValue,
                MinValue = actionSettings.FirstMinValue
            };

            list.Add(firstAction);

            if (actionSettings.SecondType == ActionTypeEnum.Any && actionSettings.ThirdType == ActionTypeEnum.Any &&
                actionSettings.FourthType == ActionTypeEnum.Any)
            {
                return list;
            }

            var secondAction = new ObligatoryAction
            {
                ActionType = actionSettings.SecondType,
                MaxValue = actionSettings.SecondMaxValue,
                MinValue = actionSettings.SecondMinValue
            };

            list.Add(secondAction);

            if (actionSettings.ThirdType == ActionTypeEnum.Any && actionSettings.FourthType == ActionTypeEnum.Any)
            {
                return list;
            }

            var thirdAction = new ObligatoryAction
            {
                ActionType = actionSettings.ThirdType,
                MaxValue = actionSettings.ThirdMaxValue,
                MinValue = actionSettings.ThirdMinValue
            };

            list.Add(thirdAction);

            if (actionSettings.FourthType == ActionTypeEnum.Any)
            {
                return list;
            }

            var fourthdAction = new ObligatoryAction
            {
                ActionType = actionSettings.FourthType,
                MaxValue = actionSettings.FourthMaxValue,
                MinValue = actionSettings.FourthMinValue
            };

            list.Add(fourthdAction);

            return list;
        }

        #endregion           

        private static List<PlayerstatisticExtended> FilterByHoleCardCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            // list if excluded hole cards
            if (settings.ExcludedCardsList.Count == 0)
                return playerStatistics;

            List<long> list = new List<long>();

            foreach (string excludedCard in settings.ExcludedCardsList)
            {
                list.Add(HoleCardsHelper.GetHoleCardValue(excludedCard));
            }

            //list of the selected hole cards
            List<long> selectedCards = new List<long>();

            for (int i = 1; i < 170; i++)
            {
                if (!list.Contains(i))
                {
                    selectedCards.Add(i);
                }
            }

            return playerStatistics.Where(x => x.Playerstatistic.Cards.Length > 0 &&
                        selectedCards.Contains(HoleCardsHelper.ConverterCardsToSuitedUnsuited(x.Playerstatistic.Cards))).ToList();
        }

        private static List<PlayerstatisticExtended> FilterByStakeCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            List<PlayerstatisticExtended> fileteredList = playerStatistics;

            if (settings.ExcludedStakes.Count == 0)
                return playerStatistics;

            foreach (Stake stake in StaticStorage.Stakes)
            {
                //ignoring any excluded stake
                if (settings.ExcludedStakes.Count(p => p.Name == stake.Name) > 0)
                    continue;

                fileteredList = fileteredList.Where(x => x.Playerstatistic.PokergametypeId == stake.ID
                    && x.Playerstatistic.BigBlind == stake.StakeValue).ToList();
            }

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByNoOfPlayerCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            int low = 0, high = 0;

            if (settings.PlayersNoCustom)
            {
                low = settings.PlayersNoMinVal;
                high = settings.PlayersNoMaxVal;
            }
            else
            {
                if (settings.PlayersNoHeadsUp)
                    low = 2;
                else if (settings.PlayersNo34)
                    low = 3;
                else if (settings.PlayersNo56)
                    low = 5;
                else if (settings.PlayersNoMax)
                    low = 7;
                if (settings.PlayersNoMax)
                    high = 10;
                else if (settings.PlayersNo56)
                    high = 6;
                else if (settings.PlayersNo34)
                    high = 4;
                else if (settings.PlayersNoHeadsUp)
                    high = 2;
            }

            List<PlayerstatisticExtended> filteredList = new List<PlayerstatisticExtended>();
            try
            {
                filteredList.AddRange(playerStatistics.Where(x => x.HandHistory.GameDescription?.SeatType.MaxPlayers <= high && x.HandHistory.GameDescription?.SeatType.MaxPlayers >= low));
            }
            catch (Exception ex)
            {
                LogProvider.Log.Error(CustomModulesNames.PlayerXRay, "FilterByNoOfPlayerCondition method exception", ex);
            }

            return filteredList;
        }

        private static List<PlayerstatisticExtended> FilterByPositionRaiserCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            // if everything is unchecked return entry list of Playerstatistics
            if (!settings.PositionBBRaiser && !settings.PositionButtonRaiser &&
                !settings.PositionCutoffRaiser && !settings.PositionEarlyRaiser &&
                !settings.PositionMiddleRaiser && !settings.PositionSBRaiser)
            {
                return playerStatistics;
            }

            var fileteredList = new List<PlayerstatisticExtended>();

            foreach (var playerStatistic in playerStatistics)
            {
                var facedHandActions = new List<HandAction>();

                foreach (var hA in playerStatistic.HandHistory.HandActions
                    .Where(hA => hA.HandActionType != HandActionType.SMALL_BLIND && hA.HandActionType != HandActionType.BIG_BLIND &&
                            hA.HandActionType != HandActionType.ANTE && hA.HandActionType != HandActionType.POSTS))
                {
                    if (hA.PlayerName != playerStatistic.Playerstatistic.PlayerName)
                    {
                        facedHandActions.Add(hA);
                    }
                    else
                    {
                        break;
                    }
                }

                var handAction = facedHandActions.FirstOrDefault(x => x.HandActionType == HandActionType.RAISE);

                var firstRaiserPositionString = string.Empty;

                if (handAction != null)
                {
                    var firstRaiserPosition = Converter.ToPosition(playerStatistic.HandHistory, handAction.PlayerName);
                    firstRaiserPositionString = Converter.ToPositionString(firstRaiserPosition);
                }

                if (string.IsNullOrEmpty(firstRaiserPositionString))
                {
                    return fileteredList;
                }

                if ((firstRaiserPositionString == "SB" && settings.PositionSBRaiser) ||
                    (firstRaiserPositionString == "BB" && settings.PositionBBRaiser) ||
                    (firstRaiserPositionString == "EP" && settings.PositionEarlyRaiser) ||
                    (firstRaiserPositionString == "MP" && settings.PositionMiddleRaiser) ||
                    (firstRaiserPositionString == "CO" && settings.PositionCutoffRaiser) ||
                    (firstRaiserPositionString == "BTN" && settings.PositionButtonRaiser))
                {
                    fileteredList.Add(playerStatistic);
                }
            }

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByPositionThreeBetCondition(List<PlayerstatisticExtended> playerStatistics, NoteSettingsObject settings)
        {
            if (!settings.PositionBB3Bet && !settings.PositionSB3Bet && !settings.PositionButton3Bet &&
                    !settings.PositionCutoff3Bet && !settings.PositionEarly3Bet && !settings.PositionMiddle3Bet)
            {
                return playerStatistics;
            }

            var fileteredList = new List<PlayerstatisticExtended>();

            foreach (var playerStatistic in playerStatistics)
            {
                var raiser = playerStatistic.HandHistory.PreFlop.FirstOrDefault(x => x.HandActionType == HandActionType.RAISE)?.PlayerName;

                if (string.IsNullOrEmpty(raiser))
                {
                    continue;
                }

                var threeBet = new ConditionalBet();

                PlayerStatisticCalculator.Calculate3Bet(threeBet, playerStatistic.HandHistory.PreFlop.ToList(), playerStatistic.Playerstatistic.PlayerName, raiser);

                if (!threeBet.Happened)
                {
                    continue;
                }

                var firstRaiserPosition = Converter.ToPosition(playerStatistic.HandHistory, threeBet.HappenedByPlayer);
                var firstRaiserPositionString = Converter.ToPositionString(firstRaiserPosition);

                if ((firstRaiserPositionString == "SB" && settings.PositionSB3Bet) ||
                    (firstRaiserPositionString == "BB" && settings.PositionBB3Bet) ||
                    (firstRaiserPositionString == "EP" && settings.PositionEarly3Bet) ||
                    (firstRaiserPositionString == "MP" && settings.PositionMiddle3Bet) ||
                    (firstRaiserPositionString == "CO" && settings.PositionCutoff3Bet) ||
                    (firstRaiserPositionString == "BTN" && settings.PositionButton3Bet))
                {
                    fileteredList.Add(playerStatistic);
                }
            }

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByPreflopFacingCondition(List<PlayerstatisticExtended> playerstatistics, NoteSettingsObject settings)
        {
            List<PlayerstatisticExtended> fileteredList = new List<PlayerstatisticExtended>();

            if (!settings.Facing1Limper && !settings.Facing1Raiser && !settings.Facing2PlusLimpers && !settings.Facing2Raisers && !settings.FacingRaisersCallers && !settings.FacingUnopened)
                return new List<PlayerstatisticExtended>();

            if (settings.Facing1Limper && settings.Facing1Raiser && settings.Facing2PlusLimpers && settings.Facing2Raisers && settings.FacingRaisersCallers && settings.FacingUnopened)
                return playerstatistics;

            if (settings.Facing1Limper)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.FacingPreflop == EnumFacingPreflop.Limper).ToList());
            if (settings.Facing1Raiser)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.FacingPreflop == EnumFacingPreflop.Raiser).ToList());
            if (settings.Facing2PlusLimpers)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.FacingPreflop == EnumFacingPreflop.MultipleLimpers).ToList());
            if (settings.Facing2Raisers)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.FacingPreflop == EnumFacingPreflop.ThreeBet).ToList()); //todo check if correct
            if (settings.FacingRaisersCallers)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.FacingPreflop == EnumFacingPreflop.MultipleCallers).ToList()); //todo check if correct
            if (settings.FacingUnopened)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.FacingPreflop == EnumFacingPreflop.Unopened).ToList());

            return fileteredList;
        }

        private static List<PlayerstatisticExtended> FilterByPositionCondition(List<PlayerstatisticExtended> playerstatistics, NoteSettingsObject settings)
        {
            List<PlayerstatisticExtended> fileteredList = new List<PlayerstatisticExtended>();

            if (!settings.PositionBB && !settings.PositionButton && !settings.PositionSB && !settings.PositionMiddle && !settings.PositionCutoff && !settings.PositionEarly)
                return new List<PlayerstatisticExtended>();
            if (settings.PositionSB)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.Position == EnumPosition.SB).ToList());
            if (settings.PositionBB)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.Position == EnumPosition.BB).ToList());
            if (settings.PositionButton)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.Position == EnumPosition.BTN).ToList());
            if (settings.PositionCutoff)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.Position == EnumPosition.CO).ToList());
            if (settings.PositionEarly)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.Position == EnumPosition.EP || x.Playerstatistic.Position == EnumPosition.UTG || x.Playerstatistic.Position == EnumPosition.UTG_1 || x.Playerstatistic.Position == EnumPosition.UTG_2).ToList());
            if (settings.PositionMiddle)
                fileteredList.AddRange(playerstatistics.Where(x => x.Playerstatistic.Position == EnumPosition.MP || x.Playerstatistic.Position == EnumPosition.MP1 || x.Playerstatistic.Position == EnumPosition.MP2 || x.Playerstatistic.Position == EnumPosition.MP3).ToList());

            return fileteredList;
        }

        #region Filters

        private static List<PlayerstatisticExtended> FilterByAllSelectedFilters(List<PlayerstatisticExtended> playerstatistics, ICollection<FilterObject> filters, ICollection<FilterObject> filtersComparison)
        {
            List<PlayerstatisticExtended> fileteredList = playerstatistics;

            if (filters.Count == 0 && filtersComparison.Count == 0)
            {
                return fileteredList;
            }

            foreach (FilterObject filter in filters)
            {
                fileteredList = NoteManagerHelper.FilterByASelectedFilter(fileteredList, filter);
            }

            return fileteredList;
        }

        #endregion

        #region Helpers 

        private class ObligatoryAction
        {
            public ActionTypeEnum ActionType { get; set; }

            public double MaxValue { get; set; }

            public double MinValue { get; set; }
        }

        #endregion
    }
}