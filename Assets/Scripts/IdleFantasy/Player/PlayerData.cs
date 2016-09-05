﻿using System;
using System.Collections;
using System.Collections.Generic;
using MyLibrary;
using Newtonsoft.Json;

namespace IdleFantasy {
    public class PlayerData : IPlayerData, IResourceInventory {
        public const string PROGRESS_KEY = "Progress";
        public const string TRAINER_SAVE_DATA = "TrainerSaveData";
        
        public Dictionary<string, int> UnitTrainingLevels;

        private Hashtable mPlayerProgress = new Hashtable();
        public Dictionary<string, UnitProgress> UnitProgress { get { return ( Dictionary<string, UnitProgress>)mPlayerProgress[GenericDataLoader.UNITS]; } }
        public Dictionary<string, BuildingProgress> BuildingProgress { get { return ( Dictionary<string, BuildingProgress>)mPlayerProgress[GenericDataLoader.BUILDINGS]; } }
        public Dictionary<string, GuildProgress> GuildProgress { get { return (Dictionary<string, GuildProgress>) mPlayerProgress[GenericDataLoader.GUILDS]; } }

        private List<Guild> mGuilds = new List<Guild>();
        public List<Guild> Guilds { get { return mGuilds; } }

        private List<Building> mBuildings = new List<Building>();
        public List<Building> Buildings { get { return mBuildings; } }

        public Dictionary<string, MapData> mMaps = new Dictionary<string, MapData>();    
        public Dictionary<string, MapData> Maps { get { return mMaps; } }

        public Dictionary<string, WorldMissionProgress> mMissionProgress = new Dictionary<string, WorldMissionProgress>();
        public Dictionary<string, WorldMissionProgress> MissionProgress { get { return mMissionProgress; } }

        private TrainerSaveData mTrainerSaveData;
        private ITrainerManager mTrainerManager;
        public ITrainerManager TrainerManager { get { return mTrainerManager; } }

        private UnitUnlockPlanData mUnitUnlockPlan;
        public UnitUnlockPlanData UnitUnlockPlan { get { return mUnitUnlockPlan; } }

        private GameMetrics mGameMetrics;
        public GameMetrics GameMetrics { get { return mGameMetrics; } }

        private Dictionary<string, int> mInventory = new Dictionary<string, int>();

        private ViewModel mModel;

        private IBasicBackend mBackend;
        
        public void Init( IBasicBackend i_backend ) {
            SubscribeToMessages();

            mBackend = i_backend;
            mModel = new ViewModel();

            DownloadAllProgressData();
            DownloadTrainerData();
            DownloadCurrencyData();
            DownloadMapData();
            DownloadMissionProgress();
            DownloadUnlockPlan();
            DownloadGameMetrics();
        }

        private void SubscribeToMessages() {
            MyMessenger.AddListener<string, int>( MissionKeys.MISSION_COMPLETED, OnMissionCompleted );
        }

        public void Dispose() {
            UnsubscribeFromMessages();
        }

        private void UnsubscribeFromMessages() {
            MyMessenger.RemoveListener<string, int>( MissionKeys.MISSION_COMPLETED, OnMissionCompleted );
        }

        private void OnMissionCompleted( string i_missionWorld, int i_missionIndex ) {
            IncrementMetric( GameMetricsList.TOTAL_MISSIONS_DONE  );
            UpdateMissionProgress( i_missionWorld, i_missionIndex );

            CheckForUnitUnlock();
        }

        private void IncrementMetric( string i_metric ) {
            GameMetrics.IncrementMetric( i_metric );
        }

        private void UpdateMissionProgress( string i_missionWorld, int i_missionIndex ) {            
            WorldMissionProgress missionProgress = MissionProgress[i_missionWorld];
            missionProgress.Missions[i_missionIndex].Completed = true;
        }

        private void CheckForUnitUnlock() {
            int totalMissionsCompleted = GameMetrics.GetMetric( GameMetricsList.TOTAL_MISSIONS_DONE );
            if ( UnitUnlockPlan.UnitsUnlockAtLevel.ContainsKey( totalMissionsCompleted ) ) {                
                UnlockUnit( UnitUnlockPlan.UnitsUnlockAtLevel[totalMissionsCompleted] );
            }
        }

        private void UnlockUnit( string i_unitID ) {
            ShowUnlockPopup( i_unitID );
            UpdateUnitDataForUnlock( i_unitID );
            SendUnitUnlockEvent( i_unitID );            
        }

        private void ShowUnlockPopup( string i_unitID ) {
            string unlockText = StringTableManager.Get( StringKeys.UNLOCK_TEXT );
            unlockText = DrsStringUtils.Replace( unlockText, StringKeys.CLASS_KEY, UnitKeys.GetName( i_unitID ) );

            ViewModel model = new ViewModel();
            model.SetProperty( InfoPopupProperties.MAIN_IMAGE, UnitKeys.GetIconKey( i_unitID ) );
            model.SetProperty( InfoPopupProperties.MAIN_TEXT, unlockText );

            MyMessenger.Send<string, ViewModel>( InfoPopupEvents.QUEUE, InfoPopupProperties.STANDARD_POPUP, model );
        }

        private void UpdateUnitDataForUnlock( string i_unitID ) {
            foreach ( Building building in Buildings ) {
                if ( building.Unit.GetID() == i_unitID ) {
                    building.Level.Upgrade();
                    building.Unit.Level.Upgrade();
                }
            }
        }

