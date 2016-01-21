using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Windows;
using System.Linq;
//using Kinect.Toolbox.Gestures.Learning_Machine;


namespace Kinect.Toolbox
{
    [Serializable]
    public class AngleGestureTest  // Analogous class to RecordedPath but for angle-based gestures
    {
        List<float> keyAngles; //key angles to be detected in whole gesture
        readonly int samplesCount;
        [NonSerialized]

        float startAngle, endAngle, middleAngle, angleThreshold; //, minStartAngle, maxStartAngle, minEndAngle, maxEndAngle, minMiddleAngle, maxMiddleAngle;
        float minDuration, maxDuration;


        public List<float> Angles
        {
            get { return keyAngles; }
            set { keyAngles = value; }
        }


        public AngleGestureTest(int samplesCount)
        {
            this.samplesCount = samplesCount;
            keyAngles = new List<float>();
        }

        public bool Match2Slower(List<float> angles, float threshold, float minimalScore, float minSize)
        {
            System.Console.WriteLine("******RUNNING ANGLEGESTURE.MATCH!!!******* ");
            if (angles.Count < samplesCount)
            {
                System.Console.WriteLine("**The angle set entry has not enough samples to be compared with template model. EXITING *** ");
                return false;
            }
            if (!(angles.Count > minSize))
            {
                System.Console.WriteLine("************The angle set has too many samples. EXITING *******");
                return false;
            }
            List<int> peakIndexes = findPeakPositions(this.keyAngles);
            System.Console.WriteLine(peakIndexes.Count + "************Pattern Peaks (max and mins) found: *******: ");
            for (int i = 0; i < peakIndexes.Count; ++i) System.Console.WriteLine(peakIndexes[i] + " position for angle " + this.keyAngles[peakIndexes[i]]);
            bool continueSearch = true;
            bool peakFound;
            int lastPeakFoundIndex = -1;
            int index;

            // TODO : ADD TIME CONTROL: while ((peak[i].time - startTime)< maxDuration)
            for (int p = 0; p < peakIndexes.Count && continueSearch == true; p++)
            {
                peakFound = false;
                index = lastPeakFoundIndex + 1;
                while (index < angles.Count && !peakFound)
                {
                    while (!peakFound)
                    {
                        //System.Console.WriteLine("Trying to find if angle: "+ angles[index]+ " is a peak in ["+ (this.keyAngles[peakIndexes[p]] - threshold) + "-" + (this.keyAngles[peakIndexes[p]] + threshold) +" ]");
                        if ((angles[index] >= (this.keyAngles[peakIndexes[p]] - threshold)) && (angles[index] <= (this.keyAngles[peakIndexes[p]] + threshold)))
                        {
                            System.Console.WriteLine("************ Peak found for position - degree: *******: " + index + " - " + angles[index]);
                            peakFound = true;
                            lastPeakFoundIndex = index;
                        }
                        //else{ 
                        //System.Console.WriteLine("************ Peak NOT found*******: ");                            
                        //}
                        index++;
                    }
                }
                if (!peakFound) continueSearch = false;
            }
            System.Console.WriteLine("AngleGesture.Match returned----------------------------------" + continueSearch);
            if (continueSearch) return true;
            else return false;
            //List<float> locals = GoldenSection.Pack(angles, samplesCount);   //??
            //float score = GoldenSection.Search(locals, angles, -MathHelper.PiOver4, MathHelper.PiOver4, threshold);
            //return score > minimalScore;
        }

