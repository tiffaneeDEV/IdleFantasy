﻿using UnityEngine;
using System.Collections.Generic;

namespace IdleFantasy {
    public class MissionTestHelper : MonoBehaviour {
        public GameObject MainCanvas;
        public GameObject MissionViewPrefab;

        private Mission mTestMission;

        void Start() {
            CreateTestMission();
        }

        private void CreateTestMission() {
            MissionData testMissionData = new MissionData();
            testMissionData.DescriptionKey = "TEST_MISSION_DESC";

            MissionTaskData taskA = new MissionTaskData();
            taskA.DescriptionKey = "Task 1";
            taskA.PowerRequirement = 1000;
            taskA.StatRequirement = "TEST_STAT_1";

            MissionTaskData taskB = new MissionTaskData();
            taskB.DescriptionKey = "Task 2";
            taskB.PowerRequirement = 50;
            taskB.StatRequirement = "TEST_STAT_2";

            testMissionData.Tasks = new List<MissionTaskData>() { taskA, taskB };

            mTestMission = new Mission( testMissionData );
        }

        void Update() {
            if ( Input.GetKeyDown( KeyCode.M ) ) {
                CreateMissionUI();
            }
        }

        private void CreateMissionUI() {
            GameObject missionUI = gameObject.InstantiateUI( MissionViewPrefab, MainCanvas );
            MissionView view = missionUI.GetComponent<MissionView>();
            view.Init( mTestMission );
        }
    }
}