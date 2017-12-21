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
using System.Xml.Serialization;
using DriveHUD.PlayerXRay.BusinessHelper.ApplicationSettings;

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

        private NotesAppSettings PredefinedNotesAppSettings { get; set; }

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
            licenseInfo.LicenseType.Returns(LicenseType.XRayCombo);

            var licenceService = Substitute.For<ILicenseService>();
            licenceService.IsRegistered.Returns(true);
            licenceService.LicenseInfos.Returns(new[] { licenseInfo });
            unityContainer.RegisterInstance(licenceService);
        }

        protected override void Initalize()
        {
            base.Initalize();
            ReadAllNotes();
        }

        protected void ReadAllNotes()
        {
            var resourcesAssembly = typeof(PlayerXRayNoteService).Assembly;

            var resourceName = "DriveHUD.PlayerXRay.Resources.DefaultNotes.xml";

            using (var stream = resourcesAssembly.GetManifestResourceStream(resourceName))
            {
                var xmlSerializer = new XmlSerializer(typeof(NotesAppSettings));
                PredefinedNotesAppSettings = xmlSerializer.Deserialize(stream) as NotesAppSettings;
            }
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

        [Test]
        [TestCase("HeroCheckRaisesLight-1.xml", EnumPokerSites.IPoker, "UbuntuFoo", "Check raises light", false)]
        [TestCase("HeroCheckRaisesLight-1.xml", EnumPokerSites.IPoker, "UbuntuFoo", "Check raises light", false)]
        [TestCase("BetsWeakWithWeakHand.xml", EnumPokerSites.IPoker, "takisaris", "Bets weak w/ weak hands", false)]
        [TestCase("BetsWeakWithWeakHand.xml", EnumPokerSites.IPoker, "Hero", "Bets weak w/ weak hands", false)]
        [TestCase("BetsWeakWithWeakHand.xml", EnumPokerSites.IPoker, "mariusban365", "Bets weak w/ weak hands", false)]
        [TestCase("BetsWeakWithWeakHand.xml", EnumPokerSites.IPoker, "WhiteRiderT", "Bets weak w/ weak hands", false)]
        [TestCase("BetsWeakWithWeakHand.xml", EnumPokerSites.IPoker, "Lex44", "Bets weak w/ weak hands", false)]
        [TestCase("BetsWeakWithWeakHand-2.xml", EnumPokerSites.IPoker, "P6_890925RK", "Bets weak w/ weak hands", false)]
        [TestCase("CallsFlopCheckRaiseWAir.xml", EnumPokerSites.IPoker, "P4_254046CL", "Bets weak w/ weak hands", false)]
        [TestCase("CallsFlopCheckRaiseWAir.xml", EnumPokerSites.IPoker, "P4_254046CL", "Calls flop check raise w/ air", false)]
        [TestCase("CallsFlopCheckRaiseWAir-2.xml", EnumPokerSites.IPoker, "Hero", "Calls flop check raise w/ air", false)]
        [TestCase("CallsFlopCheckRaiseWAir-3.xml", EnumPokerSites.IPoker, "P2_773369KJ", "Calls flop check raise w/ air", false)]
        [TestCase("OverBetsStrong.xml", EnumPokerSites.IPoker, "Hero", "Over bets strong", false)]
        [TestCase("OverBetsStrong.xml", EnumPokerSites.IPoker, "Hero", "Bet 70%+ of pot with weak made hand", false)]
        [TestCase("DoubleBarrelsOOPWAir.xml", EnumPokerSites.IPoker, "P5_736203JN", "Double barrels OOP w/ air", false)]
        [TestCase("DonkBetsFlopStrong.xml", EnumPokerSites.IPoker, "P6_909159DM", "Donk bets flop strong", false)]        
        public void TestPredefinedNotes(string handHistoryFile, EnumPokerSites pokerSite, string playerName, string noteName, bool expected)
        {
            var handHistoryObject = CreateHandHistoryObject(handHistoryFile, pokerSite, playerName);

            var noteProcessingService = new NoteProcessingService();

            var note = ReadNote(noteName);

            Assert.IsNotNull(note, $"Note [{noteName}] must be in predefined data set.");

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

        private NoteObject ReadNote(string noteName)
        {
            var note = PredefinedNotesAppSettings
                .AllNotes
                .SingleOrDefault(x => x.Name == noteName);

            return note;
        }

        private class HandHistoryObject
        {
            public HandHistory HandHistory { get; set; }

            public Playerstatistic Stat { get; set; }
        }

    }
}