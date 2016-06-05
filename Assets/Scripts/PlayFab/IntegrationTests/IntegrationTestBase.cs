﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IdleFantasy.PlayFab.IntegrationTests {
    public abstract class IntegrationTestBase : MonoBehaviour {
        protected const string TARGET_ID = "TargetID";
        protected const string CLASS = "Class";

        protected const string SAVE_KEY = "SaveKey";    // used in params sent to cloud for test methods

        protected abstract IEnumerator RunAllTests();

        protected IdleFantasyBackend mBackend;

        void Start() {
            mBackend = BackendManager.Backend;
            StartCoroutine( StartTests() );
        }

        private IEnumerator StartTests() {
            yield return mBackend.WaitUntilNotBusy();

            yield return RunAllTests();

            DoneWithTests();
        }

        protected void SetPlayerData( string i_key, string i_value ) {
            Dictionary<string, string> setDataParams = new Dictionary<string, string>();
            setDataParams["Key"] = i_key;
            setDataParams["Value"] = i_value;
            mBackend.MakeCloudCall( IdleFantasyBackend.TEST_SET_DATA, setDataParams, null );
        }

        protected void SetInternalData( string i_key, string i_value ) {
            Dictionary<string, string> setDataParams = new Dictionary<string, string>();
            setDataParams[SAVE_KEY] = i_key;
            setDataParams["Value"] = i_value;
            mBackend.MakeCloudCall( IdleFantasyBackend.TEST_SET_INTERNAL_DATA, setDataParams, null );
        }

        protected void SetPlayerCurrency( int i_amount ) {
            Dictionary<string, string> setCurrencyParams = new Dictionary<string, string>();
            setCurrencyParams["Type"] = VirtualCurrencies.GOLD;
            setCurrencyParams["Amount"] = i_amount.ToString();
            mBackend.MakeCloudCall( IdleFantasyBackend.TEST_SET_CURRENCY, setCurrencyParams, null );
        }

        protected void DoneWithTests() {
            IntegrationTest.Pass();
        }

        protected void FailTestIfClientInSync( string i_testName ) {
            if ( !mBackend.ClientOutOfSync ) {
                IntegrationTest.Fail( i_testName + ": Client should be out of sync, but it's not." );
            }
        }

        protected void FailTestIfCurrencyDoesNotEqual( int i_amount ) {
            mBackend.GetVirtualCurrency( VirtualCurrencies.GOLD, ( numGold ) => {
                if ( numGold != i_amount ) {
                    IntegrationTest.Fail( "Currency did not equal " + i_amount );
                }
            } );
        }

        protected void GetProgressData<T>( string i_class, string i_targetID, Callback<T> i_resultsCallback ) {
            Dictionary<string, string> getParams = new Dictionary<string, string>();
            getParams.Add( "Class", i_class );
            getParams.Add( "TargetID", i_targetID );

            mBackend.MakeCloudCall( "getProgressData", getParams, ( results ) => {
                if ( results.ContainsKey( "data" ) ) {
                    T progress = JsonConvert.DeserializeObject<T>( results["data"] );
                    i_resultsCallback( progress );
                }
                else {
                    IntegrationTest.Fail( "Results did not have data." );
                }
            } );
        }

        protected void FailTestIfNotProgressLevel( string i_class, string i_targetID, int i_level ) {
            Dictionary<string, string> getParams = new Dictionary<string, string>();
            getParams.Add( "Class", i_class );
            getParams.Add( "TargetID", i_targetID );

            mBackend.MakeCloudCall( "getProgressData", getParams, ( results ) => {
                if ( results.ContainsKey( "data" ) ) {
                    ProgressBase progress = JsonConvert.DeserializeObject<ProgressBase>( results["data"] );
                    if ( progress.Level != i_level ) {
                        IntegrationTest.Fail( "Level did not match: " + i_level );
                    }
                }
                else {
                    IntegrationTest.Fail( "Results did not have data." );
                }
            } );
        }

        protected void FailTestIfReturnedCallDoesNotEqual( string i_cloudMethod, double i_value, Dictionary<string,string> i_params = null ) {
            mBackend.MakeCloudCall( i_cloudMethod, i_params, ( results ) => {
                if ( results.ContainsKey( "data" ) ) {
                    double value = double.Parse( results["data"] );

                    if ( value != i_value ) {
                        IntegrationTest.Fail( "Value should have been " + i_value + " but was " + value );
                    }
                }
                else {
                    IntegrationTest.Fail( "Results did not have data." );
                }
            } );
        }

        protected void FailTestIfReturnedCallEquals( string i_cloudMethod, double i_value, Dictionary<string, string> i_params = null ) {
            mBackend.MakeCloudCall( i_cloudMethod, i_params, ( results ) => {
                if ( results.ContainsKey( "data" ) ) {
                    double value = double.Parse( results["data"] );
                    
                    if ( value == i_value ) {
                        IntegrationTest.Fail( "Value was: " + i_value );
                    }
                }
                else {
                    IntegrationTest.Fail( "Results did not have data." );
                }
            } );
        }
    }
}