        private void SendUnitUnlockEvent( string i_unitID ) {
            MyMessenger.Send<string>( UnitKeys.UNLOCK_EVENT, i_unitID );
        }

        private void DownloadUnlockPlan() {
            mBackend.GetTitleData( BackendConstants.UNIT_UNLOCKS, ( jsonData ) => {
                mUnitUnlockPlan = JsonConvert.DeserializeObject<UnitUnlockPlanData>( jsonData );
            } );
        }

        private void DownloadGameMetrics() {
            mBackend.GetPlayerData( BackendConstants.GAME_METRICS, ( jsonData ) => {
                mGameMetrics = JsonConvert.DeserializeObject<GameMetrics>( jsonData );
            } );
        }

        private void DownloadMapData() {
            mBackend.GetPlayerData( BackendConstants.MAP_BASE, ( jsonData ) => {
                mMaps[BackendConstants.WORLD_BASE] = JsonConvert.DeserializeObject<MapData>( jsonData );
            });
        }

        private void DownloadMissionProgress() {
            mBackend.GetPlayerDataDeserialized<Dictionary<string, WorldMissionProgress>>( BackendConstants.MISSION_PROGRESS, ( progress ) => {
                mMissionProgress = progress;
            } );
        }

        private void DownloadTrainerData() {
            mBackend.GetPlayerData( TRAINER_SAVE_DATA, ( jsonData ) => {
                mTrainerSaveData = JsonConvert.DeserializeObject<TrainerSaveData>( jsonData );
            } );
        }

        private void DownloadCurrencyData() {
            mBackend.GetVirtualCurrency( VirtualCurrencies.GOLD, ( numGold ) => {
                Gold = numGold;
            } );
        }
        private void DownloadAllProgressData() {            
            DownloadProgressDataForKey<UnitProgress>( GenericDataLoader.UNITS );
            DownloadProgressDataForKey<BuildingProgress>( GenericDataLoader.BUILDINGS );
            DownloadProgressDataForKey<GuildProgress>( GenericDataLoader.GUILDS, AddGuilds );
        }

        public void AddDataStructures() {
            AddBuildings();
        }

        private void AddGuilds() {
            foreach ( KeyValuePair<string, GuildProgress> kvp in GuildProgress ) {
                Guilds.Add( new Guild( kvp.Value ) );
            }
        }

        private void AddBuildings() {
            foreach ( KeyValuePair<string, BuildingProgress> kvp in BuildingProgress ) {
                BuildingData buildingData = GenericDataLoader.GetData<BuildingData>( kvp.Value.ID );
                UnitProgress unitProgressForBuilding = UnitProgress[buildingData.Unit];
                Buildings.Add( new Building( kvp.Value, unitProgressForBuilding ) );
            }
        }

        private void DownloadProgressDataForKey<T>( string i_key, Callback i_doneDownloadingCallback = null ) where T : ProgressBase {
            string dataKey = i_key + PROGRESS_KEY;
            mBackend.GetPlayerData( dataKey, ( jsonData ) => {
                Dictionary<string, T> allProgressData = JsonConvert.DeserializeObject<Dictionary<string, T>>( jsonData );

                SetIDsOnProgressData( allProgressData );

                mPlayerProgress[i_key] = allProgressData;

                if ( i_doneDownloadingCallback != null ) {
                    i_doneDownloadingCallback();
                }
            } );
        }

        // This method exists to set IDs on all progress data, because the ID isn't inside the JSON itself
        private void SetIDsOnProgressData<T>( Dictionary<string, T> i_progressData ) where T : ProgressBase {
            foreach ( KeyValuePair<string, T> kvp in i_progressData ) {
                kvp.Value.ID = kvp.Key;
            }
        }

        public void CreateManagers() {
            mTrainerManager = new TrainerManager( mModel, mTrainerSaveData, UnitProgress );
        }

        public int Gold {
            get { return mModel.GetPropertyValue<int>( VirtualCurrencies.GOLD ); }
            set {
                mModel.SetProperty( VirtualCurrencies.GOLD, value );

                if ( !mInventory.ContainsKey( VirtualCurrencies.GOLD ) ) {
                    mInventory.Add( VirtualCurrencies.GOLD, value );
                }
                else {
                    mInventory[VirtualCurrencies.GOLD] = value;
                }
            }
        }

        public ViewModel GetViewModel() {
            return mModel;
        }

        public int GetResourceCount( string i_resource ) {
            if ( mInventory.ContainsKey( i_resource ) ) {
                return mInventory[i_resource];
            }
            else {
                mInventory[i_resource] = 0;
                return 0;
            }
        }

        public bool HasEnoughResources( string i_resource, int i_count ) {
            int amountOfResource = GetResourceCount( i_resource );
            bool hasEnough = amountOfResource >= i_count;
            return hasEnough;
        }

        public void SpendResources( string i_resource, int i_count ) {
            int amountOfResource = GetResourceCount( i_resource );
            int remainingValue = Math.Max( amountOfResource - i_count, 0 );
            mInventory[i_resource] = remainingValue;

            UpdateInventoryData();
        }

        public void UpdateInventoryData() {
            foreach ( KeyValuePair<string, int> inventoryItem in mInventory ) {
                mModel.SetProperty( inventoryItem.Key, inventoryItem.Value );
            }
        }
    }
}