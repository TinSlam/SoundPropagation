using UnityEngine;

namespace SoundPropagation{
    public class SoundProperties: MonoBehaviour{
        public SoundSourceBase.Properties modelProperties;
        public AudioManager.Properties audioProperties;
        public SoundModelDebugger.Properties debugProperties;

        public void initialize(){
            audioProperties.initialize();
            modelProperties.initialize();
            debugProperties.initialize();
        }
    }
}
