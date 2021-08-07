using UnityEngine;

namespace SoundPropagation{
    public class ObstacleData: MonoBehaviour{
        public bool addNoise = true;

        [HideInInspector] public Vector2[] vertices;
        public SurfaceMaterial[] materials;

        public SurfaceMaterial defaultMaterial;

        public void initialize(){
            LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();

            vertices = new Vector2[lineRenderer.positionCount];

            for(int i = 0; i < lineRenderer.positionCount; i++)
                vertices[i] = convertPoint(lineRenderer.GetPosition(i));

            initializeMaterials();
        }

        public void initializeMaterials(){
            if(materials.Length == 0){
                materials = new SurfaceMaterial[vertices.Length];

                for(int i = 0; i < vertices.Length; i++)
                    materials[i] = defaultMaterial;
            }
        }

        private Vector2 convertPoint(Vector3 vector){
            if(!addNoise)
                return new Vector2(vector.x, vector.z);

            float epsilon = 0.05f;
            return new Vector2(vector.x + Random.Range(-epsilon, epsilon), vector.z + Random.Range(-epsilon, epsilon));
        }
    }
}
