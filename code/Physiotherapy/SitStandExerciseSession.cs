using System;

namespace Kinect.Toolbox
{
    public class SitStandExerciseSession: ExerciseSession
    {
        int nSits; // amount of repetitions (sit-stand)
        float avgSecondsToSit; // avg time taken to sit
        float avgSecondsToStand; // avg time taken to stand
        //To-be-added:
        float avgStandingDegree; // hipCenter-spine-shoulderCenter
        float avgSittingDegree; // hipCenter-spine-shoulderCenter
        float standingCompleteness; //standingHeight/usualHeight ratio
        
        public SitStandExerciseSession()
        {

        }

        public SitStandExerciseSession(string sessionName, string username, DateTime startDateTime, float duration, int nSits, float avgSecondsToSit, float avgSecondsToStand)
            : base(sessionName, username, startDateTime, duration)
        {
            this.nSits = nSits;
            this.avgSecondsToSit = avgSecondsToSit;
            this.avgSecondsToStand = avgSecondsToStand;
        }

        public int NSits
        {
            get { return nSits; }
            set { nSits = value; }
        }

        public float AvgSecondsToSit
        {
            get { return avgSecondsToSit; }
            set { avgSecondsToSit = value; }
        }

        public float AvgSecondsToStand
        {
            get { return avgSecondsToStand; }
            set { avgSecondsToStand = value; }
        }



    }
}
