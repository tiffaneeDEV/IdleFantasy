﻿using System.Collections;

namespace IdleFantasy.PlayFab.IntegrationTests {
    public class TestLegalTrainerAssignment : TestTrainerAssignments {
        protected override IEnumerator RunAllTests() {
            yield return TestLegalAssignment();
        }

        private IEnumerator TestLegalAssignment() {
            SetTrainerCount( 1 );
            yield return mBackend.WaitUntilNotBusy();
            SetProgressData( 1, 0 );

            yield return mBackend.WaitUntilNotBusy();

            yield return MakeAssignmentChange( 1 );

            FailTestIfReturnedCallDoesNotEqual( GET_AVAILABLE_TRAINERS_CLOUD_METHOD, 0 );
            FailTestIfAssignedTrainersDoesNotEqual( 1 );

            yield return mBackend.WaitUntilNotBusy();
        }
    }
}