﻿using UnityEngine;
using System.Collections;

namespace IdleFantasy.PlayFab.IntegrationTests {
    public class TestCanAffordUpgrade : TestUpgrades {
        protected override IEnumerator RunTest() {
            SetPlayerData( mCurrentTestData.SaveKey, DrsStringUtils.Replace( mCurrentTestData.SaveValue, "NUM", 1 ) );
            SetPlayerCurrency( mCurrentTestData.Cost );

            yield return mBackend.WaitUntilNotBusy();

            yield return MakeUpgradeCall();

            FailTestIfCurrencyDoesNotEqual( 0 );
            FailTestIfNotProgressLevel( mCurrentTestData.TestClass, mCurrentTestData.TestID, 2 );            
        }
    }
}