//-----------------------------------------------------------------------
// <copyright file="NoteTests.cs" company="Ace Poker Solutions">
// Copyright © 2017 Ace Poker Solutions. All Rights Reserved.
// Unless otherwise noted, all materials contained in this Site are copyrights, 
// trademarks, trade dress and/or other intellectual properties, owned, 
// controlled or licensed by Ace Poker Solutions and may not be used without 
// written consent except as provided in these terms and conditions or in the 
// copyright notice (documents and software) or other proprietary notices 
// provided with the relevant materials.
// </copyright>
//----------------------------------------------------------------------

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Model;
using HandHistories.Parser.Parsers;
using DriveHUD.Entities;
using HandHistories.Parser.Parsers.Factory;
using Microsoft.Practices.ServiceLocation;
using DriveHUD.Common.Utils;
using DriveHUD.PlayerXRay.Services;
using HandHistories.Objects.Hand;
using DriveHUD.PlayerXRay.DataTypes.NotesTreeObjects;
using NSubstitute;
using DriveHUD.PlayerXRay.Licensing;
using DriveHUD.PlayerXRay.DataTypes;
using System.IO;

namespace DriveHUD.PlayerXRay.Tests
{
    [TestFixture]
    class NoteTests : BaseNoteTests
    {
        #region SetUp

        /// <summary>
        /// Initialize environment for test
        /// </summary>
        [OneTimeSetUp]
        public virtual void SetUp()
        {
            Initalize();
        }

        #endregion

        private const string testDataFolder = @"..\..\TestData";

        protected override string TestDataFolder
        {
            get
            {
                return testDataFolder;
            }
        }

        protected override void InitializeContainer(UnityContainer unityContainer)
        {
            unityContainer.RegisterType<IPlayerStatisticCalculator, PlayerStatisticCalculator>();
            unityContainer.RegisterType<IHandHistoryParserFactory, HandHistoryParserFactoryImpl>();

            InitializeLicenseService(unityContainer);
        }

        protected virtual void InitializeLicenseService(UnityContainer unityContainer)
        {
            var licenseInfo = Substitute.For<ILicenseInfo>();
            licenseInfo.IsRegistered.Returns(true);
            licenseInfo.CashLimit.Returns(int.MaxValue);
            licenseInfo.TournamentLimit.Returns(int.MaxValue);
            licenseInfo.LicenseType.Returns(LicenseType.Combo);

            var licenceService = Substitute.For<ILicenseService>();
            licenceService.IsRegistered.Returns(true);
            licenceService.LicenseInfos.Returns(new[] { licenseInfo });
            unityContainer.RegisterInstance(licenceService);
        }

        [Test]
     
