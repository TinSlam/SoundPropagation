using UnityEngine;

namespace SoundPropagation{
    public class ReflectionSource: SoundSourceBase{
        public Vector2 direction;
        public Vector2 normal;
        private float distanceTravelled;

        private Properties properties;

        public ReflectionSource(Vector2 position, float amplitude, Vector2 direction, Vector2 normal, float distanceTravelled, Properties properties){
            this.position = position;
            this.amplitude = amplitude;
            this.direction = direction;
            this.normal = normal;
            this.distanceTravelled = distanceTravelled;
            this.properties = properties;
        }

        public float computeAmplitude(Vector2 point, out float delay, SoundModel soundModel){
            delay = 0;

            Vector2 direction = point - position;
            
            if(Vector2.dot(direction, normal) <= 0)
                return 0;
            
            float distance = direction.getMagnitude();

            if(amplitude < distance)
                return 0;

            if(!Utils.isInVision(position, point))
                return 0;

            delay = (distance + distanceTravelled) * properties.reflectionDistanceDelayFactor.value;
            
            direction /= distance;

            return transferFunction(amplitude, distance * distance, soundModel) * Mathf.Pow(Vector2.dot(direction, this.direction), 4);
        }

        public override float computeAmplitude(Vector2 point, SoundModel soundModel){
            return 0;
        }
    }
}
