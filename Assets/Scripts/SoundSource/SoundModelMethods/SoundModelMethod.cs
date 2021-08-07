using System.Collections.Generic;

namespace SoundPropagation{
    public abstract class SoundModelMethod{
        public enum Method{
            OUR_METHOD,
            EUCLIDEAN
        };
        
        public static EuclideanMethod euclideanMethod = new EuclideanMethod();
        public static OurStaticMethod ourStaticMethod = new OurStaticMethod();

        public abstract void computeModel(PrimarySoundSource source);
        public abstract void computeSound(Vector2 listener, PrimarySoundSource source);
        public abstract void visualizeModel(PrimarySoundSource source);
        public abstract float computeAudio(Vector2 listener, PrimarySoundSource source, out Vector2 soundDirection);

        public Vector2 computeSoundDirection(List<AudioManager.DirectedSound> diffraction, List<AudioManager.DirectedSound> transmission){
            Vector2 result = new Vector2(0, 0);

            float amp = 0;

            for(int i = 0; i < diffraction.Count; i++){
                amp += diffraction[i].amplitude;
                result += diffraction[i].direction * diffraction[i].amplitude;
            }

            for(int i = 0; i < transmission.Count; i++){
                amp += transmission[i].amplitude;
                result += transmission[i].direction * transmission[i].amplitude;
            }

            if(amp == 0)
                return new Vector2(0, 0);

            return result / amp;
        }
    }
}
