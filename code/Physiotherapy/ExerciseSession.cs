using System;

namespace Kinect.Toolbox
{
    public class ExerciseSession
    {
        string sessionName;
        string username; // Individual name in the ontology
        DateTime startDateTime;
        float duration;
        
        public ExerciseSession()
        {

        }
        public ExerciseSession(string sessionName, string username, DateTime startDateTime, float duration)
        {
            this.sessionName = sessionName;
            this.username = username;
            this.startDateTime = startDateTime;
            this.duration = duration;
        }

        public string SessionName
        {
            get { return sessionName; }
            set { sessionName = value; }
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        
        public DateTime StartDateTime
        {
            get { return startDateTime; }
            set { startDateTime = value; }
        }

        public float Duration
        {
            get { return duration; }
            set { duration = value; }
        }


    }
}