        [TestCase("HeroFirstPreflopActionIsCall.txt", EnumPokerSites.PokerStars, "DURKADURDUR", ActionTypeEnum.Call, 10, 60, true)]
        [TestCase("HeroFirstPreflopActionIsCall.txt", EnumPokerSites.PokerStars, "DURKADURDUR", ActionTypeEnum.Call, 60, 80, false)]
        public void TestFirstPreflopPlayerAction(string handHistoryFile, EnumPokerSites pokerSite, string playerName, ActionTypeEnum actionType, double min, double max, bool expected)
        {
            var handHistoryObject = CreateHandHistoryObject(handHistoryFile, pokerSite, playerName);

            var note = new NoteObject();     
            note.Settings.PreflopActions.FirstType = actionType;
            note.Settings.PreflopActions.FirstMaxValue = max;
            note.Settings.PreflopActions.FirstMinValue = min;

            var noteProcessingService = new NoteProcessingService();

            var playernotes = noteProcessingService.ProcessHand(new[] { note }, handHistoryObject.Stat, handHistoryObject.HandHistory);

            Assert.IsNotNull(playernotes, "Notes must be not null");
            Assert.That(playernotes.Any(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase("HeroSecondPreflopActionIsFold.xml", EnumPokerSites.IPoker, "Hero", ActionTypeEnum.Fold, 0, 0, true)]
        [TestCase("HeroSecondPreflopActionIsFold.xml", EnumPokerSites.IPoker, "Hero", ActionTypeEnum.Any, 0, 0, true)]
        [TestCase("HeroSecondPreflopActionIsRaise.xml", EnumPokerSites.IPoker, "Hero", ActionTypeEnum.Raise, 0, 0, true)]
        [TestCase("HeroSecondPreflopActionIsRaise.xml", EnumPokerSites.IPoker, "Hero", ActionTypeEnum.Any, 0, 0, true)]
        [TestCase("HeroSecondPreflopActionIsRaise.xml", EnumPokerSites.IPoker, "Hero", ActionTypeEnum.Raise, 0, 50, false)]
        [TestCase("HeroSecondPreflopActionIsRaise.xml", EnumPokerSites.IPoker, "Hero", ActionTypeEnum.Raise, 100, 200, true)]
        public void TestSecondPreflopPlayerAction(string handHistoryFile, EnumPokerSites pokerSite, string playerName, ActionTypeEnum actionType, double min, double max, bool expected)
        {
            var handHistoryObject = CreateHandHistoryObject(handHistoryFile, pokerSite, playerName);

            var note = new NoteObject();          
            note.Settings.PreflopActions.SecondType = actionType;
            note.Settings.PreflopActions.SecondMaxValue = max;
            note.Settings.PreflopActions.SecondMinValue = min;

            var noteProcessingService = new NoteProcessingService();

            var playernotes = noteProcessingService.ProcessHand(new[] { note }, handHistoryObject.Stat, handHistoryObject.HandHistory);

            Assert.IsNotNull(playernotes, "Notes must be not null");
            Assert.That(playernotes.Any(), Is.EqualTo(expected));
        }

        private HandHistoryObject CreateHandHistoryObject(string fileName, EnumPokerSites pokerSite, string playerName)
        {
            var handHistoryFileFullName = Path.Combine(TestDataFolder, fileName);

            var handHistoryFileInfo = new FileInfo(handHistoryFileFullName);

            Assert.That(handHistoryFileInfo.Exists, $"{handHistoryFileFullName} doesn't exists. Please check.");

            var handHistoryText = File.ReadAllText(handHistoryFileInfo.FullName);

            var parsingResult = ParseHandHistory(pokerSite, handHistoryText);
            var player = parsingResult.Players.FirstOrDefault(x => x.Playername == playerName);

            Assert.IsNotNull(player, $"Player {playerName} has not been found");

            var playerStatisticCalculator = ServiceLocator.Current.GetInstance<IPlayerStatisticCalculator>();

            var stat = playerStatisticCalculator.CalculateStatistic(parsingResult, player);

            return new HandHistoryObject
            {
                HandHistory = parsingResult.Source,
                Stat = stat
            };
        }

        private ParsingResult ParseHandHistory(EnumPokerSites pokerSite, string handHistoryText)
        {
            var pokerSiteNetwork = EntityUtils.GetSiteNetwork(pokerSite);

            var handHistoryParserFactory = ServiceLocator.Current.GetInstance<IHandHistoryParserFactory>();

            var handHistoryParser = pokerSite == EnumPokerSites.Unknown || pokerSiteNetwork == EnumPokerNetworks.WPN ?
                handHistoryParserFactory.GetFullHandHistoryParser(handHistoryText) :
                handHistoryParserFactory.GetFullHandHistoryParser(pokerSite);

            var parsedHand = handHistoryParser.ParseFullHandHistory(handHistoryText, true);

            var gameType = new Gametypes
            {
                Anteincents = Utils.ConvertToCents(parsedHand.GameDescription.Limit.Ante),
                Bigblindincents = Utils.ConvertToCents(parsedHand.GameDescription.Limit.BigBlind),
                CurrencytypeId = (short)parsedHand.GameDescription.Limit.Currency,
                Istourney = parsedHand.GameDescription.IsTournament,
                PokergametypeId = (short)(parsedHand.GameDescription.GameType),
                Smallblindincents = Utils.ConvertToCents(parsedHand.GameDescription.Limit.SmallBlind),
                Tablesize = (short)parsedHand.GameDescription.SeatType.MaxPlayers
            };

            var players = parsedHand.Players.Select(player => new Players
            {
                Playername = player.PlayerName,
                PokersiteId = (short)pokerSite
            }).ToList();

            var parsingResult = new ParsingResult
            {
                HandHistory = new Handhistory { PokersiteId = (short)pokerSite, HandhistoryVal = handHistoryText },
                Players = players,
                GameType = gameType,
                Source = parsedHand
            };

            return parsingResult;
        }

        private class HandHistoryObject
        {
            public HandHistory HandHistory { get; set; }

            public Playerstatistic Stat { get; set; }
        }

    }
}