﻿using NUnit.Framework;
using NSubstitute;

#pragma warning disable 0414
namespace IdleFantasy.UnitTests {
    [TestFixture]
    public class TravelOptionTests {

        [SetUp]
        public void BeforeTest() {
            UnitTestUtils.LoadOfflineData();
        }

        [Test]
        public void SettingName_SetsProperty() {
            string name = "TestName";
            TravelOption optionUnderTest = GetBasicTravelOptionForTest();

            optionUnderTest.SetOptionName( name );

            Assert.AreEqual( optionUnderTest.ViewModel.GetPropertyValue<string>( TravelOption.NAME_PROPERTY ), name );
        }

        static object[] MapClearCountSource = {
            new object[] { 0, 9 },
            new object[] { 1, 18 },
            new object[] { 2, 27 },
            new object[] { 3, 36 }
        };

        [Test]
        [TestCaseSource( "MapClearCountSource" )]
        public void TestClearCountIsExpected_GivenIndex( int i_index, int i_expectedRequiredCount ) {
            TravelOption optionUnderTest = GetBasicTravelOptionForTest();

            int requiredClearCount = optionUnderTest.GetRequiredMapClearCount( i_index );

            Assert.AreEqual( i_expectedRequiredCount, requiredClearCount );
        }

        static object[] GetClearsUntilAvailable_Source = {
            new object[] { 0, 1, 8 },
            new object[] { 1, 10, 8 },
            new object[] { 2, 100, 0 },
            new object[] { 3, 20, 16 }
        };

        [Test]
        [TestCaseSource("GetClearsUntilAvailable_Source")]
        public void TestClearsUntilAvailableCountIsExpected_GivenIndexAndProgress( int i_index, int i_progress, int i_expected ) {
            TravelOption optionUnderTest = GetBasicTravelOptionForTest();
            IWorldMissionProgress missionProgress = Substitute.For<IWorldMissionProgress>();
            missionProgress.GetCompletedMissionCount().Returns( i_progress );            

            int clearsUntilAvailable = optionUnderTest.GetClearsUntilAvailable( missionProgress, i_index );

            Assert.AreEqual( i_expected, clearsUntilAvailable );
        }

        [Test]
        public void WithGoodMissionProgress_TravelOptionIsAvailable() {
            TravelOption optionUnderTest = GetBasicTravelOptionForTest();

            bool isAvailable = optionUnderTest.IsOptionAvailable( GetGoodMissionProgress(), 0 );

            Assert.IsTrue( isAvailable );
        }

        [Test]
        public void WithBadMissionProgress_TravelOptionIsNotAvailable() {
            TravelOption optionUnderTest = GetBasicTravelOptionForTest();

            bool isAvailable = optionUnderTest.IsOptionAvailable( GetBadMissionProgress(), 0 );

            Assert.IsFalse( isAvailable );
        }

        [Test]
        public void WithGoodMissionProgress_TravelOptionNameIsMapName() {
            string testName = "Test";
            IMapName mapName = Substitute.For<IMapName>();
            mapName.GetStringName().Returns( testName );
            TravelOption optionUnderTest = GetBasicTravelOptionForTest();

            string name = optionUnderTest.GetOptionName( mapName, GetGoodMissionProgress(), 0 );

            Assert.AreEqual( testName, name );
        }

        private IWorldMissionProgress GetGoodMissionProgress() {
            IWorldMissionProgress missionProgress = Substitute.For<IWorldMissionProgress>();
            missionProgress.GetCompletedMissionCount().Returns( 1000 );

            return missionProgress;
        }

        private IWorldMissionProgress GetBadMissionProgress() {
            IWorldMissionProgress missionProgress = Substitute.For<IWorldMissionProgress>();
            missionProgress.GetCompletedMissionCount().Returns( 0 );

            return missionProgress;
        }

        private TravelOption GetBasicTravelOptionForTest() {
            TravelOption optionUnderTest = new TravelOption( Substitute.For<IMapName>(), 0, Substitute.For<IWorldMissionProgress>() );
            return optionUnderTest;
        }
    }
}