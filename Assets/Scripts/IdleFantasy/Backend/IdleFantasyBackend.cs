﻿using MyLibrary;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IdleFantasy {
    public class IdleFantasyBackend : PlayFabBackend, IIdleFantasyBackend {
        public IdleFantasyBackend() : base() {}

        #region Game cloud calls
        // these cloud calls must be made ONE AT A TIME to avoid the server processing them too closely together.
        // if this happens, the server may increment the wrong values at the wrong times. i.e. two upgrade calls to
        // close together will both see the current level as 1, and change it to 2, when the first all should change it
        // from 1->2 and the second from 2->3
        public void MakeUpgradeCall( string i_className, string i_targetID, string i_upgradeID ) {
            Dictionary<string, string> upgradeParams = new Dictionary<string, string>();
            upgradeParams.Add( BackendConstants.CLASS, i_className );
            upgradeParams.Add( BackendConstants.TARGET_ID, i_targetID );
            upgradeParams.Add( BackendConstants.UPGRADE_ID, i_upgradeID );

            QueueCloudCall( BackendConstants.INIT_UPGRADE, upgradeParams, null );
        }

        public void CompleteMission(string i_missionType, int i_missionIndex, Dictionary<int, MissionTaskProposal> i_taskProposals ) {            
            Dictionary<string, string> cloudParams = new Dictionary<string, string>();
            cloudParams.Add( BackendConstants.MISSION_TYPE, i_missionType );
            cloudParams.Add( BackendConstants.MISSION_INDEX, i_missionIndex.ToString() );
            cloudParams.Add( BackendConstants.MISSION_PROPOSALS, JsonConvert.SerializeObject( i_taskProposals ) );

            QueueCloudCall( BackendConstants.COMPLETE_MISSION, cloudParams, null );
        }  

        public void MakeAddPointsToUpgradeCall( string i_className, string i_targetID, string i_upgradeID, int i_points ) {
            Dictionary<string, string> upgradeParams = new Dictionary<string, string>();
            upgradeParams.Add( BackendConstants.CLASS, i_className );
            upgradeParams.Add( BackendConstants.TARGET_ID, i_targetID );
            upgradeParams.Add( BackendConstants.UPGRADE_ID, i_upgradeID );
            upgradeParams.Add( BackendConstants.POINTS, i_points.ToString() );

            QueueCloudCall( BackendConstants.ADD_POINTS_TO_UPGRADE, upgradeParams, null );
        }

        public void MakeAddProgressToUpgradeCall( string i_className, string i_targetID, string i_upgradeID, float i_progress ) {
            Dictionary<string, string> upgradeParams = new Dictionary<string, string>();
            upgradeParams.Add( BackendConstants.CLASS, i_className );
            upgradeParams.Add( BackendConstants.TARGET_ID, i_targetID );
            upgradeParams.Add( BackendConstants.UPGRADE_ID, i_upgradeID );
            upgradeParams.Add( BackendConstants.PROGRESS, i_progress.ToString() );

            QueueCloudCall( BackendConstants.ADD_PROGRESS_TO_UPGRADE, upgradeParams, null );
        }

        public void MakeTrainerPurchase() {
            QueueCloudCall( BackendConstants.INIT_TRAINER_PURCHASE, null, null );
        }

        public void ChangeAssignedTrainers( string i_unitID, int i_change ) {
            Dictionary<string, string> assignParams = new Dictionary<string, string>();
            assignParams.Add( BackendConstants.CHANGE, i_change.ToString() );
            assignParams.Add( BackendConstants.TARGET_ID, i_unitID );

            QueueCloudCall( BackendConstants.INIT_TRAINING_CHANGE, assignParams, null );
        }
        #endregion

        #region Wait-for-game calls
        // these are calls that are important and rely on all previous game calls to be complete.
        // this is because the game calls are actually processed on the client after sending them so the
        // player doesn't have to wait. But these calls require the state of the server to be up-to-date before
        // sending.
        public void SendTravelRequest( string i_world, int i_optionIndex ) {
            Dictionary<string, string> cloudParams = new Dictionary<string, string>();
            cloudParams.Add( BackendConstants.MAP_WORLD, i_world );
            cloudParams.Add( BackendConstants.INDEX, i_optionIndex.ToString() );

            QueueCloudCall( BackendConstants.TRAVEL_TO, cloudParams, ( results ) => {
                if ( !IsClientOutOfSync() ) {
                    PlayerManager.Data.PlayerTraveledToNewArea( results );
                }
            } );
        }
        #endregion
    }
}