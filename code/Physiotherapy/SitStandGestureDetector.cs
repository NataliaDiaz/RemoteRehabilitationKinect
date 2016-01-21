using System;
using Microsoft.Kinect;
using System.Linq;
using System.IO;

namespace Kinect.Toolbox
{
    public class SitStandGestureDetector : TemplatedGestureDetector
    {
        public float SwipeMinimalLength { get; set; }
        public float SwipeMaximalHeight { get; set; }
        public int SwipeMininalDuration { get; set; }
        public int SwipeMaximalDuration { get; set; }

        

        public SitStandGestureDetector(string gestureName, Stream kbStream, int windowSize = 20)//60)
            : base(gestureName, kbStream, windowSize) //This constructor calls the superclass with the parameters in parenthesis
        {
            SwipeMinimalLength = 1.2f; //0.4f; // Make bigger? //Distance from first point to last in the path //Check that the first and the last points are at a good distance from each other.
            SwipeMaximalHeight = 0.2f;
            SwipeMininalDuration = 250; // Maybe to be removed? //Check that the first and last points were created within a given period of time.
            SwipeMaximalDuration = 1500; // Maybe to be removed?
            System.Console.WriteLine("Sit/Stand STARTED --------------------SwipeMinimalLength:" + SwipeMinimalLength+ " WindowSize:"+windowSize);
        }
    
    //public SitStandGestureDetector()//int windowSize = 20)
        ////    : base(windowSize)
        //{
        //    SwipeMinimalLength = 0.8f; //0.4f; // Make bigger? //Distance from first point to last in the path //Check that the first and the last points are at a good distance from each other.
        //    SwipeMaximalHeight = 0.2f;
        //    SwipeMininalDuration = 250; // Maybe to be removed? //Check that the first and last points were created within a given period of time.
        //    SwipeMaximalDuration = 1500; // Maybe to be removed?
        //}

        //public override SitStandGestureDetector(string gestureName, Stream kbStream, int windowSize = 60)
        //    : base(windowSize)
        //{
        //    Epsilon = 0.035f;
        //    MinimalScore = 0.80f;
        //    MinimalSize = 0.1f;
        //    this.gestureName = gestureName;
        //    learningMachine = new LearningMachine(kbStream);

        //    SwipeMinimalLength = 0.8f; //0.4f; // Make bigger? //Distance from first point to last in the path //Check that the first and the last points are at a good distance from each other.
        //    SwipeMaximalHeight = 0.2f;
        //    SwipeMininalDuration = 250; // Maybe to be removed? //Check that the first and last points were created within a given period of time.
        //    SwipeMaximalDuration = 1500; // Maybe to be removed?

        //}

        protected bool ScanPositions(Func<Vector3, Vector3, bool> directionFunction,
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    //double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    //if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    //{
                        return true;
                    //}
                }
            }

            return false;
        }

        protected override void LookForGesture()
        {
            // Swipe to right
            //if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
            //    (p1, p2) => p2.X - p1.X > -0.01f, // Progression to right
            //    (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
            //    SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            //{
            //    RaiseGestureDetected("SwipeToRight");
            //    return;
            //}

            //// Swipe to left
            //if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
            //    (p1, p2) => p2.X - p1.X < 0.01f, // Progression to right
            //    (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
            //    SwipeMininalDuration, SwipeMaximalDuration))// Duration
            //{
            //    RaiseGestureDetected("SwipeToLeft");
            //    return;
            //}

            //CONSIDERING DURATION OF GESTURE
            //if (LearningMachine.Match(Entries.Select(e => new Vector2(e.Position.X, e.Position.Y)).ToList(), Epsilon, MinimalScore, MinimalSize))
            //{
            //    System.Console.WriteLine("Sit/Stand DETECTED --------------------SwipeMinimalLength:" + SwipeMinimalLength);
            //    // Sitting
            //    if (ScanPositions(//(p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
            //        (p1, p2) => p2.Y - p1.Y > -0.01f, // Progression up
            //        (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
            //        SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            //    {
            //        RaiseGestureDetected("Sit");
            //        return;
            //    }

            //    // Standing
            //    if (ScanPositions(//(p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
            //        (p1, p2) => p2.Y - p1.Y < 0.01f, // Progression down
            //        (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
            //        SwipeMininalDuration, SwipeMaximalDuration))// Duration
            //    {
            //        RaiseGestureDetected("Stand");
            //        return;
            //    }
            //}
            if (LearningMachine.Match(Entries.Select(e => new Vector2(e.Position.X, e.Position.Y)).ToList(), Epsilon, MinimalScore, MinimalSize))
            {
                System.Console.WriteLine("Sit/Stand DETECTED --------------------SwipeMinimalLength:" + SwipeMinimalLength);
                // Sitting
                if (ScanPositions(//(p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
                    (p1, p2) => p2.Y - p1.Y > -0.01f, // Progression up
                    (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                    SwipeMininalDuration, SwipeMaximalDuration)) // Duration
                {
                    RaiseGestureDetected("Sit");
                    return;
                }

                // Standing
                if (ScanPositions(//(p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
                    (p1, p2) => p2.Y - p1.Y < 0.01f, // Progression down
                    (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                    SwipeMininalDuration, SwipeMaximalDuration))// Duration
                {
                    RaiseGestureDetected("Stand");
                    return;
                }
            }
        }
    }
}