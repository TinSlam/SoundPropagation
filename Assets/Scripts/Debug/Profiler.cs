using UnityEngine;

namespace SoundPropagation{
    public class Profiler{
        public class ProfilingUnit{
            public float average;

            private float[] history;
            private float historyCount;
            private int historySize;
            private int historyPointer;

            public ProfilingUnit(int historySize){
                average = 0;
                historyCount = 0;
                historyPointer = 0;
                this.historySize = historySize;
                history = new float[historySize];
            }

            public void addNewValue(float value){
                if(historyCount != historySize){
                    average = (average * historyCount + value) / (historyCount + 1);
                    history[historyPointer++] = value;
                    historyCount++;
                }else{
                    historyPointer %= historySize;
                    average = (average * historySize - history[historyPointer] + value) / historySize;
                    history[historyPointer++] = value;
                }
            }
        }

        public static int diffractionRayTracker;
        public static int diffractionSourceTracker;

        public static int reflectionRayTracker;
        public static int reflectionSourceTracker;

        public static int transmissionRayTracker;
        public static int transmissionSourceTracker;
    }

    public class Timer{
        private float startTime;
        private float endTime;

        public void start(){
            startTime = Time.realtimeSinceStartup;
        }

        public void stop(){
            endTime = Time.realtimeSinceStartup;
        }

        public float getTimeElapsed(){
            return endTime - startTime;
        }
    }

    public class StopWatch{
        public float timeElapsed = 0;
        private float currentStart;

        public void resume(){
            currentStart = Time.realtimeSinceStartup;
        }

        public void pause(){
            timeElapsed += Time.realtimeSinceStartup - currentStart;
        }

        public void reset(){
            timeElapsed = 0;
            currentStart = Time.realtimeSinceStartup;
        }
    }
}
