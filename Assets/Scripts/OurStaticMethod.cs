using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class OurStaticMethod: SoundModelMethod{
        public override void computeModel(PrimarySoundSource source){
            Profiler.diffractionRayTracker = 0;
            Profiler.diffractionSourceTracker = 0;

            Timer diffractionTimer = new Timer();
            diffractionTimer.start();
            
            if(source.soundModel.properties.modelProperties.diffraction.value)
                source.computeDiffractionSources();
            
            diffractionTimer.stop();

            Profiler.reflectionRayTracker = 0;
            Profiler.reflectionSourceTracker = 0;

            Timer reflectionTimer = new Timer();
            reflectionTimer.start();

            if(source.soundModel.properties.modelProperties.reflection.value)
                source.computeReflection();

            reflectionTimer.stop();
            Profiler.reflectionSourceTracker = source.reflectionSources.Count;

            Profiler.transmissionRayTracker = 0;
            Profiler.transmissionSourceTracker = 0;

            Timer transmissionTimer = new Timer();
            transmissionTimer.start();

            if(source.soundModel.properties.modelProperties.transmission.value)
                source.computeTransmission();

            transmissionTimer.stop();
            Profiler.transmissionRayTracker = source.soundModel.properties.modelProperties.transmissionRayCount.value;
            Profiler.transmissionSourceTracker = source.transmissionSources.Count;
        }

        public override void computeSound(Vector2 listener, PrimarySoundSource source){
            Vector2 soundDirection;
            source.soundModel.audioManager.setVolume(computeAudio(listener, source, out soundDirection) * source.soundModel.properties.modelProperties.maxVolume.value);

            if(source.soundModel.properties.audioProperties.outputChannels.value == AudioManager.OutputChannels.stereo)
                source.soundModel.audioManager.setStereoPan(source.soundModel.listenerDirection, soundDirection, source.soundModel.properties.audioProperties.volumeBasedStereo.value);
            else
                source.soundModel.audioManager.setMono();
            
            Timer reflectionTimer = new Timer();
            reflectionTimer.start();

            AudioManager.computeReflectionAudio(listener, source);

            reflectionTimer.stop();
            source.soundModel.averageReflectionSoundComputationTime.addNewValue(reflectionTimer.getTimeElapsed());
        }

        public override float computeAudio(Vector2 listener, PrimarySoundSource source, out Vector2 soundDirection){
            List<AudioManager.DirectedSound> diffractionSounds = new List<AudioManager.DirectedSound>();
            List<AudioManager.DirectedSound> transmissionSounds = new List<AudioManager.DirectedSound>();

            Timer diffractionTimer = new Timer();
            diffractionTimer.start();
            float diffractionAmplitude = AudioManager.computeDiffractionVolume(listener, source, diffractionSounds);
            diffractionTimer.stop();
            source.soundModel.averageDiffractionSoundComputationTime.addNewValue(diffractionTimer.getTimeElapsed());
            
            Timer transmissionTimer = new Timer();
            transmissionTimer.start();
            float transmissionAmplitude = AudioManager.computeTransmissionAudio(listener, source, transmissionSounds);
            transmissionTimer.stop();
            source.soundModel.averageTransmissionSoundComputationTime.addNewValue(transmissionTimer.getTimeElapsed());

            float amplitude = Mathf.Max(diffractionAmplitude, transmissionAmplitude);

            if(source.soundModel.properties.audioProperties.outputChannels.value == AudioManager.OutputChannels.stereo){
                if(source.soundModel.properties.debugProperties.visualizeAllSoundDirections.value)
                    foreach(AudioManager.DirectedSound dir in diffractionSounds)
                        source.soundModel.manager.debugger.drawLine(listener, listener + dir.direction * dir.amplitude, 0.1f, 0.2f, ResourceManager.materialColorYellow);

                soundDirection = computeSoundDirection(diffractionSounds, transmissionSounds);

                if(source.soundModel.properties.debugProperties.visualizeWeightedSoundDirection.value)
                    source.soundModel.manager.debugger.drawLine(listener, listener + soundDirection * 3, 0.1f, 0.2f, ResourceManager.materialColorMagenta);
            }else
                soundDirection = new Vector2(0, 0);

            return amplitude;
        }

        public override void visualizeModel(PrimarySoundSource source){
            source.visualizePrimarySource();
            source.visualizeDiffraction();
            source.visualizeTransmission();
        }
    }
}
