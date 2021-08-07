using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class Utils{
        public static int layer = LayerMask.NameToLayer("SoundModels");
        public static int soundModelLayer = 1 << layer;
        public static int ignoreSoundModelLayer = ~soundModelLayer;
        public static Camera camera2D;

        private const double epsilon = 2e-3;

        public static bool floatLess(float value, float other){
            return (other - value) > epsilon;
        }

        public static bool floatGreater(float value, float other){
            return (value - other) > epsilon;
        }

        public static bool floatEqual(float value, float other){
            return Mathf.Abs(value - other) < epsilon;
        }

        public static bool vectorsEqual(Vector2 vector1, Vector2 vector2){
            return Vector2.getSquareMagnitude(vector1, vector2) < 0.005f;
        }

        public static bool directionsEqual(Vector2 vector1, Vector2 vector2){
            return Vector2.getAngleBetweenVectors(vector1, vector2) < 1;
        }

        public static bool vectorsEqualPrecise(Vector2 vector1, Vector2 vector2){
            return Vector2.getDistance(vector1, vector2) < 0.005f;
        }

        public static string getKey(Vector2 point){
            return point.ToString();
        }

        public static float getAngleInDegrees(Vector2 vector){
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }

        public static float convertAngleTo360Degrees(float angle){
            while(angle < 0)
                angle += 360;
            while(angle >= 360)
                angle -= 360;
            return angle;
        }

        public static Vector2 getMousePosition(){
            return Vector2.convert(camera2D.ScreenToWorldPoint(Input.mousePosition));
        }

        public static bool isInsideClockRangeOfTwoVector(Vector2 start, Vector2 end, Vector2 vector){
            if(vectorsEqual(vector.normalize(), start.normalize()) || vectorsEqual(vector.normalize(), end.normalize()))
                return true;

            float angle1 = Vector2.getAngleBetweenVectors(start, vector);
            if (floatGreater(start.x * vector.y - start.y * vector.x, 0f))
                angle1 = 360 - angle1;

            float angle2 = Vector2.getAngleBetweenVectors(vector, end);
            if (floatGreater(vector.x * end.y - vector.y * end.x, 0f))
                angle2 = 360 - angle2;

            float angle3 = Vector2.getAngleBetweenVectors(start, end);
            if (floatGreater(start.x * end.y - start.y * end.x, 0f))
                angle3 = 360 - angle3;

            // TODO: Delete this if buggy :D
            float angle = angle1 + angle2;
            while(angle > 360)
                angle -= 360;
            angle = angle1 + angle2;

            return floatEqual(angle, angle3);
        }

        public static float clockwiseAngle(Vector2 from, Vector2 to){
            if(vectorsEqual(from.normalize(), to.normalize()))
                return 0f;

            return from.x * to.y - from.y * to.x > 0 ? 360 - Vector2.getAngleBetweenVectors(from, to) : Vector2.getAngleBetweenVectors(from, to);
        }

        public static float clockwiseAngle(Vector3 from, Vector3 to, Vector3 normal){
            float angle = Vector3.Angle(from, to);

            if(Vector3.Dot(normal, Vector3.Cross(from, to)) <= 0)
                return angle;
            else
                return 360 - angle;
        }

        public static bool isPointInsidePolygon(Vector2 point, Obstacle.Vertex[] polygonVertices){
            Ray2D ray = new Ray2D(Vector2.convert(point), new Vector3(0, 1));
            int intersections = 0;

            for(int i = 1; i < polygonVertices.Length; i++)
                if(detectIntersection(ray, polygonVertices[i - 1].position, polygonVertices[i].position))
                    intersections++;

            if(!vectorsEqual(polygonVertices[0].position, polygonVertices[polygonVertices.Length - 1].position))
                if(detectIntersection(ray, polygonVertices[polygonVertices.Length - 1].position, polygonVertices[0].position))
                    intersections++;

            return intersections % 2 == 1;
        }

        private static bool detectIntersection(Ray2D ray, Vector2 p1, Vector2 p2){
            float pointY;
            
            if(floatEqual(p1.x, p2.x))
                return false;
            else if(floatEqual(p1.y, p2.y))
                pointY = p1.y;
            else{
                float a = p1.x - p2.x;
                float b = p1.y - p2.y;
                float c = p2.y / b - p2.x / a;

                pointY = b / a * ray.origin.x + b * c;
            }

            if(floatLess(pointY, ray.origin.y))
                return false;
            else{
                Vector2 leftP = floatLess(p1.x, p2.x) ? p1 : p2;
                Vector2 rightP = floatLess(p1.x, p2.x) ? p2 : p1;
            
                if(!floatGreater(ray.origin.x, leftP.x) || floatGreater(ray.origin.x, rightP.x))
                    return false;
            }

            return true;
        }

        public static float getAngleInDegrees(Vector2 vector1, Vector2 vector2){
            float dot = Vector2.dot(vector1, vector2) / (vector1.getMagnitude() * vector2.getMagnitude());
            if(dot > 1)
                return 0;
            if(dot < -1)
                return 180;
            return Mathf.Acos(dot) * Mathf.Rad2Deg;
        }

        public static bool isInVision(Vector2 source, Vector2 point){
            Vector2 vector = point - source;
            RaycastHit2D[] hits = Physics2D.RaycastAll(Vector2.convert(source), Vector2.convert(vector), vector.getMagnitude() - 0.1f, soundModelLayer);

            foreach(RaycastHit2D hit in hits)
                if(hit.distance > 0.1f && Vector2.getSquareMagnitude(new Vector2(hit.point), point) > 0.5f)
                    return false;

            return true;
        }
        public static bool isInVision(Vector2 source, Vector2 direction, float distance){
            RaycastHit2D[] hits = Physics2D.RaycastAll(Vector2.convert(source), Vector2.convert(direction), distance - 0.1f, soundModelLayer);

            foreach(RaycastHit2D hit in hits)
                if(hit.distance > 0.1f)
                    return false;

            return true;
        }

        public static bool isInVision(UnityEngine.Vector2 source, UnityEngine.Vector2 point){
            UnityEngine.Vector2 vector = point - source;
            RaycastHit2D[] hits = Physics2D.RaycastAll(source, vector, vector.magnitude, soundModelLayer);

            foreach(RaycastHit2D hit in hits)
                if(UnityEngine.Vector2.Distance(hit.point, source) > 0.1f && UnityEngine.Vector2.Distance(hit.point, point) > 0.1f)
                    return false;

            return true;
        }

        public static bool isLeftTurn(Vector2 p, Vector2 q, Vector2 r){
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y) < 0;
        }

        public static RaycastHit2D[] raycastAll(Vector2 position, Vector2 direction, float distance, out int offset){
            RaycastHit2D[] hits = Physics2D.RaycastAll(Vector2.convert(position), Vector2.convert(direction), distance, soundModelLayer);

            offset = 0;
            while(hits.Length > offset && hits[offset].distance < 0.05f)
                offset++;

            return hits;
        }

        public static Vector2[] findNeighbors(Vector2 point, Edge[] edges){
            Vector2[] neighbors = new Vector2[2];

            if(point == edges[0].point1)
                neighbors[0] = edges[0].point2;
            else
                neighbors[0] = edges[0].point1;

            if(point == edges[1].point1)
                neighbors[1] = edges[1].point2;
            else
                neighbors[1] = edges[1].point1;

            return neighbors;
        }

        public static Vector2 weightedDirection(Vector2[] directions, float[] amplitudes){
            Vector2 result = new Vector2(0, 0);

            float amp = 0;

            for(int i = 0; i < directions.Length; i++){
                amp += amplitudes[i];
                result += directions[i] * amplitudes[i];
            }

            if(amp == 0)
                return new Vector2(0, 0);

            return result / amp;
        }

        public static int[] generateRandomSequence(int size){
            int[] sequence = new int[size];

            List<int> bag = new List<int>();
            for(int i = 0; i < size; i++)
                bag.Add(i);

            for(int i = 0; i < size; i++){
                int rng = Random.Range(0, bag.Count);
                sequence[i] = bag[rng];
                bag.RemoveAt(rng);
            }

            return sequence;
        }

        public static float orientation(Vector2 p, Vector2 q, Vector2 r){
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y); // Positive => Right
        }

        public static bool isClockWise(Vector2[] vertices){
            float sum = 0;

            for(int i = 0; i < vertices.Length; i++){
                Vector2 current = vertices[i];
                Vector2 next = vertices[(i + 1) % vertices.Length];

                sum += (next.x - current.x) * (next.y + current.y);
            }

            return sum > 0;
        }

        public static bool oneWayBitangent(Vector2 point1, Vector2 point2, Vector2 neighbor1, Vector2 neighbor2){
            float o1 = orientation(point1, point2, neighbor1);
            float o2 = orientation(point1, point2, neighbor2);

            return o1 * o2 >= 0;
        }
    }
}
