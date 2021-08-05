using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class RenderingUtils{
        public static Vector3[] circlePoints;

        public static GameObject createMeshObject(string name, Material material){
            GameObject meshObject = new GameObject("", typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.name = name;
            Mesh mesh = new Mesh();
            meshObject.GetComponent<MeshFilter>().mesh = mesh;
            meshObject.GetComponent<MeshRenderer>().material = material;
            return meshObject;
        }

        public static GameObject drawSoundMesh(List<Vector2> points, Vector2 position, float amplitude, float maxAmplitude, float depth, SoundModel soundModel, GameObject meshObject){
            Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;
            mesh.Clear();

            Vector3[] vertices = new Vector3[points.Count + 1];
            int[] triangles = new int[3 * points.Count];
            Color[] colors = new Color[vertices.Length];

            vertices[0] = new Vector3(position.x, depth, position.y);
            colors[0] = Color.Lerp(Color.black, Color.white, amplitude / maxAmplitude);

            for(int i = 0; i < points.Count; i++){
                vertices[i + 1] = new Vector3(points[i].x, depth, points[i].y);
                colors[i + 1] = Color.Lerp(Color.black, Color.white, soundModel.source.transferFunction(amplitude, Vector2.getSquareMagnitude(position, points[i]), soundModel) / maxAmplitude);
            }

            for(int i = 0; i < points.Count; i++){
                int currentIndex = i + 1;
                int nextIndex = i + 2;

                if(i == points.Count - 1)
                    nextIndex = 1;

                triangles[3 * i + 0] = nextIndex;
                triangles[3 * i + 1] = currentIndex;
                triangles[3 * i + 2] = 0;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            
            return meshObject;
        }

        public static GameObject drawSoundMeshDiffraction(List<Vector2> points, Vector2 position, float amplitude, float maxAmplitude, float depth, DiffractionSource diffractionSource, SoundModel soundModel, GameObject meshObject){
            Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;
            mesh.Clear();

            Vector3[] vertices = new Vector3[points.Count + 1];
            int[] triangles = new int[3 * points.Count];
            Color[] colors = new Color[vertices.Length];

            vertices[0] = new Vector3(position.x, depth, position.y);
            colors[0] = Color.Lerp(Color.black, Color.white, amplitude / maxAmplitude);

            for(int i = 0; i < points.Count; i++){
                vertices[i + 1] = new Vector3(points[i].x, depth, points[i].y);
                colors[i + 1] = Color.Lerp(Color.black, Color.white, soundModel.source.transferFunction(amplitude, points[i], diffractionSource, soundModel) / maxAmplitude);
                //colors[i + 1] = Color.Lerp(Color.black, Color.white, soundModel.source.transferFunction(amplitude, Vector2.getSquareMagnitude(position, points[i]), soundModel) / maxAmplitude);
            }

            for(int i = 0; i < points.Count; i++){
                int currentIndex = i + 1;
                int nextIndex = i + 2;

                if(i == points.Count - 1)
                    nextIndex = 1;

                triangles[3 * i + 0] = nextIndex;
                triangles[3 * i + 1] = currentIndex;
                triangles[3 * i + 2] = 0;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            
            return meshObject;
        }

        public static GameObject create3DMesh(Obstacle.Vertex[] points, float height, string name){
            GameObject meshObject = createMeshObject(name, ResourceManager.meshMaterial);

            Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;
            mesh.Clear();

            Vector3[] vertices = new Vector3[2 * points.Length];
            int[] triangles = new int[3 * 2 * points.Length * 2];
            Color[] colors = new Color[2 * points.Length];

            Color color = new Color(0.3f, 0.3f, 0.3f);

            for(int i = 0; i < points.Length; i++){
                Vector3 current = Vector2.convert(points[i].position);
            
                vertices[i] = new Vector3(current.x, 0, current.y);
                colors[i] = color;

                vertices[i + points.Length] = new Vector3(current.x, height, current.y);
                colors[i + points.Length] = color;
            }

            int offset = 3 * 2 * points.Length;

            for(int i = 0; i < points.Length; i++){
                int currentIndex = i;
                int nextIndex = (i + 1) % points.Length;

                int currentIndexUp = currentIndex + points.Length;
                int nextIndexUp = nextIndex + points.Length;

                triangles[i * 6 + 0] = currentIndex;
                triangles[i * 6 + 1] = nextIndex;
                triangles[i * 6 + 2] = nextIndexUp;

                triangles[i * 6 + 3] = nextIndexUp;
                triangles[i * 6 + 4] = currentIndexUp;
                triangles[i * 6 + 5] = currentIndex;

                triangles[i * 6 + 0 + offset] = currentIndex;
                triangles[i * 6 + 2 + offset] = nextIndex;
                triangles[i * 6 + 1 + offset] = nextIndexUp;

                triangles[i * 6 + 3 + offset] = nextIndexUp;
                triangles[i * 6 + 5 + offset] = currentIndexUp;
                triangles[i * 6 + 4 + offset] = currentIndex;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;

            return meshObject;
        }

        public static void drawPoint(Vector2 point, float size, float y, Material material, GameObject renderer){
            renderer.GetComponent<MeshRenderer>().material = material;
            renderer.transform.position = new Vector3(point.x, y, point.y);
            renderer.transform.localScale = new Vector3(size, size, 1);
        }

        public static void drawLine(Vector2 start, Vector2 end, float size, float y, Material material, GameObject renderer){
            LineRenderer lineRenderer = renderer.GetComponent<LineRenderer>();
            lineRenderer.material = material;

            lineRenderer.SetPosition(0, new Vector3(start.x, y, start.y));
            lineRenderer.SetPosition(1, new Vector3(end.x, y, end.y));

            lineRenderer.startWidth = size;
            lineRenderer.endWidth = size;
        }

        public static void drawCircle(Vector2 center, float radius, float size, float depth, Material material, GameObject renderer){
            LineRenderer lineRenderer = renderer.GetComponent<LineRenderer>();
            lineRenderer.material = material;

            lineRenderer.startWidth = size;
            lineRenderer.endWidth = size;

            Vector3 offset = new Vector3(center.x, depth, center.y);

            for(int i = 0; i < lineRenderer.positionCount; i++)
                lineRenderer.SetPosition(i, circlePoints[i] * radius + offset);
        }
    }
}
