﻿//-----------------------------------------------------------------------
// <copyright file="FiltersHelper.cs" company="Ace Poker Solutions">
// Copyright © 2017 Ace Poker Solutions. All Rights Reserved.
// Unless otherwise noted, all materials contained in this Site are copyrights, 
// trademarks, trade dress and/or other intellectual properties, owned, 
// controlled or licensed by Ace Poker Solutions and may not be used without 
// written consent except as provided in these terms and conditions or in the 
// copyright notice (documents and software) or other proprietary notices 
// provided with the relevant materials.
// </copyright>
//----------------------------------------------------------------------

using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects;
using System.Collections.Generic;

namespace DriveHUD.PlayerXRay.DataTypes
{
    public class FiltersHelper
    {
        public static HashSet<FilterEnum> FiltersWithValueRequired = new HashSet<FilterEnum>
        {
            FilterEnum.BBsBetPreflopisBiggerThan,
            FilterEnum.BBsBetPreflopisLessThan,
            FilterEnum.BBsCalledPreflopisBiggerThan,
            FilterEnum.BBsCalledPreflopisLessThan,
            FilterEnum.BBsPutInPreflopisBiggerThan,
            FilterEnum.BBsPutInPreflopisLessThan,
            FilterEnum.PreflopRaiseSizePotisBiggerThan,
            FilterEnum.PreflopRaiseSizePotisLessThan,
            FilterEnum.PreflopFacingRaiseSizePotisBiggerThan,
            FilterEnum.PreflopFacingRaiseSizePotisLessThan,
            FilterEnum.PlayersonFlopisBiggerThan,
            FilterEnum.PlayersonFlopisLessThan,
            FilterEnum.PlayersonFlopisEqualTo,
            FilterEnum.BBsBetFlopisBiggerThan,
            FilterEnum.BBsBetFlopisLessThan,
            FilterEnum.BBsCalledFlopisBiggerThan,
            FilterEnum.BBsCalledFlopisLessThan,
            FilterEnum.BBsPutinFlopisBiggerThan,
            FilterEnum.BBsPutinFlopisLessThan,
            FilterEnum.FlopPotSizeinBBsisBiggerThan,
            FilterEnum.FlopPotSizeinBBsisLessThan,
            FilterEnum.FlopStackPotRatioisBiggerThan,
            FilterEnum.FlopStackPotRatioisLessThan,
            FilterEnum.FlopBetSizePotisBiggerThan,
            FilterEnum.FlopBetSizePotisLessThan,
            FilterEnum.FlopRaiseSizePotisBiggerThan,
            FilterEnum.FlopRaiseSizePotisLessThan,
            FilterEnum.FlopFacingBetSizePotisBiggerThan,
            FilterEnum.FlopFacingBetSizePotisLessThan,
            FilterEnum.FlopFacingRaiseSizePotisBiggerThan,
            FilterEnum.FlopFacingRaiseSizePotisLessThan,
            FilterEnum.PlayersonTurnisBiggerThan,
            FilterEnum.PlayersonTurnisLessThan,
            FilterEnum.PlayersonTurnisEqualTo,
            FilterEnum.BBsBetTurnisBiggerThan,
            FilterEnum.BBsBetTurnisLessThan,
            FilterEnum.BBsCalledTurnisBiggerThan,
            FilterEnum.BBsCalledTurnisLessThan,
            FilterEnum.BBsPutinTurnisBiggerThan,
            FilterEnum.BBsPutinTurnisLessThan,
            FilterEnum.TurnPotSizeinBBsisBiggerThan,
            FilterEnum.TurnPotSizeinBBsisLessThan,
            FilterEnum.TurnStackPotRatioisBiggerThan,
            FilterEnum.TurnStackPotRatioisLessThan,
            FilterEnum.TurnBetSizePotisBiggerThan,
            FilterEnum.TurnBetSizePotisLessThan,
            FilterEnum.TurnRaiseSizePotisBiggerThan,
            FilterEnum.TurnRaiseSizePotisLessThan,
            FilterEnum.TurnFacingBetSizePotisBiggerThan,
            FilterEnum.TurnFacingBetSizePotisLessThan,
            FilterEnum.TurnFacingRaiseSizePotisBiggerThan,
            FilterEnum.TurnFacingRaiseSizePotisLessThan,
            FilterEnum.PlayersonRiverisBiggerThan,
            FilterEnum.PlayersonRiverisLessThan,
            FilterEnum.PlayersonRiverisEqualTo,
            FilterEnum.BBsBetRiverisBiggerThan,
            FilterEnum.BBsBetRiverisLessThan,
            FilterEnum.BBsCalledRiverisBiggerThan,
            FilterEnum.BBsCalledRiverisLessThan,
            FilterEnum.BBsPutinRiverisBiggerThan,
            FilterEnum.BBsPutinRiverisLessThan,
            FilterEnum.RiverPotSizeinBBsisBiggerThan,
            FilterEnum.RiverPotSizeinBBsisLessThan,
            FilterEnum.RiverStackPotRatioisBiggerThan,
            FilterEnum.RiverStackPotRatioisLessThan,
            FilterEnum.RiverBetSizePotisBiggerThan,
            FilterEnum.RiverBetSizePotisLessThan,
            FilterEnum.RiverRaiseSizePotisBiggerThan,
            FilterEnum.RiverRaiseSizePotisLessThan,
            FilterEnum.RiverFacingBetSizePotisBiggerThan,
            FilterEnum.RiverFacingBetSizePotisLessThan,
            FilterEnum.RiverFacingRaiseSizePotisBiggerThan,
            FilterEnum.RiverFacingRaiseSizePotisLessThan,
            FilterEnum.FinalPotSizeinBBsisBiggerThan,
            FilterEnum.FinalPotSizeinBBsisLessThan,
            FilterEnum.PlayerWonBBsIsBiggerThan,
            FilterEnum.PlayerWonBBsIsLessThan,
            FilterEnum.PlayerLostBBsIsBiggerThan,
            FilterEnum.PlayerLostBBsIsLessThan,
            FilterEnum.PlayerWonOrLostBBsIsBiggerThan,
            FilterEnum.PlayerWonOrLostBBsIsLessThan,
            FilterEnum.PlayersSawShowdownIsBiggerThan,
            FilterEnum.PlayersSawShowdownIsLessThan,
            FilterEnum.PlayersSawShowdownIsEqualTo,
            FilterEnum.AllinWinIsBiggerThan,
            FilterEnum.AllinWinIsLessThan
        };

