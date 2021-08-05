using UnityEngine;

namespace SoundPropagation{
    public class ResourceManager{
        public static Material materialColorRed;
        public static Material materialColorGreen;
        public static Material materialColorBlue;
        public static Material materialColorYellow;
        public static Material materialColorMagenta;
    
        public static Material meshMaterial;
        public static Material soundModelMaterial;

        public static GameObject pointPrefab;
        public static GameObject linePrefab;
        public static GameObject circlePrefab;
        public static GameObject soundMeshPrefab;
        public static GameObject beaconPrefab;

        public static void initializeResources(){
            initializeMaterials();
            loadPrefabs();
        }

        public static void cleanUpResources(){
            cleanUpMaterials();
        }

        private static void initializeMaterials(){
            materialColorRed = createColorMaterial(Color.red);
            materialColorGreen = createColorMaterial(Color.green);
            materialColorBlue = createColorMaterial(Color.blue);
            materialColorYellow = createColorMaterial(Color.yellow);
            materialColorMagenta = createColorMaterial(Color.magenta);
        
            meshMaterial = new Material(Shader.Find("ColorTransparent"));
            soundModelMaterial = new Material(Shader.Find("SoundModel"));
        }

        private static void cleanUpMaterials(){
            Object.Destroy(materialColorRed);
            Object.Destroy(materialColorGreen);
            Object.Destroy(materialColorBlue);
            Object.Destroy(materialColorYellow);
            Object.Destroy(materialColorMagenta);

            Object.Destroy(meshMaterial);
        }

        private static void loadPrefabs(){
            pointPrefab = Resources.Load("Prefabs/Debug/Point") as GameObject;
            linePrefab = Resources.Load("Prefabs/Debug/Line") as GameObject;
            
            circlePrefab = Resources.Load("Prefabs/Debug/Circle") as GameObject;
            {
                GameObject gameObject = Object.Instantiate(circlePrefab);
                LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
                
                Vector3[] circlePoints = new Vector3[lineRenderer.positionCount];
                for(int i = 0; i < circlePoints.Length; i++)
                    circlePoints[i] = lineRenderer.GetPosition(i);

                Object.Destroy(gameObject);
                RenderingUtils.circlePoints = circlePoints;
            }

            soundMeshPrefab = Resources.Load("Prefabs/Debug/SoundMesh") as GameObject;
            soundMeshPrefab.GetComponent<MeshRenderer>().material = soundModelMaterial;

            beaconPrefab = Resources.Load("Prefabs/Debug/Beacon") as GameObject;
        }

        private static Material createColorMaterial(Color color){
            Material material = new Material(Shader.Find("Color"));
            material.SetColor("_color", color);
            return material;
        }

        public static GameObject instantiatePrefab(string path){
            return Object.Instantiate(Resources.Load(path)) as GameObject;
        }
    }
}
