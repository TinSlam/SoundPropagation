using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class TransmissionSource: SoundSourceBase{
        public List<Vector2> meshCoordinates = new List<Vector2>();

        public TransmissionSource(Vector2 position, float amplitude, Obstacle.ObstacleTreeNode obstacleDepth){
            this.position = position;
            this.amplitude = amplitude;
            this.obstacleDepth = obstacleDepth;
        }

        public override float computeAmplitude(Vector2 point, SoundModel soundModel){
            return 0;
        }

        public float computeAmplitude(Vector2 point, Obstacle.ObstacleTreeNode depth, SoundModel soundModel){
            if(depth != obstacleDepth)
                return 0;

            return transferFunction(amplitude, Vector2.getSquareMagnitude(position, point), soundModel);
        }

        public void drawMesh(SoundModel soundModel){
            findMeshCoordinates();
            soundModel.debugger.drawSoundMesh(meshCoordinates, position, amplitude, soundModel.source.amplitude, 0.1f);
        }

        private void findMeshCoordinates(){
            float degrees = 10;
            for(float i = 0; i < 360; i += degrees){
                float radians = i * Mathf.Deg2Rad;
                meshCoordinates.Add(position + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * amplitude);
            }
        }
    }
}
