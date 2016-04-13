﻿using NUnit.Framework;

#pragma warning disable 0414

namespace IdleFantasy.UnitTests {
    [TestFixture]
    public class UnitUpgradeTests {

        private Building mBuilding;
        private Unit mUnit;

        [SetUp]
        public void BeforeTest() {
            UnitData data = GenericDataLoader.GetData<UnitData>( GenericDataLoader.UNITS, GenericDataLoader.TEST_UNIT );
            mUnit = new Unit( data );

            mBuilding = BuildingUpgradeTests.GetMockBuilding();
            mBuilding.SetUnit( mUnit );
        }


        [Test]
        public void UpgradeResetsNextUnitProgress() {
            mBuilding.NextUnitProgress = .5f;

            mUnit.Level.Upgrade();

            Assert.AreEqual( 0, mBuilding.NextUnitProgress );
        }

        [Test]
        public void UpgradeResetsNumUnits() {
            mBuilding.NumUnits = 1;

            mUnit.Level.Upgrade();

            Assert.AreEqual( 0, mBuilding.NumUnits );
        }

        [Test]
        public void UpgradeSlowsDownUnitProgress() {

        }
    }
}