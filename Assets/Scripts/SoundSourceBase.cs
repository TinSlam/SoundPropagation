using UnityEngine;

namespace SoundPropagation{
    public abstract class SoundSourceBase{
        [HideInInspector] public float amplitude;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public float range;

        public Obstacle.ObstacleTreeNode obstacleDepth;

        public abstract float computeAmplitude(Vector2 point, SoundModel soundModel);

        public float transferFunction(float sourceAmplitude, float distanceSquared, SoundModel soundModel) {
            return transferFunction(sourceAmplitude, distanceSquared, soundModel.properties.modelProperties.decayingFactor.value, soundModel.properties.modelProperties.transferFunctionMethod.value);
        }

        public float transferFunction(float sourceAmplitude, Vector2 destination, DiffractionSource diffractionSource, SoundModel soundModel) {
            float distance = Vector2.getDistance(diffractionSource.position, destination);

            float scale = diffractionSource.scaleRange(destination, soundModel);

            if(scale == 0)
                scale = 0.00001f;

            distance /= scale;

            return transferFunction(sourceAmplitude, distance * distance, soundModel);
        }

        public float transferFunction(float sourceAmplitude, float distanceSquared, float decayingFactor, TransferFunctionMethod method) {
            float range;

            switch(method) {
                case TransferFunctionMethod.distance:
                    return Mathf.Max(0, sourceAmplitude - decayingFactor * Mathf.Sqrt(distanceSquared));

                case TransferFunctionMethod.squaredDistance:
                    range = calculateRange(sourceAmplitude, decayingFactor, method);
                    return sourceAmplitude * Mathf.Pow(Mathf.Max(0, range - Mathf.Sqrt(distanceSquared)), 2) / Mathf.Pow(range, 2);

                default:
                    return 0;
            }
        }

        public float calculateRange(float amplitude, SoundModel soundModel) {
            return calculateRange(amplitude, soundModel.properties.modelProperties.decayingFactor.value, soundModel.properties.modelProperties.transferFunctionMethod.value);
        }

        public float calculateRange(float amplitude, float decayingFactor, TransferFunctionMethod transferFunctionMethod) {
            switch(transferFunctionMethod) {
                case TransferFunctionMethod.distance:
                    return amplitude / decayingFactor;

                case TransferFunctionMethod.squaredDistance:
                    return Mathf.Sqrt(amplitude / decayingFactor);

                default:
                    return 0;
            }
        }

        public enum TransferFunctionMethod {
            distance,
            squaredDistance
        };

        [System.Serializable]
        public class Properties: SoundPropagation.Properties {
            public PropertySoundModelMethodEnum soundModelMethod;
            public PropertyFloat maxVolume;
            public PropertyFloat amplitude;
            public PropertyFloat decayingFactor;
            public PropertyTransferFunctionEnum transferFunctionMethod;

            public PropertyBool diffraction;
            public PropertyBool decayingDiffraction;
            public PropertyBool diffractionStaticCaching;
            public PropertyBool diffractionGreedySearch;
            public PropertyBool diffractionRemoveRedundantCorners;
            public PropertyBool diffractionMapPartitioning;

            public PropertyBool transmission;
            public PropertyInt transmissionRayCount;

            public PropertyBool reflection;
            public PropertyInt reflectionRayCount;
            public PropertyInt reflectionBounceLimit;
            public PropertyFloat reflectionMinimumRangeLimit;
            public PropertyFloat reflectionDistanceDelayFactor;

            public override void initialize() {
                setProperties(new GenericProperty[]{
                    soundModelMethod,
                    maxVolume,
                    amplitude,
                    decayingFactor,
                    transferFunctionMethod,
                    diffraction,
                    decayingDiffraction,
                    diffractionStaticCaching,
                    diffractionGreedySearch,
                    diffractionRemoveRedundantCorners,
                    diffractionMapPartitioning,
                    transmission,
                    transmissionRayCount,
                    reflection,
                    reflectionRayCount,
                    reflectionBounceLimit,
                    reflectionMinimumRangeLimit,
                    reflectionDistanceDelayFactor
                });
            }
        }
    }
}
