using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class GeometryLoader: MonoBehaviour{
        [Tooltip("Maps from https://movingai.com/benchmarks/grids.html")]
        public bool loadMapFromFile;
 
        [Tooltip("Relative path from Assets/Resources/Maps/")]
        public string fileName;

        public void load(Manager manager){
            if(loadMapFromFile)
                loadFromFile(manager);
            else
                loadFromObjects(manager);
        }

        private void loadFromObjects(Manager manager){
            List<ObstacleData> data = new List<ObstacleData>();

            loadObstacles(transform, data);

            manager.addGeometry(createGeometry(data, manager));

            foreach(float threshold in manager.geometryProperties.geometrySimplificationThresholds.value)
                manager.addGeometry(createGeometry(simplifyObstacles(data, threshold), manager));

            manager.setGeometry();

            Destroy(gameObject);
        }

        private void loadFromFile(Manager manager){
            //List<Vector2[]> vertices = MapLoader.loadMap(mapName);

            //manager.addGeometry(createGeometry(vertices));

            //foreach(float threshold in manager.geometryProperties.geometrySimplificationThresholds.value)
            //    manager.addGeometry(createGeometry(simplifyObstacles(vertices, threshold)));

            //manager.setGeometry();
        }

        private void loadObstacles(Transform transform, List<ObstacleData> data){
            for(int i = 0; i < transform.childCount; i++)
                if(!transform.GetChild(i).gameObject.activeSelf)
                    continue;
                else if(transform.GetChild(i).childCount == 0){
                    ObstacleData obstacleData = transform.GetChild(i).GetComponent<ObstacleData>();
                    
                    if(obstacleData == null)
                        return;

                    obstacleData.initialize();
                    data.Add(obstacleData);
                }else
                    loadObstacles(transform.GetChild(i), data);

            if(data.Count == 0)
                data.Add(createDummyObstacle());
        }

        private ObstacleData createDummyObstacle(){
            GameObject obstacleObject = new GameObject("Dummy Obstacle", typeof(ObstacleData));
            obstacleObject.transform.parent = transform;

            ObstacleData data = obstacleObject.GetComponent<ObstacleData>();
            
            data.vertices = new Vector2[]{
                new Vector2(0, 0),
                new Vector2(100, 0),
                new Vector2(100, 100),
                new Vector2(0, 100)
            };
            data.materials = new SurfaceMaterial[0];

            data.initializeMaterials();

            return data;
        }

        private Geometry createGeometry(List<ObstacleData> data, Manager manager){
            GameObject geometryObject = new GameObject("SoundGeometry", typeof(Geometry));
            geometryObject.transform.parent = transform.parent;
            
            Geometry geometry = geometryObject.GetComponent<Geometry>();
            
            List<Obstacle> obstacles = new List<Obstacle>();

            foreach(ObstacleData obstacleData in data)
                obstacles.Add(new Obstacle(obstacleData, manager.geometryProperties.create3DColliders.value));

            geometry.initialize(obstacles, manager);

            geometry.partitionMap();

            return geometry;
        }

        private Geometry createGeometry(List<Vector2[]> data, Manager manager){
            GameObject geometryObject = new GameObject("SoundGeometry", typeof(Geometry));
            geometryObject.transform.parent = transform.parent;
            
            Geometry geometry = geometryObject.GetComponent<Geometry>();
            
            List<Obstacle> obstacles = new List<Obstacle>();

            foreach(Vector2[] obstacleData in data)
                obstacles.Add(new Obstacle(obstacleData, manager.geometryProperties.create3DColliders.value));

            geometry.initialize(obstacles, manager);

            geometry.partitionMap();

            return geometry;
        }

        private List<ObstacleData> simplifyObstacles(List<ObstacleData> data, float threshold){
            List<ObstacleData> simplifiedData = new List<ObstacleData>();

            foreach(ObstacleData obstacle in data){
                ObstacleData simplifiedObstacle = simplifyObstacle(obstacle, threshold);
                if(simplifiedObstacle != null)
                    simplifiedData.Add(simplifiedObstacle);
            }

            return simplifiedData;
        }

        private List<Vector2[]> simplifyObstacles(List<Vector2[]> data, float threshold){
            List<Vector2[]> simplifiedData = new List<Vector2[]>();

            foreach(Vector2[] obstacle in data){
                Vector2[] simplifiedObstacle = simplifyObstacle(obstacle, threshold);
                if(simplifiedObstacle != null)
                    simplifiedData.Add(simplifiedObstacle);
            }

            return simplifiedData;
        }

        private ObstacleData simplifyObstacle(ObstacleData obstacle, float distanceThreshold){
            List<Vector2> newVertices = new List<Vector2>();
            List<SurfaceMaterial> newMaterials = new List<SurfaceMaterial>();

            Vector2[] vertices = obstacle.vertices;
            SurfaceMaterial[] materials = obstacle.materials;

            float threshold = distanceThreshold * distanceThreshold;

            List<Vector2> members = new List<Vector2>();
            List<SurfaceMaterial> materialMembers = new List<SurfaceMaterial>();
            
            Vector2 center = vertices[0];
            SurfaceMaterial averageMaterial = materials[0];
            
            members.Add(vertices[0]);
            materialMembers.Add(materials[0]);

            for(int i = 1; i < vertices.Length; i++){
                members.Add(vertices[i]);
                materialMembers.Add(materials[i]);

                if(Vector2.getSquareMagnitude(vertices[i], center) < threshold){
                    center = new Vector2(0, 0);
                    averageMaterial = new SurfaceMaterial(0, 0);

                    foreach(Vector2 member in members)
                        center += member;

                    foreach(SurfaceMaterial member in materialMembers)
                        averageMaterial += member;
                    
                    center /= members.Count;
                    averageMaterial /= materialMembers.Count;
                }else{
                    newVertices.Add(center);
                    newMaterials.Add(averageMaterial);

                    members.Clear();
                    materialMembers.Clear();

                    center = vertices[i];
                    averageMaterial = materials[i];

                    members.Add(vertices[i]);
                    materialMembers.Add(materials[i]);
                }
            }

            if(newVertices.Count == 0)
                return null;

            if(Vector2.getSquareMagnitude(newVertices[0], center) < threshold){
                center = newVertices[0];
                averageMaterial = newMaterials[0];

                foreach(Vector2 member in members)
                    center += member;
                
                foreach(SurfaceMaterial member in materialMembers)
                    averageMaterial += member;

                center /= members.Count + 1;
                averageMaterial /= materialMembers.Count + 1;

                newVertices[0] = center;
                newMaterials[0] = averageMaterial;
            }else{
                newVertices.Add(center);
                newMaterials.Add(averageMaterial);
            }

            if(newVertices.Count < 3)
                return null;

            GameObject newObstacleData = Instantiate(obstacle.gameObject);
            newObstacleData.transform.parent = transform;

            ObstacleData newData = newObstacleData.GetComponent<ObstacleData>();
            newData.vertices = newVertices.ToArray();
            newData.materials = newMaterials.ToArray();

            return newData;
        }

        private Vector2[] simplifyObstacle(Vector2[] obstacle, float distanceThreshold){
            List<Vector2> newVertices = new List<Vector2>();
            List<SurfaceMaterial> newMaterials = new List<SurfaceMaterial>();

            Vector2[] vertices = obstacle;

            float threshold = distanceThreshold * distanceThreshold;

            List<Vector2> members = new List<Vector2>();
            List<SurfaceMaterial> materialMembers = new List<SurfaceMaterial>();
            
            Vector2 center = vertices[0];
            
            members.Add(vertices[0]);

            for(int i = 1; i < vertices.Length; i++){
                members.Add(vertices[i]);

                if(Vector2.getSquareMagnitude(vertices[i], center) < threshold){
                    center = new Vector2(0, 0);

                    foreach(Vector2 member in members)
                        center += member;

                    center /= members.Count;
                }else{
                    newVertices.Add(center);

                    members.Clear();
                    materialMembers.Clear();

                    center = vertices[i];

                    members.Add(vertices[i]);
                }
            }

            if(newVertices.Count == 0)
                return null;

            if(Vector2.getSquareMagnitude(newVertices[0], center) < threshold){
                center = newVertices[0];

                foreach(Vector2 member in members)
                    center += member;
                
                center /= members.Count + 1;

                newVertices[0] = center;
            }else{
                newVertices.Add(center);
            }

            if(newVertices.Count < 3)
                return null;

            return newVertices.ToArray();
        }
    }
}
