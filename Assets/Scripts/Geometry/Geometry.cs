using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class Geometry: MonoBehaviour{
        [System.Serializable]
        public class Properties: SoundPropagation.Properties{
            public PropertyBool renderMode3D;
            public PropertyBool visualizeGeometry;
            public PropertyBool create3DColliders;
            public PropertyInt activeGeometryIndex;
            public PropertyFloatArray geometrySimplificationThresholds;
            public PropertyInt mapPartitionSize;

            public override void initialize(){
                setProperties(new GenericProperty[]{
                    renderMode3D,
                    visualizeGeometry,
                    create3DColliders,
                    activeGeometryIndex,
                    geometrySimplificationThresholds,
                    mapPartitionSize
                });
            }
        }

        public struct PartitionedVertex{
            public Vector2 vertex;
            public int obstacleIndex;
            public Obstacle obstacle;
            public Obstacle.ObstacleTreeNode obstacleDepth;

            public PartitionedVertex(Vector2 vertex, int obstacleIndex, Obstacle obstacle, Obstacle.ObstacleTreeNode obstacleDepth){
                this.vertex = vertex;
                this.obstacleIndex = obstacleIndex;
                this.obstacle = obstacle;
                this.obstacleDepth = obstacleDepth;
            }
        }

        public List<Obstacle.ObstacleTreeNode> obstacleTree = new List<Obstacle.ObstacleTreeNode>();
        public List<PartitionedVertex>[,] partitionedMap;
        public Dictionary<Vector2, Edge[]> vertexEdgeMap = new Dictionary<Vector2, Edge[]>();

        private Manager manager;

        public Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        public Vector2 max = new Vector2(float.MinValue, float.MinValue);
        public Vector2 center;
        public float width;
        public float height;

        public int partitionedMapWidth;
        public int partitionedMapHeight;

        public const byte INTERACTION_NONE = 0;
        public const byte INTERACTION_CONTAINS = 1;
        public const byte INTERACTION_CONTAINED = 2;

        public struct CachedDiffractionEntry{
            public float distance;
            public Vector2 direction;

            public CachedDiffractionEntry(float distance, Vector2 direction){
                this.distance = distance;
                this.direction = direction.normalize();
            }
        }

        public Dictionary<Obstacle.Vertex, Dictionary<Obstacle.Vertex, CachedDiffractionEntry>> cachedDiffractionTable = new Dictionary<Obstacle.Vertex, Dictionary<Obstacle.Vertex, CachedDiffractionEntry>>();

        public void initialize(List<Obstacle> obstacles, Manager manager){
            this.manager = manager;

            createObstacleTree(obstacles);
            structureGameObjects();

            center = (min + max) / 2;
            width = max.x - min.x;
            height = max.y - min.y;
        }

        private void structureGameObjects(){
            GameObject obstacleTreeObject = new GameObject("Obstacles");
            obstacleTreeObject.transform.parent = transform;

            foreach(Obstacle.ObstacleTreeNode node in obstacleTree)
                structureGameObjects(node, obstacleTreeObject);
        }

        private void structureGameObjects(Obstacle.ObstacleTreeNode node, GameObject nodeObject){
            GameObject obstacleObject = new GameObject("Obstacle");
            obstacleObject.transform.parent = nodeObject.transform;

            node.obstacle.meshObject.transform.parent = obstacleObject.transform;
            node.obstacle.lineRendererObject.transform.parent = obstacleObject.transform;

            GameObject edgesObject = new GameObject("Edges");
            edgesObject.transform.parent = obstacleObject.transform;

            foreach(Edge edge in node.obstacle.edges){
                edge.gameObject.transform.parent = edgesObject.transform;

                Vector2[] points = new Vector2[]{ edge.point1, edge.point2 };

                foreach(Vector2 point in points){
                    Edge[] edges;
                    bool exists = vertexEdgeMap.TryGetValue(point, out edges);

                    if(exists)
                        edges[1] = edge;
                    else{
                        edges = new Edge[2];
                        edges[0] = edge;
                        vertexEdgeMap.Add(point, edges);
                    }
                }
            }

            for(int i = 0; i < node.obstacle.vertices.Length; i++){
                Vector2 point = node.obstacle.vertices[i].position;

                if(point.x < min.x)
                    min.x = point.x;

                if(point.x > max.x)
                    max.x = point.x;

                if(point.y < min.y)
                    min.y = point.y;

                if(point.y > max.y)
                    max.y = point.y;
            }

            if(node.children.Count == 0)
                return;

            GameObject childrenObject = new GameObject("Children");
            childrenObject.transform.parent = obstacleObject.transform;

            foreach(Obstacle.ObstacleTreeNode child in node.children)
                structureGameObjects(child, childrenObject);

            //createCachedDiffractionTable();
        }

        // Not properly tested yet.
        private void createCachedDiffractionTable(){
            List<Obstacle.Vertex> vertices = new List<Obstacle.Vertex>();
            grabAllVertices(vertices);

            foreach(Obstacle.Vertex vertexKey in vertices){
                Dictionary<Obstacle.Vertex, CachedDiffractionEntry> map = new Dictionary<Obstacle.Vertex, CachedDiffractionEntry>();
                cachedDiffractionTable.Add(vertexKey, map);

                foreach(Obstacle.Vertex vertexValue in vertices){
                    if(!Utils.oneWayBitangent(vertexKey.position, vertexValue.position, vertexValue.prev.position, vertexValue.next.position))
                        continue;

                    if(!Utils.isInVision(vertexKey.position, vertexValue.position))
                        continue;

                    Vector2 direction = vertexValue.position - vertexKey.position;
                    float distance = direction.getMagnitude();
                    
                    map.Add(vertexValue, new CachedDiffractionEntry(distance, direction));
                }
            }
        }

        private void grabAllVertices(List<Obstacle.Vertex> vertices){
            foreach(Obstacle.ObstacleTreeNode node in obstacleTree)
                grabAllVertices(node, vertices);
        }

        private void grabAllVertices(Obstacle.ObstacleTreeNode node, List<Obstacle.Vertex> vertices){
            foreach(Obstacle.Vertex vertex in node.obstacle.vertices)
                vertices.Add(vertex);

            foreach(Obstacle.ObstacleTreeNode child in node.children)
                grabAllVertices(child, vertices);
        }

        public static int interaction(Obstacle obstacle1, Obstacle obstacle2){
            if(Utils.isPointInsidePolygon(obstacle1.vertices[0].position, obstacle2.vertices))
                return INTERACTION_CONTAINED;
            if(Utils.isPointInsidePolygon(obstacle2.vertices[0].position, obstacle1.vertices))
                return INTERACTION_CONTAINS;
            return INTERACTION_NONE;
        }

        private void createObstacleTree(List<Obstacle> obstacles){
            obstacleTree = new List<Obstacle.ObstacleTreeNode>();
            List<Obstacle.ObstacleTreeNode> visitedNodes = new List<Obstacle.ObstacleTreeNode>();

            foreach(Obstacle obstacle in obstacles){
                List<Obstacle.ObstacleTreeNode> containsList = new List<Obstacle.ObstacleTreeNode>();
                List<Obstacle.ObstacleTreeNode> containedList = new List<Obstacle.ObstacleTreeNode>();

                Obstacle.ObstacleTreeNode currentNode = new Obstacle.ObstacleTreeNode(obstacle, null);

                foreach(Obstacle.ObstacleTreeNode node in visitedNodes){
                    switch(interaction(obstacle, node.obstacle)){
                        case INTERACTION_CONTAINS:
                            containsList.Add(node);
                            break;

                        case INTERACTION_CONTAINED:
                            containedList.Add(node);
                            break;

                        default:
                            break;
                    }
                }

                if(containedList.Count > 0){
                    Obstacle.ObstacleTreeNode parent = containedList[0];
                    for(int i = 1; i < containedList.Count; i++)
                        if(containedList[i].depth > parent.depth)
                            parent = containedList[i];

                    currentNode.parent = parent;
                    currentNode.depth = parent.depth + 1;
                }

                foreach(Obstacle.ObstacleTreeNode child in containsList){
                    child.depth++;
                    if(child.parent == currentNode.parent)
                        child.parent = currentNode;
                }

                visitedNodes.Add(currentNode);
            }

            foreach(Obstacle.ObstacleTreeNode node in visitedNodes)
                if(node.depth == 1)
                    obstacleTree.Add(node);

            foreach(Obstacle.ObstacleTreeNode node in visitedNodes)
                if(node.parent != null)
                    node.parent.children.Add(node);
        }

        public void updateRenderMode(bool mode){
            foreach(Obstacle.ObstacleTreeNode node in obstacleTree)
                updateRenderMode(mode, node);
        }

        private void updateRenderMode(bool mode, Obstacle.ObstacleTreeNode node){
            node.obstacle.updateRenderMode(mode, manager.geometryProperties.visualizeGeometry.value);

            foreach(Obstacle.ObstacleTreeNode child in node.children)
                updateRenderMode(mode, child);
        }

        public Obstacle.ObstacleTreeNode findObstacleDepth(Vector2 position){
            Obstacle.ObstacleTreeNode obstacleDepthNode = null;

            List<Obstacle.ObstacleTreeNode> currentList = obstacleTree;

            while(true){
                bool goDeeper = false;

                foreach(Obstacle.ObstacleTreeNode child in currentList)
                    if(Utils.isPointInsidePolygon(position, child.obstacle.vertices)){
                        obstacleDepthNode = child;
                        currentList = child.children;
                        goDeeper = true;
                        break;
                    }

                if(!goDeeper)
                    break;
            }

            return obstacleDepthNode;
        }

        public void partitionMap(){
            partitionedMapWidth = ((int) Mathf.Ceil(width) - 1) / manager.geometryProperties.mapPartitionSize.value + 1;
            partitionedMapHeight = ((int) Mathf.Ceil(height) - 1) / manager.geometryProperties.mapPartitionSize.value + 1;
            
            partitionedMap = new List<PartitionedVertex>[partitionedMapWidth, partitionedMapHeight];

            for(int i = 0; i < partitionedMapWidth; i++)
                for(int j = 0; j < partitionedMapHeight; j++)
                    partitionedMap[i, j] = new List<PartitionedVertex>();

            foreach(Obstacle.ObstacleTreeNode node in obstacleTree)
                addToPartition(null, node);
        }

        private void addToPartition(Obstacle.ObstacleTreeNode parent, Obstacle.ObstacleTreeNode node){
            Obstacle obstacle = node.obstacle;
            for(int i = 0; i < obstacle.vertices.Length; i++){
                Vector2 vector = obstacle.vertices[i].position;
                
                int xPartition = (int) ((vector.x - min.x) / manager.geometryProperties.mapPartitionSize.value);
                int yPartition = (int) ((vector.y - min.y) / manager.geometryProperties.mapPartitionSize.value);

                if(xPartition == partitionedMapWidth)
                    xPartition--;
                
                if(yPartition == partitionedMapHeight)
                    yPartition--;
                
                partitionedMap[xPartition, yPartition].Add(new PartitionedVertex(vector, i, obstacle, parent));
            }

            foreach(Obstacle.ObstacleTreeNode child in node.children)
                addToPartition(node, child);
        }
    }
}
