using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class Obstacle{
        public class ObstacleTreeNode{
            public Obstacle obstacle;
            public ObstacleTreeNode parent;
            public List<ObstacleTreeNode> children;
            public int depth;

            public ObstacleTreeNode(Obstacle obstacle, ObstacleTreeNode parent){
                this.obstacle = obstacle;
                this.parent = parent;
                
                if(parent == null)
                    depth = 1;
                else
                    depth = parent.depth + 1;

                children = new List<ObstacleTreeNode>();
            }

            public void printTree(){
                Debug.Log("Obstacle " + obstacle.ToString() + " at depth " + depth);

                foreach(ObstacleTreeNode child in children)
                    child.printTree();
            }
        }

        public class Vertex{
            public Vector2 position;
            
            public Obstacle obstacle;
            
            public Edge edge1;
            public Edge edge2;
            
            public bool isConcave;

            public Vertex next;
            public Vertex prev;

            public Vertex(Vector2 position, Obstacle obstacle){
                this.position = position;
                this.obstacle = obstacle;
            }
        }

        public Vertex[] vertices;

        public Edge[] edges;

        public GameObject meshObject;
        public GameObject lineRendererObject;

        public Obstacle(ObstacleData obstacleData, bool create3DCollider){
            bool clockWise = Utils.isClockWise(obstacleData.vertices);

            if(!clockWise)
                System.Array.Reverse(obstacleData.vertices);

            vertices = new Vertex[obstacleData.vertices.Length];
            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vertex(obstacleData.vertices[i], this);

            for(int i = 0; i < vertices.Length; i++){
                vertices[i].next = vertices[(i + 1) % vertices.Length];
                vertices[i].prev = vertices[(i - 1 + vertices.Length) % vertices.Length];
            }

            determineConcavePoints();

            defineEdges(obstacleData, clockWise, create3DCollider);

            meshObject = RenderingUtils.create3DMesh(vertices, 5, "Mesh");
            createLineRenderer();
        }

        public Obstacle(Vector2[] obstacleData, bool create3DCollider){
            bool clockWise = Utils.isClockWise(obstacleData);

            if(!clockWise)
                System.Array.Reverse(obstacleData);

            vertices = new Vertex[obstacleData.Length];
            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vertex(obstacleData[i], this);

            for(int i = 0; i < vertices.Length; i++){
                vertices[i].next = vertices[(i + 1) % vertices.Length];
                vertices[i].prev = vertices[(i - 1 + vertices.Length) % vertices.Length];
            }

            determineConcavePoints();

            defineEdgeOnlyVertices(create3DCollider);

            meshObject = RenderingUtils.create3DMesh(vertices, 5, "Mesh");
            createLineRenderer();
        }

        private void defineEdges(ObstacleData data, bool clockWise, bool create3DCollider){
            edges = new Edge[vertices.Length];
            
            Vector2 point1;
            Vector2 point2;

            SurfaceMaterial material;

            GameObject edge;

            for(int i = 0; i < vertices.Length; i++){
                point1 = vertices[i].position;
                point2 = vertices[i].next.position;

                if(clockWise)
                    material = data.materials[i];
                else
                    material = data.materials[(vertices.Length - 1 - i - 1 + vertices.Length) % vertices.Length];

                edge = new GameObject("Edge");
                edges[i] = edge.AddComponent<Edge>();
                edges[i].initialize(point1, point2, this, material, create3DCollider);
                
                vertices[i].edge1 = edges[i];
                vertices[i].next.edge2 = edges[i];
            }
        }

        private void defineEdgeOnlyVertices(bool create3DCollider){
            edges = new Edge[vertices.Length];
            
            Vector2 point1;
            Vector2 point2;

            GameObject edge;

            for(int i = 0; i < vertices.Length; i++){
                point1 = vertices[i].position;
                point2 = vertices[i].next.position;

                edge = new GameObject("Edge");
                edges[i] = edge.AddComponent<Edge>();
                edges[i].initialize(point1, point2, this, SurfaceMaterial.defaultMaterial, create3DCollider);

                vertices[i].edge1 = edges[i];
                vertices[i].next.edge2 = edges[i];
            }
        }

        private void determineConcavePoints(){
            for(int i = 0; i < vertices.Length; i++)
                vertices[i].isConcave = (vertices[i].prev.position.x - vertices[i].next.position.x) * (vertices[i].position.y - vertices[i].next.position.y) - (vertices[i].position.x - vertices[i].next.position.x) * (vertices[i].prev.position.y - vertices[i].next.position.y) > 0;
        }

        private void OnDestroy(){
            Object.Destroy(meshObject);
        }

        public void updateRenderMode(bool renderMode3D, bool visualize){
            meshObject.SetActive(renderMode3D && visualize);
            lineRendererObject.SetActive(!renderMode3D && visualize);
        }

        private void createLineRenderer(){
            lineRendererObject = new GameObject("2D Renderer", typeof(LineRenderer));
            
            LineRenderer renderer = lineRendererObject.GetComponent<LineRenderer>();

            renderer.positionCount = vertices.Length;
            renderer.loop = true;

            renderer.startWidth = 0.4f;
            renderer.endWidth = 0.4f;

            renderer.material = ResourceManager.materialColorGreen;

            for(int i = 0; i < renderer.positionCount; i++)
                renderer.SetPosition(i, new Vector3(vertices[i].position.x, 0.1f, vertices[i].position.y));
        }
    }
}