        public bool Match(List<float> angles, float threshold, float minimalScore, float minSize)
        {
            System.Console.WriteLine("******RUNNING ANGLEGESTURE.MATCH!!!******* ");
            if (angles.Count < samplesCount)
            {
                System.Console.WriteLine("**The angle set entry has not enough samples to be compared with template model. EXITING *** ");
                return false;
            }
            List<int> modelPeakIndexes = findPeakPositions(this.keyAngles);
            List<int> samplePeakIndexes = findPeakPositions(angles);
            System.Console.WriteLine(modelPeakIndexes.Count + " ---PATTERN Model Peaks (max and mins) found out of " + this.keyAngles.Count + " values");
            for (int i = 0; i < modelPeakIndexes.Count; ++i) System.Console.WriteLine(modelPeakIndexes[i] + " position for angle " + this.keyAngles[modelPeakIndexes[i]]);
            System.Console.WriteLine(samplePeakIndexes.Count + " ---Sample Peaks (max and mins) found out of " + angles.Count + " values");
            for (int i = 0; i < samplePeakIndexes.Count; ++i) System.Console.WriteLine(samplePeakIndexes[i] + " position for angle " + angles[samplePeakIndexes[i]]);
            bool continueSearch = true;
            bool peakFound;
            int lastPeakFoundIndex = -1;
            int index;

            // TODO : ADD TIME CONTROL: while ((peak[i].time - startTime)< maxDuration)
            for (int p = 0; p < modelPeakIndexes.Count && continueSearch == true; p++)
            {
                peakFound = false;
                index = lastPeakFoundIndex + 1;
                while (index < samplePeakIndexes.Count && !peakFound)
                {
                    //while (!peakFound){
                    //System.Console.WriteLine("Trying to find if angle: "+ angles[index]+ " is a peak in ["+ (this.keyAngles[modelPeakIndexes[p]] - threshold) + "-" + (this.keyAngles[modelPeakIndexes[p]] + threshold) +" ]");
                    if ((angles[samplePeakIndexes[index]] >= (this.keyAngles[modelPeakIndexes[p]] - threshold)) && (angles[samplePeakIndexes[index]] <= (this.keyAngles[modelPeakIndexes[p]] + threshold)))
                    {
                        System.Console.WriteLine("************ Peak found for position - degree: *******: " + samplePeakIndexes[index] + " - " + angles[samplePeakIndexes[index]]);
                        peakFound = true;
                        lastPeakFoundIndex = index;
                    }
                    //else{ 
                    //System.Console.WriteLine("************ Peak NOT found*******: ");                            
                    //}
                    index++;
                    //}
                }
                if (!peakFound) continueSearch = false;
            }
            System.Console.WriteLine("AngleGesture.Match returned----------------------------------" + continueSearch);
            if (continueSearch) return true;
            else return false;
            //List<float> locals = GoldenSection.Pack(angles, samplesCount);   //??
            //float score = GoldenSection.Search(locals, angles, -MathHelper.PiOver4, MathHelper.PiOver4, threshold);
            //return score > minimalScore;
        }


        //Added
        public List<int> findPeakPositions(List<float> angles)
        {
            //Calculate derivatives
            List<int> time = new List<int>();
            time.Add(0);
            //List<float> maxAndMins;
            for (int i = 1; i < angles.Count; ++i)
            {
                if (i < angles.Count - 1)
                {
                    if ((angles[i] < angles[i - 1] && angles[i] < angles[i + 1]) || (angles[i] > angles[i - 1] && angles[i] > angles[i + 1]))
                    {
                        time.Add(i);
                    }
                }
                else
                {
                    time.Add(i);
                }
            }
            return time;
        }


        public float maximumAngle()
        {
            return keyAngles.Max();
        }

        public float minimumAngle()
        {
            return keyAngles.Min();
        }

        static void Main(string[] args)
        {
            int windowSize = 60;
            AngleGestureTest a = new AngleGestureTest(windowSize);
            a.Angles = new List<float> { 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11 };
            List<float> anglesEntry = new List<float> { 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11 };
            //a.Angles = new List<float> { 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11};//, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11 };
            //List<float> anglesEntry = new List<float> { 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11};//, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11, 22, 33, 44, 55, 66, 77, 78, 87, 66, 55, 44, 33, 22, 11 };
            float Epsilon = 0.035f;
            float MinimalScore = 0.80f;
            float MinimalSize = 0.1f;

            Console.WriteLine("TESTING ANGLE GESTURE");
            a.Match(anglesEntry, Epsilon, MinimalScore, MinimalSize);
            // Console.WriteLine("Find Peaks returns", a.findPeakPositions(anglesEntry));
        }

    }
}