        public static HashSet<FilterEnum> PercentBasedFilters = new HashSet<FilterEnum>
        {
            FilterEnum.PreflopRaiseSizePotisBiggerThan,
            FilterEnum.PreflopRaiseSizePotisLessThan,
            FilterEnum.PreflopFacingRaiseSizePotisBiggerThan,
            FilterEnum.PreflopFacingRaiseSizePotisLessThan,
            FilterEnum.FlopBetSizePotisBiggerThan,
            FilterEnum.FlopBetSizePotisLessThan,
            FilterEnum.FlopRaiseSizePotisBiggerThan,
            FilterEnum.FlopRaiseSizePotisLessThan,
            FilterEnum.FlopFacingBetSizePotisBiggerThan,
            FilterEnum.FlopFacingBetSizePotisLessThan,
            FilterEnum.FlopFacingRaiseSizePotisBiggerThan,
            FilterEnum.FlopFacingRaiseSizePotisLessThan,
            FilterEnum.TurnBetSizePotisBiggerThan,
            FilterEnum.TurnBetSizePotisLessThan,
            FilterEnum.TurnRaiseSizePotisBiggerThan,
            FilterEnum.TurnRaiseSizePotisLessThan,
            FilterEnum.TurnFacingBetSizePotisBiggerThan,
            FilterEnum.TurnFacingBetSizePotisLessThan,
            FilterEnum.TurnFacingRaiseSizePotisBiggerThan,
            FilterEnum.TurnFacingRaiseSizePotisLessThan,
            FilterEnum.RiverBetSizePotisBiggerThan,
            FilterEnum.RiverBetSizePotisLessThan,
            FilterEnum.RiverRaiseSizePotisBiggerThan,
            FilterEnum.RiverRaiseSizePotisLessThan,
            FilterEnum.RiverFacingBetSizePotisBiggerThan,
            FilterEnum.RiverFacingBetSizePotisLessThan,
            FilterEnum.RiverFacingRaiseSizePotisBiggerThan,
            FilterEnum.RiverFacingRaiseSizePotisLessThan
        };

