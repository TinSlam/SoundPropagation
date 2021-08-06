using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class DiffractionSource: SoundSourceBase{
        public List<DiffractionSource> diffractionSources = new List<DiffractionSource>();

        public Obstacle.Vertex sourceVertex;

        public Vector2 startDirection;
        public Vector2 endDirection;

        private Vector2 obstacleStartDirection;
        private Vector2 obstacleEndDirection;

        public Vector2 freeDirection;
        public Vector2 edgeDirection;

        public float aggregatedScale = 1;

        public DiffractionSource parent;

        public List<Vector2> meshCoordinates = new List<Vector2>();

        public DiffractionSource(Obstacle.Vertex sourceVertex, Vector2 position, float amplitude, float range, Vector2 startDirection, Vector2 endDirection, Vector2 obstacleStartDirection, Vector2 obstacleEndDirection, DiffractionSource parent){
            this.sourceVertex = sourceVertex;

            this.position = position;
            this.amplitude = amplitude;
            this.range = range;

            this.startDirection = startDirection;
            this.endDirection = endDirection;

            // Yeah...
            this.obstacleStartDirection = obstacleEndDirection;
            this.obstacleEndDirection = obstacleStartDirection;

            if(startDirection == obstacleStartDirection)
                edgeDirection = startDirection;
            else if(startDirection == obstacleEndDirection)
                edgeDirection = startDirection;
            else if(endDirection == obstacleStartDirection)
                edgeDirection = endDirection;
            else
                edgeDirection = endDirection;

            if(edgeDirection == startDirection)
                freeDirection = endDirection;
            else
                freeDirection = startDirection;

            this.parent = parent;
        }

        public override float computeAmplitude(Vector2 point, SoundModel soundModel){
            if(!Utils.isInsideClockRangeOfTwoVector(startDirection, endDirection, point - position))
                return 0;
            
            float distance = Vector2.getDistance(position, point);

            if(!Utils.isInVision(position, point - position, distance))
                return 0;

            return transferFunction(amplitude, point, this, soundModel);
        }

        public void computeDiffraction(List<DiffractionSource> allDiffractionSources, Dictionary<Vector2, float> uniqueCorners, SoundModel soundModel){
            if(soundModel.properties.modelProperties.diffractionMapPartitioning.value)
                mapPartitionedDiffraction(uniqueCorners, soundModel);
            else
                exhaustiveDiffraction(uniqueCorners, soundModel);

            if(allDiffractionSources != null)
                allDiffractionSources.AddRange(diffractionSources);
        }

        private void exhaustiveDiffraction(Dictionary<Vector2, float> uniqueCorners, SoundModel soundModel){
            findDiffractionCorners(obstacleDepth.obstacle, true, uniqueCorners, soundModel.manager.activeGeometry.obstacleTree, soundModel);

            foreach(Obstacle.ObstacleTreeNode child in obstacleDepth.children){
                Obstacle obstacle = child.obstacle;
                findDiffractionCorners(obstacle, false, uniqueCorners, soundModel.manager.activeGeometry.obstacleTree, soundModel);
            }
        }

        private void mapPartitionedDiffraction(Dictionary<Vector2, float> uniqueCorners, SoundModel soundModel){
            int sourceX = (int) ((position.x - soundModel.manager.activeGeometry.min.x) / soundModel.manager.geometryProperties.mapPartitionSize.value);
            int sourceY = (int) ((position.y - soundModel.manager.activeGeometry.min.y) / soundModel.manager.geometryProperties.mapPartitionSize.value);
            int radius = ((int) Mathf.Ceil(amplitude) - 1) / soundModel.manager.geometryProperties.mapPartitionSize.value + 1;
                
            int xStart = Mathf.Max(0, sourceX - radius);
            int xEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapWidth - 1, sourceX + radius);
            int yStart = Mathf.Max(0, sourceY - radius);
            int yEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapHeight - 1, sourceY + radius);

            for(int i = xStart; i <= xEnd; i++)
                for(int j = yStart; j <= yEnd; j++)
                    foreach(Geometry.PartitionedVertex vertex in soundModel.manager.activeGeometry.partitionedMap[i, j]){
                        if((vertex.obstacleDepth != null && vertex.obstacleDepth.children.Contains(obstacleDepth)) ||
                            (vertex.obstacleDepth == null && soundModel.manager.activeGeometry.obstacleTree.Contains(obstacleDepth)))
                            attemptDiffractionCornerCreation(vertex.obstacle, vertex.obstacleIndex, true, uniqueCorners, soundModel);
                        else if(obstacleDepth == vertex.obstacleDepth)
                            attemptDiffractionCornerCreation(vertex.obstacle, vertex.obstacleIndex, false, uniqueCorners, soundModel);
                    }
        }

        private void findDiffractionCorners(Obstacle obstacle, bool reversed, Dictionary<Vector2, float> uniqueCorners, List<Obstacle.ObstacleTreeNode> obstacleTree, SoundModel soundModel){
            for(int i = 0; i < obstacle.vertices.Length; i++)
                attemptDiffractionCornerCreation(obstacle, i, reversed, uniqueCorners, soundModel);
        }

        public DiffractionSource attemptDiffractionCornerCreation(Obstacle obstacle, int vertexIndex, bool reversed, Dictionary<Vector2, float> uniqueCorners, SoundModel soundModel){
            if(!soundModel.properties.modelProperties.diffractionStaticCaching.value)
                return attemptDiffractionCornerCreationOld(obstacle, vertexIndex, reversed, uniqueCorners, soundModel);
            
            if(reversed && !obstacle.vertices[vertexIndex].isConcave)
                return null;

            if(!reversed && obstacle.vertices[vertexIndex].isConcave)
                return null;

            Vector2 vertex = obstacle.vertices[vertexIndex].position;
            
            Geometry.CachedDiffractionEntry diffractionEntry;
            if(!soundModel.manager.activeGeometry.cachedDiffractionTable[sourceVertex].TryGetValue(obstacle.vertices[vertexIndex], out diffractionEntry))
                return null;
            
            float distance = diffractionEntry.distance;
            Vector2 vector = -diffractionEntry.direction;

            float scaledRange = distance * scaleRange(vertex, soundModel);

            if(distance > scaledRange)
                return null;

            float newAmplitude = transferFunction(amplitude, vertex, this, soundModel);

            float oldAmplitude = 0;
            bool cornerAlreadyExists = soundModel.properties.modelProperties.diffractionRemoveRedundantCorners.value && uniqueCorners.TryGetValue(vertex, out oldAmplitude);
            if(cornerAlreadyExists)
                if(oldAmplitude >= newAmplitude)
                    return null;

            if(!Utils.isInsideClockRangeOfTwoVector(startDirection, endDirection, -vector))
                return null;

            Vector2[] neighbors = {
                obstacle.vertices[vertexIndex].prev.position,
                obstacle.vertices[vertexIndex].next.position
            };
                
            LineSegment2D line = new LineSegment2D(position, vertex);

            if(line.onSameLine(new LineSegment2D(vertex, neighbors[0])))
                if(Vector2.dot(vertex - position, neighbors[0] - vertex) > 0)
                    return null;

            if(line.onSameLine(new LineSegment2D(vertex, neighbors[1])))
                if(Vector2.dot(vertex - position, neighbors[1] - vertex) > 0)
                    return null;

            if(reversed){
                Vector2 temp = neighbors[0];
                neighbors[0] = neighbors[1];
                neighbors[1] = temp;
            }

            Vector2 leftVector = new Vector2(neighbors[0].x - vertex.x, neighbors[0].y - vertex.y);
            Vector2 rightVector = new Vector2(neighbors[1].x - vertex.x, neighbors[1].y - vertex.y);
            Vector2 otherEdge;

            float leftAngle = Mathf.Acos(Vector2.dot(leftVector, vector) / (leftVector.getMagnitude() * vector.getMagnitude())) * Mathf.Rad2Deg;
            float rightAngle = Mathf.Acos(Vector2.dot(rightVector, vector) / (rightVector.getMagnitude() * vector.getMagnitude())) * Mathf.Rad2Deg;

            if(float.IsNaN(leftAngle))
                leftAngle = 0;
            if(float.IsNaN(rightAngle))
                rightAngle = 0;

            float largerAngle = leftAngle > rightAngle ? leftAngle : rightAngle;

            Vector2 start;
            Vector2 end;

            if(largerAngle == leftAngle){
                Vector2 direction = new Vector2(neighbors[0].x - vertex.x, neighbors[0].y - vertex.y).normalize();
                start = direction;
                end = -vector.normalize();
                otherEdge = rightVector.normalize();
            }else{
                Vector2 direction = new Vector2(neighbors[1].x - vertex.x, neighbors[1].y - vertex.y).normalize();
                start = -vector.normalize();
                end = direction;
                otherEdge = leftVector.normalize();
            }

            Vector2 obstacleStart = start;
            Vector2 obstacleEnd = otherEdge;
            
            if((reversed && Utils.getAngleInDegrees(otherEdge, end) >= Utils.getAngleInDegrees(otherEdge, start)) ||
                (!reversed && Utils.getAngleInDegrees(otherEdge, end) < Utils.getAngleInDegrees(otherEdge, start))){
                obstacleStart = otherEdge;
                obstacleEnd = end;
            }

            DiffractionSource corner = new DiffractionSource(obstacle.vertices[vertexIndex], vertex, newAmplitude, calculateRange(newAmplitude, soundModel), start, end, obstacleStart, obstacleEnd, this);
            corner.obstacleDepth = obstacleDepth;
            corner.aggregatedScale *= scaleRange(vertex, soundModel);

            diffractionSources.Add(corner);

            if(soundModel.properties.modelProperties.diffractionRemoveRedundantCorners.value)
                uniqueCorners[vertex] = newAmplitude;

            return corner;
        }

        public DiffractionSource attemptDiffractionCornerCreationOld(Obstacle obstacle, int vertexIndex, bool reversed, Dictionary<Vector2, float> uniqueCorners, SoundModel soundModel){
            if(reversed && !obstacle.vertices[vertexIndex].isConcave)
                return null;

            if(!reversed && obstacle.vertices[vertexIndex].isConcave)
                return null;

            Vector2 vertex = obstacle.vertices[vertexIndex].position;
            
            float scaledRange = range * scaleRange(vertex, soundModel);
            
            float distanceSquared = Vector2.getSquareMagnitude(vertex, position);

            if(distanceSquared > scaledRange * scaledRange)
                return null;

            float newAmplitude = transferFunction(amplitude, vertex, this, soundModel);

            float oldAmplitude = 0;
            bool cornerAlreadyExists = soundModel.properties.modelProperties.diffractionRemoveRedundantCorners.value && uniqueCorners.TryGetValue(vertex, out oldAmplitude);
            if(cornerAlreadyExists)
                if(oldAmplitude >= newAmplitude)
                    return null;

            if(!Utils.isInsideClockRangeOfTwoVector(startDirection, endDirection, vertex - position))
                return null;

            Profiler.diffractionRayTracker++;

            if(!Utils.isInVision(position, vertex))
                return null;

            Vector2[] neighbors = {
                obstacle.vertices[vertexIndex].prev.position,
                obstacle.vertices[vertexIndex].next.position
            };
                
            LineSegment2D line = new LineSegment2D(position, vertex);

            if(line.onSameLine(new LineSegment2D(vertex, neighbors[0])))
                if(Vector2.dot(vertex - position, neighbors[0] - vertex) > 0)
                    return null;

            if(line.onSameLine(new LineSegment2D(vertex, neighbors[1])))
                if(Vector2.dot(vertex - position, neighbors[1] - vertex) > 0)
                    return null;

            if(reversed){
                Vector2 temp = neighbors[0];
                neighbors[0] = neighbors[1];
                neighbors[1] = temp;
            }

            if(!Utils.oneWayBitangent(position, vertex, neighbors[0], neighbors[1]))
                return null;

            Vector2 vector = position - vertex;
            Vector2 leftVector = new Vector2(neighbors[0].x - vertex.x, neighbors[0].y - vertex.y);
            Vector2 rightVector = new Vector2(neighbors[1].x - vertex.x, neighbors[1].y - vertex.y);
            Vector2 otherEdge;

            float leftAngle = Mathf.Acos(Vector2.dot(leftVector, vector) / (leftVector.getMagnitude() * vector.getMagnitude())) * Mathf.Rad2Deg;
            float rightAngle = Mathf.Acos(Vector2.dot(rightVector, vector) / (rightVector.getMagnitude() * vector.getMagnitude())) * Mathf.Rad2Deg;

            if(float.IsNaN(leftAngle))
                leftAngle = 0;
            if(float.IsNaN(rightAngle))
                rightAngle = 0;

            float largerAngle = leftAngle > rightAngle ? leftAngle : rightAngle;

            Vector2 start;
            Vector2 end;

            if(largerAngle == leftAngle){
                Vector2 direction = new Vector2(neighbors[0].x - vertex.x, neighbors[0].y - vertex.y).normalize();
                start = direction;
                end = -vector.normalize();
                otherEdge = rightVector.normalize();
            }else{
                Vector2 direction = new Vector2(neighbors[1].x - vertex.x, neighbors[1].y - vertex.y).normalize();
                start = -vector.normalize();
                end = direction;
                otherEdge = leftVector.normalize();
            }

            Vector2 obstacleStart = start;
            Vector2 obstacleEnd = otherEdge;
            
            if((reversed && Utils.getAngleInDegrees(otherEdge, end) >= Utils.getAngleInDegrees(otherEdge, start)) ||
                (!reversed && Utils.getAngleInDegrees(otherEdge, end) < Utils.getAngleInDegrees(otherEdge, start))){
                obstacleStart = otherEdge;
                obstacleEnd = end;
            }

            DiffractionSource corner = new DiffractionSource(obstacle.vertices[vertexIndex], vertex, newAmplitude, calculateRange(newAmplitude, soundModel), start, end, obstacleStart, obstacleEnd, this);
            corner.obstacleDepth = obstacleDepth;
            corner.aggregatedScale *= scaleRange(vertex, soundModel);

            diffractionSources.Add(corner);

            if(soundModel.properties.modelProperties.diffractionRemoveRedundantCorners.value)
                uniqueCorners[vertex] = newAmplitude;

            return corner;
        }

        public void drawMesh(SoundModel soundModel){
            findMeshCoordinates(soundModel);
            sortMeshCoordinates(soundModel);

            soundModel.debugger.drawSoundMeshDiffraction(meshCoordinates, position, amplitude, soundModel.source.amplitude, 0.1f, this);

            foreach(DiffractionSource source in diffractionSources)
                source.drawMesh(soundModel);
        }

        private Vector2 findFreeDirection(SoundModel soundModel){
            Vector2 freeDirection = new Vector2(0, 0);
            Vector2 other = new Vector2(0, 0);
            float edgeSize = 0;
            float maxDot = float.MinValue;

            foreach(Edge edge in soundModel.manager.activeGeometry.vertexEdgeMap[position]){
                Vector2 otherPoint;
                
                if(edge.point1 != position)
                    otherPoint = edge.point1;
                else
                    otherPoint = edge.point2;

                Vector2 direction = (otherPoint - position) / edge.size;

                {
                    float dot = Vector2.dot(direction, startDirection);
                    if(dot > maxDot){
                        maxDot = dot;
                        freeDirection = endDirection;
                        edgeSize = edge.size;
                        other = otherPoint;
                    }
                }

                {
                    float dot = Vector2.dot(direction, endDirection);
                    if(dot > maxDot){
                        maxDot = dot;
                        freeDirection = startDirection;
                        edgeSize = edge.size;
                        other = otherPoint;
                    }
                }
            }

            castRay(freeDirection, soundModel);

            Vector2 edgeDirection = freeDirection == startDirection ? endDirection : startDirection;
            
            float scaledRange = range * scaleRange(position + edgeDirection * range, soundModel);

            if(edgeSize > scaledRange)
                meshCoordinates.Add(position + edgeDirection * scaledRange);
            else{
                if(soundModel.manager.activeGeometry.findObstacleDepth(position + edgeDirection * (edgeSize + 0.01f)) != obstacleDepth)
                    meshCoordinates.Add(other);
                else{
                    int offset;
                    RaycastHit2D[] hits = Utils.raycastAll(position + edgeDirection * edgeSize, edgeDirection, scaledRange - edgeSize, out offset);
                
                    if(hits.Length == offset)
                        meshCoordinates.Add(position + edgeDirection * scaledRange);
                    else
                        meshCoordinates.Add(new Vector2(hits[offset].point));
                }
            }

            return edgeDirection;
        }

        public void findMeshCoordinates(SoundModel soundModel){
            Vector2 edgeDirection = findFreeDirection(soundModel);
            
            int sourceX = (int) ((position.x - soundModel.manager.activeGeometry.min.x) / soundModel.manager.geometryProperties.mapPartitionSize.value);
            int sourceY = (int) ((position.y - soundModel.manager.activeGeometry.min.y) / soundModel.manager.geometryProperties.mapPartitionSize.value);
            int radius = ((int) Mathf.Ceil(range) - 1) / soundModel.manager.geometryProperties.mapPartitionSize.value + 1;
                
            int xStart = Mathf.Max(0, sourceX - radius);
            int xEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapWidth - 1, sourceX + radius);
            int yStart = Mathf.Max(0, sourceY - radius);
            int yEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapHeight - 1, sourceY + radius);

            for(int i = xStart; i <= xEnd; i++)
                for(int j = yStart; j <= yEnd; j++)
                    foreach(Geometry.PartitionedVertex partitionedVertex in soundModel.manager.activeGeometry.partitionedMap[i, j])
                        if(Vector2.getAngleBetweenVectors(partitionedVertex.vertex - position, edgeDirection) > 0.001f)
                            castRay(partitionedVertex.vertex, partitionedVertex.vertex - position, soundModel);

            float degrees = 10;

            float startAngle = Utils.getAngleInDegrees(startDirection);
            float endAngle = Utils.getAngleInDegrees(endDirection);

            while(endAngle < 0)
                endAngle += 360;

            while(startAngle < 0)
                startAngle += 360;

            if(endAngle > startAngle)
                startAngle += 360;

            int start = (int) ((endAngle / degrees + 1) * degrees);
            int end = (int) ((startAngle / degrees - 1) * degrees);

            for(float i = start; i <= end; i += degrees){
                float radians = i * Mathf.Deg2Rad;
                castRay(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)), soundModel);
            }
        }

        private void castRay(Vector2 direction, SoundModel soundModel){
            float scaledRange = range * scaleRange(position + direction, soundModel);

            int offset;
            RaycastHit2D[] hits = Utils.raycastAll(position, direction, scaledRange, out offset);

            for(int i = offset; i < hits.Length; i++){
                RaycastHit2D hit = hits[i];
                Vector2 point = new Vector2(hit.point);

                Edge[] edges;
                bool exists = soundModel.manager.activeGeometry.vertexEdgeMap.TryGetValue(point, out edges);

                if(!exists){
                    meshCoordinates.Add(point);
                    return;
                }else{
                    Vector2[] neighbors = Utils.findNeighbors(point, edges);
                    
                    if(!Utils.oneWayBitangent(position, point, neighbors[0], neighbors[1])){
                        meshCoordinates.Add(point);
                        return;
                    }
                }
            }

            meshCoordinates.Add(position + direction.normalize() * scaledRange);
        }

        private void castRay(Vector2 vertex, Vector2 direction, SoundModel soundModel){
            if(!Utils.isInsideClockRangeOfTwoVector(startDirection, endDirection, direction))
                return;
            
            float scaledRange = range * scaleRange(vertex, soundModel);

            float distance = Vector2.getDistance(position, vertex);
            bool instanceAdded = scaledRange <= distance;

            int offset;
            RaycastHit2D[] hits = Utils.raycastAll(position, direction, scaledRange, out offset);

            for(int i = offset; i < hits.Length; i++){
                RaycastHit2D hit = hits[i];
                Vector2 point = new Vector2(hit.point);

                if(!instanceAdded && distance < hit.distance){
                    instanceAdded = true;
                    meshCoordinates.Add(vertex);
                }

                Edge[] edges;
                bool exists = soundModel.manager.activeGeometry.vertexEdgeMap.TryGetValue(point, out edges);

                if(!exists)
                    if(Vector2.getSquareMagnitude(point, vertex) < 0.01f){
                        edges = soundModel.manager.activeGeometry.vertexEdgeMap[vertex];

                        Vector2[] neighbors = Utils.findNeighbors(vertex, edges);
                    
                        if(!Utils.oneWayBitangent(position, vertex, neighbors[0], neighbors[1])){
                            meshCoordinates.Add(vertex);
                            return;
                        }
                    }else{
                        meshCoordinates.Add(point);
                        return;
                    }
                else{
                    Vector2[] neighbors = Utils.findNeighbors(point, edges);
                    
                    if(!Utils.oneWayBitangent(position, point, neighbors[0], neighbors[1])){
                        meshCoordinates.Add(point);
                        return;
                    }
                }
            }

            if(!instanceAdded)
                meshCoordinates.Add(vertex);

            meshCoordinates.Add(position + direction.normalize() * scaledRange);
        }

        public void sortMeshCoordinates(SoundModel soundModel){
            meshCoordinates.Sort((vector2, vector1) => {
                if(vector1 == vector2)
                    return 0;

                if(vector1 == position)
                    return 1;
                else if(vector2 == position)
                    return -1;

                Vector2 v1 = vector1 - position;
                Vector2 v2 = vector2 - position;

                float angle1;
                float angle2;
            
                angle1 = Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;
                angle2 = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;

                if(float.IsNaN(angle1))
                    angle1 = position.y > v1.y ? 270 : 90;

                if(float.IsNaN(angle2))
                    angle2 = position.y > v2.y ? 270 : 90;

                if(angle1 < 0)
                    angle1 += 360;
                if(angle2 < 0)
                    angle2 += 360;

                if(Mathf.Abs(angle1 - angle2) > 0.005f)
                    if(angle1 < angle2)
                        return 1;
                    else if(angle1 > angle2)
                        return -1;

                if(Utils.floatEqual(angle1, 0f)){
                    if(Utils.floatEqual(angle2, 0f)){
                        angle1 = angle2;
                    }else if(Utils.floatEqual(angle2, 360))
                        angle1 = angle2;
                }

                if(Utils.floatEqual(angle2, 0f)){
                    if(Utils.floatEqual(angle1, 0f)){
                        angle1 = angle2;
                    }else if(Utils.floatEqual(angle1, 360))
                        angle1 = angle2;
                }

                return angle1 > angle2 ? 1 : -1;
            });

            for(int i = 0; i < meshCoordinates.Count; i++){
                Vector2 current = meshCoordinates[i];
                Vector2 next = meshCoordinates[(i + 1) % meshCoordinates.Count];

                float angleCurrent = Mathf.Atan2(current.y - position.y, current.x - position.x) * Mathf.Rad2Deg;
                float angleNext = Mathf.Atan2(next.y - position.y, next.x - position.x) * Mathf.Rad2Deg;

                if(float.IsNaN(angleCurrent))
                    angleCurrent = position.y > current.y ? 270 : 90;

                if(float.IsNaN(angleNext))
                    angleNext = position.y > next.y ? 270 : 90;

                if(angleCurrent < 0)
                    angleCurrent += 360;
                if(angleNext < 0)
                    angleNext += 360;

                if(Mathf.Abs(angleCurrent - angleNext) > 0.05f)
                    continue;

                Vector2 currentNeighbor = meshCoordinates[(i - 1 + meshCoordinates.Count) % meshCoordinates.Count];
                Vector2 nextNeighbor = meshCoordinates[(i + 2) % meshCoordinates.Count];

                Vector2 closest = current;
                Vector2 furthest = next;
                Vector2 closestNeighbor = currentNeighbor;
                Vector2 furthestNeighbor = nextNeighbor;

                if(Vector2.getSquareMagnitude(closest, position) > Vector2.getSquareMagnitude(furthest, position)){
                    closest = next;
                    furthest = current;
                    closestNeighbor = nextNeighbor;
                    furthestNeighbor = currentNeighbor;
                }

                Edge[] edges;
                if(!soundModel.manager.activeGeometry.vertexEdgeMap.TryGetValue(closest, out edges))
                    continue;

                foreach(Edge edge in edges){
                    float close;
                    float far;

                    edge.edge.findClosestPointsOnLine(closestNeighbor, out close);
                    edge.edge.findClosestPointsOnLine(furthestNeighbor, out far);

                    if(close < 0.01f)
                        break;
                    else if(far < 0.01f){
                        meshCoordinates[i] = next;
                        meshCoordinates[(i + 1) % meshCoordinates.Count] = current;
                        break;
                    }
                }
            }
        }

        public float scaleRange(Vector2 destination, SoundModel soundModel){
            if(soundModel.properties.modelProperties.decayingDiffraction.value)
                return (1 - Vector2.getAngleBetweenVectors(freeDirection, destination - position) / 180) * aggregatedScale;

            return 1;
        }
    }
}
