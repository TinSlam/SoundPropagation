using UnityEngine;

namespace SoundPropagation{
    [System.Serializable]
    public struct Vector2{
        public float x;
        public float y;

        public Vector2(float x, float y){
            this.x = x;
            this.y = y;
        }

        public Vector2(UnityEngine.Vector2 vector){
            x = vector.x;
            y = vector.y;
        }

        public float getMagnitude(){
            return Mathf.Sqrt(x * x + y * y);
        }

        public Vector2 normalize(){
            float magnitude = getMagnitude();
            return magnitude == 0 ? this : new Vector2(x / magnitude, y / magnitude);
        }

        public Vector2 findNormal(){
            return new Vector2(-y, x);
        }

        public static Vector2 operator+(Vector2 vector1, Vector2 vector2){
            return new Vector2(vector1.x + vector2.x, vector1.y + vector2.y);
        }

        public static Vector2 operator-(Vector2 vector1, Vector2 vector2){
            return new Vector2(vector1.x - vector2.x, vector1.y - vector2.y);
        }

        public static Vector2 operator-(Vector2 vector){
            return new Vector2(-vector.x, -vector.y);
        }

        public static Vector2 operator*(Vector2 vector, float value){
            return new Vector2(vector.x * value, vector.y * value);
        }
        
        public static Vector2 operator*(float value, Vector2 vector){
            return new Vector2(vector.x * value, vector.y * value);
        }

        public static Vector2 operator/(Vector2 vector, float value){
            return new Vector2(vector.x / value, vector.y / value);
        }

        public static bool operator==(Vector2 vector1, Vector2 vector2){
            return vector1.x == vector2.x && vector1.y == vector2.y;
        }
        
        public static bool operator!=(Vector2 vector1, Vector2 vector2){
            return vector1.x != vector2.x || vector1.y != vector2.y;
        }

        public override bool Equals(object obj){
            return this == (Vector2) obj;
        }

        public bool Equals(UnityEngine.Vector2 vector){
            return x == vector.x && y == vector.y;
        }

        public override int GetHashCode() {
            unchecked{
                int hash = (int) 2166136261;
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                return hash;
            }
        }

        public override string ToString(){
            return x + " " + y;
        }

        public static float dot(Vector2 vector1, Vector2 vector2){
            return vector1.x * vector2.x + vector1.y * vector2.y;
        }

        public static float getAngleBetweenVectors(Vector2 vector1, Vector2 vector2){
            float dotProduct = dot(vector1, vector2) / (vector1.getMagnitude() * vector2.getMagnitude());
            
            if(dotProduct > 1)
                dotProduct = 1;
            if(dotProduct < -1)
                dotProduct = -1;

            return Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
        }

        public static float getDistance(Vector2 vector1, Vector2 vector2){
            float dx = vector1.x - vector2.x;
            float dy = vector1.y - vector2.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static float getSquareMagnitude(Vector2 vector1, Vector2 vector2){
            float dx = vector1.x - vector2.x;
            float dy = vector1.y - vector2.y;
            return dx * dx + dy * dy;
        }

        public static Vector2 convert(Vector3 vector){
            return new Vector2(vector.x, vector.z);
        }

        public static Vector2[] convert(UnityEngine.Vector2[] vectors){
            Vector2[] newVectors = new Vector2[vectors.Length];
            
            for(int i = 0; i < vectors.Length; i++)
                newVectors[i] = convert(vectors[i]);
            
            return newVectors;
        }

        public static Vector3 convert(Vector2 vector){
            return new Vector3(vector.x, vector.y);
        }
    }
}
