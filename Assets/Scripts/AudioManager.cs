using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class AudioManager{
        public enum OutputChannels{
            stereo,
            mono
        };

        public struct DirectedSound{
            public Vector2 direction;
            public float amplitude;

            public DirectedSound(Vector2 direction, float amplitude){
                this.direction = direction.normalize();
                this.amplitude = amplitude;
            }
        }

        [System.Serializable]
        public class Properties: SoundPropagation.Properties{
            public PropertyAudioClip audioClip;
            public PropertyOutputChannelsEnum outputChannels;
            public PropertyBool volumeBasedStereo;
            public PropertyFloat audioStartDelay;
            public PropertyInt reflectionAudioPoolCount;
            public PropertyFloat reflectionMaximumAudioDelay;

            public override void initialize(){
                setProperties(new GenericProperty[]{
                    audioClip,
                    outputChannels,
                    volumeBasedStereo,
                    audioStartDelay,
                    reflectionAudioPoolCount,
                    reflectionMaximumAudioDelay
                });
            }
        }
        
        private AudioSource mainAudioSource;
        private AudioSource[] reflectionAudioSources;

        private SoundModel soundModel;

        private GameObject gameObject;

        public AudioManager(SoundModel soundModel, GameObject gameObject){
            this.soundModel = soundModel;
            this.gameObject = gameObject;

            mainAudioSource = gameObject.AddComponent<AudioSource>();

            updateAudioSources();
        }

        private void createReflectionPool(){
            if(reflectionAudioSources != null)
                foreach(AudioSource source in reflectionAudioSources)
                    Object.Destroy(source);

            reflectionAudioSources = new AudioSource[soundModel.properties.audioProperties.reflectionAudioPoolCount.value];
            for(int i = 0; i < reflectionAudioSources.Length; i++){
                reflectionAudioSources[i] = gameObject.AddComponent<AudioSource>();
                reflectionAudioSources[i].clip = soundModel.properties.audioProperties.audioClip.value;
            }
        }

        public void setTime(float time){
            setTime(mainAudioSource, time);

            for(int i = 0; i < reflectionAudioSources.Length; i++)
                setTime(reflectionAudioSources[i], time);
        }

        private void setTime(AudioSource source, float time){
            float newTime = time;
            
            while(newTime >= mainAudioSource.clip.length)
                newTime -= mainAudioSource.clip.length;
            
            source.time = newTime;
        }

        public void playLoop(){
            mainAudioSource.Stop();
            mainAudioSource.Play();
            mainAudioSource.time = soundModel.properties.audioProperties.audioStartDelay.value;
            mainAudioSource.loop = true;

            adjustAudioDelays();
        }

        private void adjustAudioDelays(){
            float offset = mainAudioSource.time;
            float interval = soundModel.properties.audioProperties.reflectionMaximumAudioDelay.value / soundModel.properties.audioProperties.reflectionAudioPoolCount.value;

            for(int i = 0; i < reflectionAudioSources.Length; i++){
                AudioSource source = reflectionAudioSources[i];

                source.Stop();
                source.Play();
                source.loop = true;

                float time = offset + (i + 1) * interval;
                
                while(time > mainAudioSource.clip.length)
                    time -= mainAudioSource.clip.length;
                
                source.time = time;
            }
        }

        public void updateAudioSources(){
            mainAudioSource.clip = soundModel.properties.audioProperties.audioClip.value;
            createReflectionPool();
            adjustAudioDelays();
        }

        public void setVolume(float volume){
            mainAudioSource.volume = volume;
        }

        public void setStereoPan(Vector2 listenerFront, Vector2 soundDirection, bool volumeBased){
            if(mainAudioSource.volume == 0){
                mainAudioSource.panStereo = 0;
                return;
            }

            Vector3 cross = Vector3.Cross(new Vector3(soundDirection.x, soundDirection.y, 0), new Vector3(listenerFront.x, listenerFront.y, 0));
            mainAudioSource.panStereo = cross.z / (listenerFront.getMagnitude() * soundDirection.getMagnitude());
            if(volumeBased)
                 mainAudioSource.panStereo *= (1 - mainAudioSource.volume);
        }

        public void setMono(){
            mainAudioSource.panStereo = 0;
        }

        public float getVolume(){
            return mainAudioSource.volume;
        }

        public static float computeTransmissionAudio(Vector2 point, PrimarySoundSource mainSource, List<DirectedSound> soundDirections){
            float amplitude = 0;

            Obstacle.ObstacleTreeNode depthNode = mainSource.soundModel.manager.activeGeometry.findObstacleDepth(point);

            foreach(TransmissionSource source in mainSource.transmissionSources) {
                float volume = source.computeAmplitude(point, depthNode, mainSource.soundModel);

                if(volume > 0 && mainSource.soundModel.properties.audioProperties.outputChannels.value == OutputChannels.stereo)
                    soundDirections.Add(new DirectedSound(source.position - point, volume));

                amplitude = Mathf.Max(amplitude, volume);
            }

            return amplitude / mainSource.amplitude;
        }

        public static void silentReflectionAudio(PrimarySoundSource mainSource){
            for(int i = 0; i < mainSource.soundModel.audioManager.reflectionAudioSources.Length; i++)
                mainSource.soundModel.audioManager.reflectionAudioSources[i].volume = 0;
        }

        public static void computeReflectionAudio(Vector2 point, PrimarySoundSource mainSource){
            float interval = mainSource.soundModel.properties.audioProperties.reflectionMaximumAudioDelay.value / mainSource.soundModel.properties.audioProperties.reflectionAudioPoolCount.value;
            float[] bins = new float[mainSource.soundModel.properties.audioProperties.reflectionAudioPoolCount.value];

            if(mainSource.soundModel.audioManager.reflectionAudioSources.Length != mainSource.soundModel.properties.audioProperties.reflectionAudioPoolCount.value)
                return;

            float scale = 1;

            foreach(ReflectionSource source in mainSource.reflectionSources){
                float delay;
                float amplitude = source.computeAmplitude(point, out delay, mainSource.soundModel);

                scale += amplitude / 10;

                float place = delay / interval;
                int leftBin = (int) (delay / interval);
                int rightBin = (int) (delay / interval) + 1;

                if(leftBin >= bins.Length)
                    continue;

                float left = leftBin * interval;
                float right = rightBin * interval;

                float leftDistance = delay - left;
                float rightDistance = right - delay;

                float leftAmplitude = amplitude * rightDistance / interval;
                float rightAmplitude = amplitude * leftDistance / interval;

                if(leftAmplitude > bins[leftBin])
                    bins[leftBin] = leftAmplitude;

                if(rightBin >= bins.Length)
                    continue;

                if(rightAmplitude > bins[rightBin])
                    bins[rightBin] = rightAmplitude;
            }

            for(int i = 0; i < bins.Length; i++){
                mainSource.soundModel.audioManager.reflectionAudioSources[i].volume = bins[i] / mainSource.amplitude * mainSource.soundModel.properties.modelProperties.maxVolume.value;
                mainSource.soundModel.audioManager.reflectionAudioSources[i].volume *= scale;
            }
        }

        public static void computeReflectionAudioOld(Vector2 point, PrimarySoundSource mainSource){
            float interval = mainSource.soundModel.properties.audioProperties.reflectionMaximumAudioDelay.value / mainSource.soundModel.properties.audioProperties.reflectionAudioPoolCount.value;
            float[] bins = new float[mainSource.soundModel.properties.audioProperties.reflectionAudioPoolCount.value];

            if(mainSource.soundModel.audioManager.reflectionAudioSources.Length != mainSource.soundModel.properties.audioProperties.reflectionAudioPoolCount.value)
                return;

            float scale = 1;

            foreach(ReflectionSource source in mainSource.reflectionSources){
                float delay;
                float amplitude = source.computeAmplitude(point, out delay, mainSource.soundModel);

                scale += amplitude / 10;

                int bin = (int) (delay / interval);
                if(bin >= bins.Length)
                    bin = bins.Length - 1;

                if(amplitude > bins[bin])
                    bins[bin] = amplitude;
            }

            for(int i = 0; i < bins.Length; i++){
                mainSource.soundModel.audioManager.reflectionAudioSources[i].volume = bins[i] / mainSource.amplitude * mainSource.soundModel.properties.modelProperties.maxVolume.value;
                mainSource.soundModel.audioManager.reflectionAudioSources[i].volume *= scale;
            }
        }

        public static float computeDiffractionVolume(Vector2 point, PrimarySoundSource mainSource, List<DirectedSound> soundDirections){
            List<Vector2> points = new List<Vector2>();
            points.Add(point);

            float amplitude = computeDiffractionVolume(points, mainSource, soundDirections)[0];

            return amplitude;
        }

        public static float[] computeDiffractionVolume(List<Vector2> points, PrimarySoundSource mainSource, List<DirectedSound> soundDirections){
            float[] volumes = new float[points.Count];

            List<DiffractionSource> diffractionSources = mainSource.allDiffractionSources;

            for(int i = 0; i < points.Count; i++){
                Vector2 point = points[i];
    
                if(mainSource.soundModel.manager.activeGeometry.findObstacleDepth(point) != mainSource.obstacleDepth)
                    continue;

                volumes[i] = mainSource.computeAmplitude(point, mainSource.soundModel);

                if(mainSource.soundModel.properties.audioProperties.outputChannels.value == OutputChannels.stereo)
                    soundDirections.Add(new DirectedSound(mainSource.position - point, volumes[i]));

                float diffractionAmp = diffractionAmplitude(point, diffractionSources, mainSource.soundModel, soundDirections);
                if(diffractionAmp > volumes[i])
                    volumes[i] = diffractionAmp;

                volumes[i] /= mainSource.amplitude;
            }

            return volumes;
        }

        private static float diffractionAmplitude(Vector2 point, List<DiffractionSource> sources, SoundModel soundModel, List<DirectedSound> soundDirections){
            float amp = 0;

            foreach(DiffractionSource source in sources) {
                float volume = source.computeAmplitude(point, soundModel);

                if(volume > 0)
                    if(soundModel.properties.audioProperties.outputChannels.value == OutputChannels.stereo)
                        soundDirections.Add(new DirectedSound(source.position - point, volume));

                if(amp < volume)
                    amp = volume;
            }

            return amp;
        }
    }
}