        public static IEnumerable<FilterObject> GetFiltersObjects()
        {
            return new[]
            {
                // PREFLOP
                Create(NoteStageType.PreFlop, "VPIP", FilterEnum.VPIP),
                Create(NoteStageType.PreFlop, "Put $ in Pot", FilterEnum.PutMoneyinPot),
                Create(NoteStageType.PreFlop, "PFR", FilterEnum.PFR),
                Create(NoteStageType.PreFlop, "PFR = False", FilterEnum.PFRFalse),
                Create(NoteStageType.PreFlop, "Did 3-bet", FilterEnum.Did3Bet),
                Create(NoteStageType.PreFlop, "Did Squeeze", FilterEnum.DidSqueeze),
                Create(NoteStageType.PreFlop, "Did Cold Call", FilterEnum.DidColdCall),
                Create(NoteStageType.PreFlop, "Could Cold Call", FilterEnum.CouldColdCall),
                Create(NoteStageType.PreFlop, "Could Cold Call = False", FilterEnum.CouldColdCallFalse),
                Create(NoteStageType.PreFlop, "Faced Preflop 3 bet", FilterEnum.FacedPreflop3Bet),
                Create(NoteStageType.PreFlop, "Folded to Preflop 3 bet", FilterEnum.FoldedToPreflop3Bet),
                Create(NoteStageType.PreFlop, "Called Preflop 3 bet", FilterEnum.CalledPreflop3Bet),
                Create(NoteStageType.PreFlop, "Raised Preflop 3 bet", FilterEnum.RaisedPreflop3Bet),
                Create(NoteStageType.PreFlop, "Faced Preflop 4 bet", FilterEnum.FacedPreflop4Bet),
                Create(NoteStageType.PreFlop, "Folded to Preflop 4 bet", FilterEnum.FoldedToPreflop4Bet),
                Create(NoteStageType.PreFlop, "Called Preflop 4 bet", FilterEnum.CalledPreflop4Bbet),
                Create(NoteStageType.PreFlop, "Raised Preflop 4 bet", FilterEnum.RaisedPreflop4Bet),
                Create(NoteStageType.PreFlop, "In BB and Steal Attempted", FilterEnum.InBBandStealAttempted),
                Create(NoteStageType.PreFlop, "In BB and Steal Defended", FilterEnum.InBBandStealDefended),
                Create(NoteStageType.PreFlop, "In BB and Steal Reraised", FilterEnum.InBBandStealReraised),
                Create(NoteStageType.PreFlop, "In SB and Steal Attempted", FilterEnum.InSBandStealAttempted),
                Create(NoteStageType.PreFlop, "In SB and Steal Defended", FilterEnum.InSBandStealDefended),
                Create(NoteStageType.PreFlop, "In SB and Steal Reraised", FilterEnum.InSBandStealReraised),
                Create(NoteStageType.PreFlop, "Limp Reraised", FilterEnum.LimpReraised),
                Create(NoteStageType.PreFlop, "BBs Bet Preflop is Bigger Than...", FilterEnum.BBsBetPreflopisBiggerThan),
                Create(NoteStageType.PreFlop, "BBs Bet Preflop is Less Than...", FilterEnum.BBsBetPreflopisLessThan),
                Create(NoteStageType.PreFlop, "BBs Called Preflop is Bigger Than...", FilterEnum.BBsCalledPreflopisBiggerThan),
                Create(NoteStageType.PreFlop, "BBs Called Preflop is Less Than...", FilterEnum.BBsCalledPreflopisLessThan),
                Create(NoteStageType.PreFlop, "BBs Put In Preflop is Bigger Than...", FilterEnum.BBsPutInPreflopisBiggerThan),
                Create(NoteStageType.PreFlop, "BBs Put In Preflop is Less Than...", FilterEnum.BBsPutInPreflopisLessThan),
                Create(NoteStageType.PreFlop, "Preflop Raise Size / Pot is Bigger Than...", FilterEnum.PreflopRaiseSizePotisBiggerThan),
                Create(NoteStageType.PreFlop, "Preflop Raise Size / Pot is Less Than...", FilterEnum.PreflopRaiseSizePotisLessThan),
                Create(NoteStageType.PreFlop, "Preflop Facing Raise Size / Pot is Bigger Than...", FilterEnum.PreflopFacingRaiseSizePotisBiggerThan),
                Create(NoteStageType.PreFlop, "Preflop Facing Raise Size / Pot is Less Than...", FilterEnum.PreflopFacingRaiseSizePotisLessThan),
                Create(NoteStageType.PreFlop, "Allin Preflop", FilterEnum.AllinPreflop),
                // FLOP
                Create(NoteStageType.Flop, "Saw Flop", FilterEnum.SawFlop),
                Create(NoteStageType.Flop, "Saw Flop = False", FilterEnum.SawFlopFalse),
                Create(NoteStageType.Flop, "Last to Act on Flop", FilterEnum.LasttoActionFlop),
                Create(NoteStageType.Flop, "Last to Act on Flop = False", FilterEnum.LasttoActionFlopFalse),
                Create(NoteStageType.Flop, "Flop Unopened", FilterEnum.FlopUnopened),
                Create(NoteStageType.Flop, "Players on Flop is Bigger Than...", FilterEnum.PlayersonFlopisBiggerThan),
                Create(NoteStageType.Flop, "Players on Flop is Less Than...", FilterEnum.PlayersonFlopisLessThan),
                Create(NoteStageType.Flop, "Players on Flop is Equal To...", FilterEnum.PlayersonFlopisEqualTo),
                Create(NoteStageType.Flop, "Flop Continuation Bet Possible", FilterEnum.FlopContinuationBetPossible),
                Create(NoteStageType.Flop, "Flop Continuation Bet Made", FilterEnum.FlopContinuationBetMade),
                Create(NoteStageType.Flop, "Facing Flop Continuation Bet", FilterEnum.FacingFlopContinuationBet),
                Create(NoteStageType.Flop, "Folded to Flop Continuation Bet", FilterEnum.FoldedtoFlopContinuationBet),
                Create(NoteStageType.Flop, "Called Flop Continuation Bet", FilterEnum.CalledFlopContinuationBet),
                Create(NoteStageType.Flop, "Raised Flop Continuation Bet", FilterEnum.RaisedFlopContinuationBet),
                Create(NoteStageType.Flop, "Flop Bet", FilterEnum.FlopBet),
                Create(NoteStageType.Flop, "Flop Bet Fold", FilterEnum.FlopBetFold),
                Create(NoteStageType.Flop, "Flop Bet Call", FilterEnum.FlopBetCall),
                Create(NoteStageType.Flop, "Flop Bet Raise", FilterEnum.FlopBetRaise),
                Create(NoteStageType.Flop, "Flop Raise", FilterEnum.FlopRaise),
                Create(NoteStageType.Flop, "Flop Raise Fold", FilterEnum.FlopRaiseFold),
                Create(NoteStageType.Flop, "Flop Raise Call", FilterEnum.FlopRaiseCall),
                Create(NoteStageType.Flop, "Flop Raise Raise", FilterEnum.FlopRaiseRaise),
                Create(NoteStageType.Flop, "Flop Call", FilterEnum.FlopCall),
                Create(NoteStageType.Flop, "Flop Call Fold", FilterEnum.FlopCallFold),
                Create(NoteStageType.Flop, "Flop Call Call", FilterEnum.FlopCallCall),
                Create(NoteStageType.Flop, "Flop Call Raise", FilterEnum.FlopCallRaise),
                Create(NoteStageType.Flop, "Flop Check", FilterEnum.FlopCheck),
                Create(NoteStageType.Flop, "Flop Check Fold", FilterEnum.FlopCheckFold),
                Create(NoteStageType.Flop, "Flop Check Call", FilterEnum.FlopCheckCall),
                Create(NoteStageType.Flop, "Flop Check Raise", FilterEnum.FlopCheckRaise),
                Create(NoteStageType.Flop, "Flop Fold", FilterEnum.FlopFold),
                Create(NoteStageType.Flop, "Flop Was Check Raised", FilterEnum.FlopWasCheckRaised),
                Create(NoteStageType.Flop, "Flop Was Bet Into", FilterEnum.FlopWasBetInto),
                Create(NoteStageType.Flop, "Flop Was Raised", FilterEnum.FlopWasRaised),
                Create(NoteStageType.Flop, "BBs Bet Flop is Bigger Than...", FilterEnum.BBsBetFlopisBiggerThan),
                Create(NoteStageType.Flop, "BBs Bet Flop is Less Than...", FilterEnum.BBsBetFlopisLessThan),
                Create(NoteStageType.Flop, "BBs Called Flop is Bigger Than...", FilterEnum.BBsCalledFlopisBiggerThan),
                Create(NoteStageType.Flop, "BBs Called Flop is Less Than...", FilterEnum.BBsCalledFlopisLessThan),
                Create(NoteStageType.Flop, "BBs Put in Flop is Bigger Than...", FilterEnum.BBsPutinFlopisBiggerThan),
                Create(NoteStageType.Flop, "BBs Put in Flop is Less Than...", FilterEnum.BBsPutinFlopisLessThan),
                Create(NoteStageType.Flop, "Flop Pot Size in BBs is Bigger Than...", FilterEnum.FlopPotSizeinBBsisBiggerThan),
                Create(NoteStageType.Flop, "Flop Pot Size in BBs is Less Than...", FilterEnum.FlopPotSizeinBBsisLessThan),
                Create(NoteStageType.Flop, "Flop Stack Pot Ratio is Bigger Than...", FilterEnum.FlopStackPotRatioisBiggerThan),
                Create(NoteStageType.Flop, "Flop Stack Pot Ratio is Less Than...", FilterEnum.FlopStackPotRatioisLessThan),
                Create(NoteStageType.Flop, "Flop Bet Size / Pot is Bigger Than...", FilterEnum.FlopBetSizePotisBiggerThan),
                Create(NoteStageType.Flop, "Flop Bet Size / Pot is Less Than...", FilterEnum.FlopBetSizePotisLessThan),
                Create(NoteStageType.Flop, "Flop Raise Size / Pot is Bigger Than...", FilterEnum.FlopRaiseSizePotisBiggerThan),
                Create(NoteStageType.Flop, "Flop Raise Size / Pot is Less Than...", FilterEnum.FlopRaiseSizePotisLessThan),
                Create(NoteStageType.Flop, "Flop Facing Bet Size / Pot is Bigger Than...", FilterEnum.FlopFacingBetSizePotisBiggerThan),
                Create(NoteStageType.Flop, "Flop Facing Bet Size / Pot is Less Than...", FilterEnum.FlopFacingBetSizePotisLessThan),
                Create(NoteStageType.Flop, "Flop Facing Raise Size / Pot is Bigger Than...", FilterEnum.FlopFacingRaiseSizePotisBiggerThan),
                Create(NoteStageType.Flop, "Flop Facing Raise Size / Pot is Less Than...", FilterEnum.FlopFacingRaiseSizePotisLessThan),
                Create(NoteStageType.Flop, "Allin on Flop", FilterEnum.AllinOnFlop),
                Create(NoteStageType.Flop, "Allin on Flop (or earlier)", FilterEnum.AllinOnFlopOrEarlier),
                // TURN
                Create(NoteStageType.Turn, "Saw Turn", FilterEnum.SawTurn),
                Create(NoteStageType.Turn, "Last to Act on Turn", FilterEnum.LasttoActonTurn),
                Create(NoteStageType.Turn, "Last to Act on Turn = False", FilterEnum.LasttoActonTurnFalse),
                Create(NoteStageType.Turn, "Turn Unopened", FilterEnum.TurnUnopened),
                Create(NoteStageType.Turn, "Turn Unopened = False", FilterEnum.TurnUnopenedFalse),
                Create(NoteStageType.Turn, "Players on Turn is Bigger Than...", FilterEnum.PlayersonTurnisBiggerThan),
                Create(NoteStageType.Turn, "Players on Turn is Less Than...", FilterEnum.PlayersonTurnisLessThan),
                Create(NoteStageType.Turn, "Players on Turn is Equal To...", FilterEnum.PlayersonTurnisEqualTo),
                Create(NoteStageType.Turn, "Turn Continuation Bet Possible", FilterEnum.TurnContinuationBetPossible),
                Create(NoteStageType.Turn, "Turn Continuation Bet Made", FilterEnum.TurnContinuationBetMade),
                Create(NoteStageType.Turn, "Facing Turn Continuation Bet", FilterEnum.FacingTurnContinuationBet),
                Create(NoteStageType.Turn, "Folded to Turn Continuation Bet", FilterEnum.FoldedtoTurnContinuationBet),
                Create(NoteStageType.Turn, "Called Turn Continuation Bet", FilterEnum.CalledTurnContinuationBet),
                Create(NoteStageType.Turn, "Raised Turn Continuation Bet", FilterEnum.RaisedTurnContinuationBet),
                Create(NoteStageType.Turn, "Turn Bet", FilterEnum.TurnBet),
                Create(NoteStageType.Turn, "Turn Bet Fold", FilterEnum.TurnBetFold),
                Create(NoteStageType.Turn, "Turn Bet Call", FilterEnum.TurnBetCall),
                Create(NoteStageType.Turn, "Turn Bet Raise", FilterEnum.TurnBetRaise),
                Create(NoteStageType.Turn, "Turn Raise", FilterEnum.TurnRaise),
                Create(NoteStageType.Turn, "Turn Raise Fold", FilterEnum.TurnRaiseFold),
                Create(NoteStageType.Turn, "Turn Raise Call", FilterEnum.TurnRaiseCall),
                Create(NoteStageType.Turn, "Turn Raise Raise", FilterEnum.TurnRaiseRaise),
                Create(NoteStageType.Turn, "Turn Call", FilterEnum.TurnCall),
                Create(NoteStageType.Turn, "Turn Call Fold", FilterEnum.TurnCallFold),
                Create(NoteStageType.Turn, "Turn Call Call", FilterEnum.TurnCallCall),
                Create(NoteStageType.Turn, "Turn Call Raise", FilterEnum.TurnCallRaise),
                Create(NoteStageType.Turn, "Turn Check", FilterEnum.TurnCheck),
                Create(NoteStageType.Turn, "Turn Check Fold", FilterEnum.TurnCheckFold),
                Create(NoteStageType.Turn, "Turn Check Call", FilterEnum.TurnCheckCall),
                Create(NoteStageType.Turn, "Turn Check Raise", FilterEnum.TurnCheckRaise),
                Create(NoteStageType.Turn, "Turn Fold", FilterEnum.TurnFold),
                Create(NoteStageType.Turn, "Turn Was Check Raised", FilterEnum.TurnWasCheckRaised),
                Create(NoteStageType.Turn, "Turn Was Bet Into", FilterEnum.TurnWasBetInto),
                Create(NoteStageType.Turn, "Turn Was Raised", FilterEnum.TurnWasRaised),
                Create(NoteStageType.Turn, "BBs Bet Turn is Bigger Than...", FilterEnum.BBsBetTurnisBiggerThan),
                Create(NoteStageType.Turn, "BBs Bet Turn is Less Than...", FilterEnum.BBsBetTurnisLessThan),
                Create(NoteStageType.Turn, "BBs Called Turn is Bigger Than...", FilterEnum.BBsCalledTurnisBiggerThan),
                Create(NoteStageType.Turn, "BBs Called Turn is Less Than...", FilterEnum.BBsCalledTurnisLessThan),
                Create(NoteStageType.Turn, "BBs Put in Turn is Bigger Than...", FilterEnum.BBsPutinTurnisBiggerThan),
                Create(NoteStageType.Turn, "BBs Put in Turn is Less Than...", FilterEnum.BBsPutinTurnisLessThan),
                Create(NoteStageType.Turn, "Turn Pot Size in BBs is Bigger Than...", FilterEnum.TurnPotSizeinBBsisBiggerThan),
                Create(NoteStageType.Turn, "Turn Pot Size in BBs is Less Than...", FilterEnum.TurnPotSizeinBBsisLessThan),
                Create(NoteStageType.Turn, "Turn Stack Pot Ratio is Bigger Than...", FilterEnum.TurnStackPotRatioisBiggerThan),
                Create(NoteStageType.Turn, "Turn Stack Pot Ratio is Less Than...", FilterEnum.TurnStackPotRatioisLessThan),
                Create(NoteStageType.Turn, "Turn Bet Size / Pot is Bigger Than...", FilterEnum.TurnBetSizePotisBiggerThan),
                Create(NoteStageType.Turn, "Turn Bet Size / Pot is Less Than...", FilterEnum.TurnBetSizePotisLessThan),
                Create(NoteStageType.Turn, "Turn Raise Size / Pot is Bigger Than...", FilterEnum.TurnRaiseSizePotisBiggerThan),
                Create(NoteStageType.Turn, "Turn Raise Size / Pot is Less Than...", FilterEnum.TurnRaiseSizePotisLessThan),
                Create(NoteStageType.Turn, "Turn Facing Bet Size / Pot is Bigger Than...", FilterEnum.TurnFacingBetSizePotisBiggerThan),
                Create(NoteStageType.Turn, "Turn Facing Bet Size / Pot is Less Than...", FilterEnum.TurnFacingBetSizePotisLessThan),
                Create(NoteStageType.Turn, "Turn Facing Raise Size / Pot is Bigger Than...", FilterEnum.TurnFacingRaiseSizePotisBiggerThan),
                Create(NoteStageType.Turn, "Turn Facing Raise Size / Pot is Less Than...", FilterEnum.TurnFacingRaiseSizePotisLessThan),
                Create(NoteStageType.Turn, "Allin on Turn", FilterEnum.AllinonTurn),
                Create(NoteStageType.Turn, "Allin on Turn (or earlier)", FilterEnum.AllinonTurnOrEarlier),
                // RIVER
                Create(NoteStageType.River, "Saw River", FilterEnum.SawRiver),
                Create(NoteStageType.River, "Last to Act on River", FilterEnum.LasttoActonRiver),
                Create(NoteStageType.River, "Last to Act on River = False", FilterEnum.LasttoActonRiverFalse),
                Create(NoteStageType.River, "River Unopened", FilterEnum.RiverUnopened),
                Create(NoteStageType.River, "River Unopened = False", FilterEnum.RiverUnopenedFalse),
                Create(NoteStageType.River, "Players on River is Bigger Than...", FilterEnum.PlayersonRiverisBiggerThan),
                Create(NoteStageType.River, "Players on River is Less Than...", FilterEnum.PlayersonRiverisLessThan),
                Create(NoteStageType.River, "Players on River is Equal To...", FilterEnum.PlayersonRiverisEqualTo),
                Create(NoteStageType.River, "River Continuation Bet Possible", FilterEnum.RiverContinuationBetPossible),
                Create(NoteStageType.River, "River Continuation Bet Made", FilterEnum.RiverContinuationBetMade),
                Create(NoteStageType.River, "Facing River Continuation Bet", FilterEnum.FacingRiverContinuationBet),
                Create(NoteStageType.River, "Folded to River Continuation Bet", FilterEnum.FoldedtoRiverContinuationBet),
                Create(NoteStageType.River, "Called River Continuation Bet", FilterEnum.CalledRiverContinuationBet),
                Create(NoteStageType.River, "Raised River Continuation Bet", FilterEnum.RaisedRiverContinuationBet),
                Create(NoteStageType.River, "River Bet", FilterEnum.RiverBet),
                Create(NoteStageType.River, "River Bet Fold", FilterEnum.RiverBetFold),
                Create(NoteStageType.River, "River Bet Call", FilterEnum.RiverBetCall),
                Create(NoteStageType.River, "River Bet Raise", FilterEnum.RiverBetRaise),
                Create(NoteStageType.River, "River Raise", FilterEnum.RiverRaise),
                Create(NoteStageType.River, "River Raise Fold", FilterEnum.RiverRaiseFold),
                Create(NoteStageType.River, "River Raise Call", FilterEnum.RiverRaiseCall),
                Create(NoteStageType.River, "River Raise Raise", FilterEnum.RiverRaiseRaise),
                Create(NoteStageType.River, "River Call", FilterEnum.RiverCall),
                Create(NoteStageType.River, "River Call Fold", FilterEnum.RiverCallFold),
                Create(NoteStageType.River, "River Call Call", FilterEnum.RiverCallCall),
                Create(NoteStageType.River, "River Call Raise", FilterEnum.RiverCallRaise),
                Create(NoteStageType.River, "River Check", FilterEnum.RiverCheck),
                Create(NoteStageType.River, "River Check Fold", FilterEnum.RiverCheckFold),
                Create(NoteStageType.River, "River Check Call", FilterEnum.RiverCheckCall),
                Create(NoteStageType.River, "River Check Raise", FilterEnum.RiverCheckRaise),
                Create(NoteStageType.River, "River Fold", FilterEnum.RiverFold),
                Create(NoteStageType.River, "River Was Check Raised", FilterEnum.RiverWasCheckRaised),
                Create(NoteStageType.River, "River Was Bet Into", FilterEnum.RiverWasBetInto),
                Create(NoteStageType.River, "River Was Raised", FilterEnum.RiverWasRaised),
                Create(NoteStageType.River, "BBs Bet River is Bigger Than...", FilterEnum.BBsBetRiverisBiggerThan),
                Create(NoteStageType.River, "BBs Bet River is Less Than...", FilterEnum.BBsBetRiverisLessThan),
                Create(NoteStageType.River, "BBs Called River is Bigger Than...", FilterEnum.BBsCalledRiverisBiggerThan),
                Create(NoteStageType.River, "BBs Called River is Less Than...", FilterEnum.BBsCalledRiverisLessThan),
                Create(NoteStageType.River, "BBs Put in River is Bigger Than...", FilterEnum.BBsPutinRiverisBiggerThan),
                Create(NoteStageType.River, "BBs Put in River is Less Than...", FilterEnum.BBsPutinRiverisLessThan),
                Create(NoteStageType.River, "River Pot Size in BBs is Bigger Than...", FilterEnum.RiverPotSizeinBBsisBiggerThan),
                Create(NoteStageType.River, "River Pot Size in BBs is Less Than...", FilterEnum.RiverPotSizeinBBsisLessThan),
                Create(NoteStageType.River, "River Stack Pot Ratio is Bigger Than...", FilterEnum.RiverStackPotRatioisBiggerThan),
                Create(NoteStageType.River, "River Stack Pot Ratio is Less Than...", FilterEnum.RiverStackPotRatioisLessThan),
                Create(NoteStageType.River, "River Bet Size / Pot is Bigger Than...", FilterEnum.RiverBetSizePotisBiggerThan),
                Create(NoteStageType.River, "River Bet Size / Pot is Less Than...", FilterEnum.RiverBetSizePotisLessThan),
                Create(NoteStageType.River, "River Raise Size / Pot is Bigger Than...", FilterEnum.RiverRaiseSizePotisBiggerThan),
                Create(NoteStageType.River, "River Raise Size / Pot is Less Than...", FilterEnum.RiverRaiseSizePotisLessThan),
                Create(NoteStageType.River, "River Facing Bet Size / Pot is Bigger Than...", FilterEnum.RiverFacingBetSizePotisBiggerThan),
                Create(NoteStageType.River, "River Facing Bet Size / Pot is Less Than...", FilterEnum.RiverFacingBetSizePotisLessThan),
                Create(NoteStageType.River, "River Facing Raise Size / Pot is Bigger Than...", FilterEnum.RiverFacingRaiseSizePotisBiggerThan),
                Create(NoteStageType.River, "River Facing Raise Size / Pot is Less Than...", FilterEnum.RiverFacingRaiseSizePotisLessThan),
                // OTHERS
                Create(NoteStageType.Other, "Saw Showdown", FilterEnum.SawShowdown),
                Create(NoteStageType.Other, "Won Hand", FilterEnum.WonHand),
                Create(NoteStageType.Other, "Final Pot Size in BBs is Bigger Than...", FilterEnum.FinalPotSizeinBBsisBiggerThan),
                Create(NoteStageType.Other, "Final Pot Size in BBs is Less Than...", FilterEnum.FinalPotSizeinBBsisLessThan),
                Create(NoteStageType.Other, "Player Won BBs is Bigger Than...", FilterEnum.PlayerWonBBsIsBiggerThan),
                Create(NoteStageType.Other, "Player Won BBs is Less Than...", FilterEnum.PlayerWonBBsIsLessThan),
                Create(NoteStageType.Other, "Player Lost BBs is Bigger Than...", FilterEnum.PlayerLostBBsIsBiggerThan),
                Create(NoteStageType.Other, "Player Lost BBs is Less Than...", FilterEnum.PlayerLostBBsIsLessThan),
                Create(NoteStageType.Other, "Player Won or Lost BBs is Bigger Than...", FilterEnum.PlayerWonOrLostBBsIsBiggerThan),
                Create(NoteStageType.Other, "Player Won or Lost BBs is Less Than...", FilterEnum.PlayerWonOrLostBBsIsLessThan),
                Create(NoteStageType.Other, "Players Saw Showdown is Bigger Than...", FilterEnum.PlayersSawShowdownIsBiggerThan),
                Create(NoteStageType.Other, "Players Saw Showdown is Less Than...", FilterEnum.PlayersSawShowdownIsLessThan),
                Create(NoteStageType.Other, "Players Saw Showdown is Equal To...", FilterEnum.PlayersSawShowdownIsEqualTo),
                Create(NoteStageType.Other, "Allin Win% is Bigger Than...", FilterEnum.AllinWinIsBiggerThan),
                Create(NoteStageType.Other, "Allin Win% is Less Than...", FilterEnum.AllinWinIsLessThan)
            };
        }

        private static FilterObject Create(NoteStageType stage, string name, FilterEnum filter)
        {
            return new FilterObject
            {
                Description = name,
                Stage = stage,
                Filter = filter
            };
        }
    }
}