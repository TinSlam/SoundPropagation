using UnityEngine;

namespace SoundPropagation{
    public class Edge: MonoBehaviour{
        public Obstacle obstacle;

        public Vector2 point1;
        public Vector2 point2;
        public LineSegment2D edge;

        public float size;

        public SurfaceMaterial material;

        private GameObject collider3DObject;

        public void initialize(Vector2 point1, Vector2 point2, Obstacle obstacle, SurfaceMaterial material, bool create3DCollider){
            this.point1 = point1;
            this.point2 = point2;

            size = Vector2.getDistance(point1, point2);

            this.obstacle = obstacle;

            edge = new LineSegment2D(point1, point2);
            this.material = material;

            EdgeCollider2D collider = gameObject.AddComponent<EdgeCollider2D>();
            collider.points = new UnityEngine.Vector2[]{Vector2.convert(point1), Vector2.convert(point2)};

            if(create3DCollider){
                collider3DObject = new GameObject("3D Collider");
                collider3DObject.transform.parent = transform;
                collider3DObject.AddComponent<BoxCollider>();
                collider3DObject.transform.position = new Vector3((point1.x + point2.x) / 2, 0, (point1.y + point2.y) / 2);
                collider3DObject.transform.rotation = Quaternion.Euler(0, -Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg, 0);
                collider3DObject.transform.localScale = new Vector3(Vector2.getDistance(point1, point2), 6, 0.1f);
            }

            gameObject.layer =  Utils.layer;
        }
    }
}
