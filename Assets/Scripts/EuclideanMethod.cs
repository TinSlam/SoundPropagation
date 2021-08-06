using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class EuclideanMethod: SoundModelMethod{
        public override void computeModel(PrimarySoundSource source){}

        public override void computeSound(Vector2 listener, PrimarySoundSource source){
            Vector2 soundDirection;
            source.soundModel.audioManager.setVolume(computeAudio(listener, source, out soundDirection) * source.soundModel.properties.modelProperties.maxVolume.value);
            
            if(source.soundModel.properties.audioProperties.outputChannels.value == AudioManager.OutputChannels.stereo)
                source.soundModel.audioManager.setStereoPan(source.soundModel.listenerDirection, soundDirection, source.soundModel.properties.audioProperties.volumeBasedStereo.value);
            else
                source.soundModel.audioManager.setMono();

            AudioManager.silentReflectionAudio(source);
        }

        public override float computeAudio(Vector2 listener, PrimarySoundSource source, out Vector2 soundDirection){
            if(source.soundModel.properties.audioProperties.outputChannels.value == AudioManager.OutputChannels.stereo){
                soundDirection = (source.position - listener).normalize();
                if(source.soundModel.properties.debugProperties.visualizeWeightedSoundDirection.value)
                    source.soundModel.manager.debugger.drawLine(listener, listener + soundDirection * 3, 0.1f, 0.2f, ResourceManager.materialColorMagenta);
            }else
                soundDirection = new Vector2(0, 0);

            return source.transferFunction(source.amplitude, Vector2.getSquareMagnitude(source.position, listener), source.soundModel) / source.amplitude;
        }

        public override void visualizeModel(PrimarySoundSource source){
            if(source.soundModel.properties.debugProperties.visualize.value && source.soundModel.properties.debugProperties.visualizePrimarySource.value){
                float degrees = 10;
                for(float i = 0; i < 360; i += degrees){
                    float radians = i * Mathf.Deg2Rad;
                    Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                    source.meshCoordinates.Add(source.position + direction.normalize() * source.amplitude);
                }

                source.meshObject.SetActive(true);
                RenderingUtils.drawSoundMesh(source.meshCoordinates, source.position, source.amplitude, source.amplitude, 0.1f, source.soundModel, source.meshObject);
            }else
                source.meshObject.SetActive(false);
        }
    }
}
