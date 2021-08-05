using UnityEngine;

namespace SoundPropagation{
    public class LineSegment2D{
        private float x1, y1;
        private float x2, y2;
        private double slope = 0;
        private double yIntercept = 0;
        private bool vertical = false;

        private Vector2 mostRecentIntersectionPoint = new Vector2();

        private const float lineIntersectionEqualFloatsDistance = 0;
        private const float lineIntersectionAcceptableDistanceForIntersection = 0.1f;
        private const float lineIntersectionAcceptableValueBetweenError = 0.15f;

        public LineSegment2D(Vector2 point1, Vector2 point2){
            initialize(point1.x, point1.y, point2.x, point2.y);
        }

        public LineSegment2D(float x1, float y1, float x2, float y2){
            initialize(x1, y1, x2, y2);
        }

        private void initialize(float x1, float y1, float x2, float y2){
            float tempX = x1;
            float tempY = y1;
            if(x1 > x2){
                x1 = x2;
                y1 = y2;
                x2 = tempX;
                y2 = tempY;
            }

            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;

            if(x1 == x2)
                vertical = true;
            else{
                slope = (y1 - y2) / (x1 - x2);
                yIntercept = y1 - slope * x1;
            }
        }

        public bool intersectsSegment(LineSegment2D line){
            float x, y;

            if(floatEqual(x1, line.x1) && floatEqual(y1, line.y1)){
                x = x1;
                y = y1;
                mostRecentIntersectionPoint = new Vector2(x, y);
                return true;
            }else if(floatEqual(x1, line.x2) && floatEqual(y1, line.y2)){
                x = x1;
                y = y1;
                mostRecentIntersectionPoint = new Vector2(x, y);
                return true;
            }else if(floatEqual(x2, line.x1) && floatEqual(y2, line.y1)){
                x = x2;
                y = y2;
                mostRecentIntersectionPoint = new Vector2(x, y);
                return true;
            }else if(floatEqual(x2, line.x2) && floatEqual(y2, line.y2)){
                x = x2;
                y = y2;
                mostRecentIntersectionPoint = new Vector2(x, y);
                return true;
            }

            if(line.vertical && vertical)
                if(line.x1 == x1){
                    x = x1;
                    y = y1;
                }else
                    return false;
            else if(line.vertical || vertical){
                if(line.vertical){
                    x = line.x1;
                    y = getY(x);
                }else{
                    x = x1;
                    y = line.getY(x);
                }
            }else{
                if(onSameLine(line)){
                    if((x1 <= line.x1 && line.x1 <= x2) || (line.x1 <= x1 && x1 <= line.x2)){
                        x = x1;
                        y = y1;
                        mostRecentIntersectionPoint = new Vector2(x, y);
                        return true;
                    }else
                        return false;
                }else if(line.slope == slope)
                    return false;

                x = (float) ((yIntercept - line.yIntercept) / (line.slope - slope));
                y = getY(x);
            }

            mostRecentIntersectionPoint = new Vector2(x, y);

            if(distanceFromPoint(x, y) < lineIntersectionAcceptableDistanceForIntersection &&
                distanceFromPoint(x, y) < lineIntersectionAcceptableDistanceForIntersection &&
                valueBetween(x, x1, x2) && valueBetween(x, line.x1, line.x2) && valueBetween(y, y1, y2) && valueBetween(y, line.y1, line.y2))
                return true;
            else
                return false;
        }

        private bool floatEqual(float value1, float value2){
            return Mathf.Abs(value1 - value2) < lineIntersectionEqualFloatsDistance;
        }

        public float distanceFromPoint(float x, float y){
            if(vertical)
                return Mathf.Abs(x1 - x);

            return Mathf.Abs((float) (slope * x - y + yIntercept)) / Mathf.Sqrt((float) (slope * slope + 1));
        }

        private bool valueBetween(float value, float first, float second){
            float epsilon = lineIntersectionAcceptableValueBetweenError;

            if(Mathf.Abs(value - first) < epsilon || Mathf.Abs(value - second) < epsilon)
                return true;

            float greater = first;
            float lower = second;

            if(greater < lower){
                greater = second;
                lower = first;
            }

            return value - lower > -epsilon && value - greater < epsilon;
        }

        public bool hasPoint(float x, float y){
            return new Vector2(x, y) == new Vector2(x1, y1) || new Vector2(x, y) == new Vector2(x2, y2);
        }

        public bool onSameLine(LineSegment2D line){
            if(vertical && line.vertical)
                if(x1 == line.x1)
                    return true;
                else
                    return false;

            if(Mathf.Abs((float) (yIntercept - line.yIntercept)) > 0.1f)
                return false;

            Vector2 first = new Vector2(x2, y2) - new Vector2(x1, y1);
            Vector2 second = new Vector2(line.x2, line.y2) - new Vector2(line.x1, line.y1);

            float angle = Mathf.Acos(Vector2.dot(first, second) / (first.getMagnitude() * second.getMagnitude())) * Mathf.Rad2Deg;

            if(float.IsNaN(angle))
                return true;

            return angle < 1;
        }

        public Vector2 findClosestPointsOnLine(Vector2 point, out float distance){
            distance = distanceFromPoint(point.x, point.y);

            if(distance == 0)
                return point;

            Vector2 point1 = new Vector2(x1, y1);
            Vector2 point2 = new Vector2(x2, y2);

            Vector2 temp = (point2 - point1).normalize();
            Vector2 normal = new Vector2(-temp.y, temp.x);
            if(Vector2.dot(normal, point - point1) < 0)
                normal = -normal;

            Vector2 pointOnLine = point + -normal * distance;

            if(Vector2.dot(point2 - pointOnLine, point1 - pointOnLine) > 0){
                float distance1 = Vector2.getSquareMagnitude(point1, point);
                float distance2 = Vector2.getSquareMagnitude(point2, point);

                if(distance1 < distance2){
                    distance = Mathf.Sqrt(distance1);
                    return point1;
                }else{
                    distance = Mathf.Sqrt(distance2);
                    return point2;
                }
            }

            return pointOnLine;
        }

        public float substituteInEquation(float x, float y){
            if(vertical)
                return x - x1;
            return (float) (slope * x + yIntercept - y);
        }

        public float getY(float x){
            if(vertical)
                return y1;
            return (float) (slope * x + yIntercept);
        }

        public bool isVertical(){
            return vertical;
        }

        public double getSlope(){
            return slope;
        }

        public Vector2 getMostRecentIntersectionPoint(){
            return mostRecentIntersectionPoint;
        }

        public Vector2 getLeftPoint(){
            return new Vector2(x1, y1);
        }

        public Vector2 getRightPoint(){
            return new Vector2(x2, y2);
        }
    }
}
