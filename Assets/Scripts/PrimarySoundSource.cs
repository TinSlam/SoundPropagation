using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class PrimarySoundSource: SoundSourceBase {
        //public List<DiffractionSource> allDiffractionSources = new List<DiffractionSource>();

        //public List<DiffractionSource> diffractionSources = new List<DiffractionSource>();
        //public List<ReflectionSource> reflectionSources = new List<ReflectionSource>();
        //public List<TransmissionSource> transmissionSources = new List<TransmissionSource>();

        //private Dictionary<Vector2, float> uniqueCorners = new Dictionary<Vector2, float>();

        //public List<Vector2> meshCoordinates = new List<Vector2>();
        //public GameObject meshObject;

        //public SoundModel soundModel;

        //public SoundModelMethod soundModelMethod;

        //public PrimarySoundSource(SoundModel soundModel, SoundModelMethod soundModelMethod){
        //    this.soundModel = soundModel;
        //    this.soundModelMethod = soundModelMethod;

        //    meshObject = RenderingUtils.createMeshObject("SoundMesh", ResourceManager.soundModelMaterial);
        //    meshObject.transform.parent = soundModel.debugger.soundMeshes.transform;
        //}

        //public void computeModel(){
        //    cleanUp();
        //    initValues();

        //    Timer modelComputationTimer = new Timer();

        //    soundModel.totalModelComputationTime.resume();
        //    modelComputationTimer.start();

        //    obstacleDepth = soundModel.manager.activeGeometry.findObstacleDepth(position);

        //    soundModelMethod.computeModel(this);

        //    modelComputationTimer.stop();
        //    soundModel.totalModelComputationTime.pause();

        //    soundModel.averageModelComputationTime.addNewValue(modelComputationTimer.getTimeElapsed());

        //    soundModelMethod.visualizeModel(this);
        //}

        //public void visualizePrimarySource(){
        //    if(soundModel.properties.debugProperties.visualize.value && soundModel.properties.debugProperties.visualizePrimarySource.value){
        //        findMeshCoordinates();
        //        sortMeshCoordinates();

        //        //for(int i = 0; i < meshCoordinates.Count; i++)
        //            //soundModel.debugger.drawPoint(meshCoordinates[i], 0.4f/* + i * 0.2f*/, 0.4f, ResourceManager.materialColorMagenta);

        //        meshObject.SetActive(true);
        //        RenderingUtils.drawSoundMesh(meshCoordinates, position, amplitude, amplitude, 0.1f, soundModel, meshObject);
        //    }else
        //        meshObject.SetActive(false);
        //}

        //public void visualizeDiffraction(){
        //    if(soundModel.properties.debugProperties.visualize.value && soundModel.properties.debugProperties.visualizeDiffraction.value)
        //        foreach(DiffractionSource source in diffractionSources)
        //            source.drawMesh(soundModel);
        //}

        //public void visualizeTransmission(){
        //    if(soundModel.properties.debugProperties.visualize.value && soundModel.properties.debugProperties.visualizeTransmission.value)
        //        foreach(TransmissionSource source in transmissionSources)
        //            source.drawMesh(soundModel);
        //}

        //public void computeTransmission(){
        //    int rayCount = soundModel.properties.modelProperties.transmissionRayCount.value;
        //    //rayCount = 1;
        //    float gap = Mathf.PI * 2 / rayCount;

        //    for(float angle = 0; angle < 2 * Mathf.PI; angle += gap){
        //        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        //        float distance = amplitude;

        //        int offset;
        //        RaycastHit2D[] hits = Utils.raycastAll(position, direction, distance, out offset);

        //        //soundModel.debugger.drawLine(position, position + direction * amplitude, 0.2f, 1, ResourceManager.materialColorMagenta);

        //        float remainingAmplitude = amplitude;
        //        float distanceTravelled = 0;

        //        for(int i = offset; i < hits.Length; i++){
        //            RaycastHit2D hit = hits[i];

        //            float transmissionRatio = hit.collider.gameObject.GetComponent<Edge>().material.transmissionRatio;

        //            if(soundModel.properties.debugProperties.useGlobalTransmissionRatio.value)
        //                transmissionRatio = soundModel.properties.debugProperties.globalTransmissionRatio.value;

        //            float segmentDistance = hit.distance - distanceTravelled;

        //            //remainingAmplitude = Mathf.Max(remainingAmplitude - segmentDistance, AudioManager.computeDiffractionVolume(new Vector2(hit.point) - 0.1f * direction, this, new List<AudioManager.DirectedSound>()) - 0.1f);
        //            remainingAmplitude = remainingAmplitude - segmentDistance;
        //            remainingAmplitude *= transmissionRatio;

        //            if(remainingAmplitude <= 0.1f)
        //                continue;

        //            distanceTravelled = hit.distance;

        //            Obstacle.ObstacleDepth depthNode = soundModel.manager.activeGeometry.findObstacleDepth(new Vector2(hit.point) + 0.1f * direction);
        //            //Obstacle.ObstacleDepth depthNode = obstacleDepth;

        //            TransmissionSource transmissionSource = new TransmissionSource(new Vector2(hit.point), remainingAmplitude, depthNode);
        //            //soundModel.debugger.drawCircle(new Vector2(hit.point), remainingAmplitude, 0.2f, 1, ResourceManager.materialColorYellow);
        //            //soundModel.debugger.drawPoint(new Vector2(hit.point), 1, 1, ResourceManager.materialColorBlue);

        //            transmissionSources.Add(transmissionSource);
        //        }
        //    }
        //}

        //public void computeDiffractionSources(){
        //    if(obstacleDepth == null)
        //        return;

        //    if(soundModel.properties.modelProperties.diffractionGreedySearch.value){
        //        greedyDiffraction();
        //    }else
        //        dfsDiffraction();

        //    allDiffractionSources.AddRange(diffractionSources);

        //    Profiler.diffractionSourceTracker = allDiffractionSources.Count;
        //}

        //private void greedyDiffraction(){
        //    computeDiffraction();

        //    MinHeap sourcesHeap = new MinHeap();

        //    foreach(DiffractionSource source in diffractionSources)
        //        sourcesHeap.Add(new VertexNode(source, -source.amplitude));

        //    while(!sourcesHeap.IsEmpty()){
        //        DiffractionSource source = sourcesHeap.Pop().source;
        //        allDiffractionSources.Add(source);

        //        source.computeDiffraction(null, uniqueCorners, soundModel);

        //        foreach(DiffractionSource diffractionSource in source.diffractionSources)
        //            sourcesHeap.Add(new VertexNode(diffractionSource, -diffractionSource.amplitude));
        //    }
        //}

        //private void dfsDiffraction(){
        //    computeDiffraction();

        //    foreach(DiffractionSource source in diffractionSources){
        //        source.computeDiffraction(allDiffractionSources, uniqueCorners, soundModel);
        //        dfsRecurse(source);
        //    }
        //}

        //private void dfsRecurse(DiffractionSource diffractionSource){
        //    foreach(DiffractionSource source in diffractionSource.diffractionSources){
        //        source.computeDiffraction(allDiffractionSources, uniqueCorners, soundModel);
        //        dfsRecurse(source);
        //    }
        //}

        //private void computeDiffraction(){
        //    if(soundModel.properties.modelProperties.diffractionMapPartitioning.value)
        //        mapPartitionedDiffraction();
        //    else
        //        exhaustiveDiffraction();
        //}

        //private void exhaustiveDiffraction(){
        //    findDiffractionCorners(obstacleDepth.obstacle, true, uniqueCorners, soundModel.manager.activeGeometry.obstacleTree);

        //    foreach(Obstacle.ObstacleDepth child in obstacleDepth.children){
        //        Obstacle obstacle = child.obstacle;
        //        findDiffractionCorners(obstacle, false, uniqueCorners, soundModel.manager.activeGeometry.obstacleTree);
        //    }
        //}

        //private void mapPartitionedDiffraction(){
        //    int sourceX = (int) ((position.x - soundModel.manager.activeGeometry.min.x) / soundModel.manager.geometryProperties.mapPartitionSize.value);
        //    int sourceY = (int) ((position.y - soundModel.manager.activeGeometry.min.y) / soundModel.manager.geometryProperties.mapPartitionSize.value);
        //    int radius = ((int) Mathf.Ceil(range) - 1) / soundModel.manager.geometryProperties.mapPartitionSize.value + 1;

        //    int xStart = Mathf.Max(0, sourceX - radius);
        //    int xEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapWidth - 1, sourceX + radius);
        //    int yStart = Mathf.Max(0, sourceY - radius);
        //    int yEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapHeight - 1, sourceY + radius);

        //    for(int i = xStart; i <= xEnd; i++)
        //        for(int j = yStart; j <= yEnd; j++)
        //            foreach(Geometry.PartitionedVertex vertex in soundModel.manager.activeGeometry.partitionedMap[i, j]){
        //                if((vertex.obstacleDepth != null && vertex.obstacleDepth.children.Contains(obstacleDepth)) ||
        //                    (vertex.obstacleDepth == null && soundModel.manager.activeGeometry.obstacleTree.Contains(obstacleDepth)))
        //                    attemptDiffractionCornerCreation(vertex.obstacle, vertex.obstacleIndex, true, uniqueCorners, soundModel.manager.activeGeometry.obstacleTree);
        //                else if(obstacleDepth == vertex.obstacleDepth)
        //                    attemptDiffractionCornerCreation(vertex.obstacle, vertex.obstacleIndex, false, uniqueCorners, soundModel.manager.activeGeometry.obstacleTree);
        //            }
        //}

        //private void findDiffractionCorners(Obstacle obstacle, bool reversed, Dictionary<Vector2, float> uniqueCorners, List<Obstacle.ObstacleDepth> obstacleTree){
        //    for(int i = 0; i < obstacle.vertices.Length; i++)
        //        attemptDiffractionCornerCreation(obstacle, i, reversed, uniqueCorners, obstacleTree);
        //}

        //public DiffractionSource attemptDiffractionCornerCreation(Obstacle obstacle, int vertexIndex, bool reversed, Dictionary<Vector2, float> uniqueCorners, List<Obstacle.ObstacleDepth> obstacleTree){
        //    if(reversed && !obstacle.vertices[vertexIndex].isConcave)
        //        return null;

        //    if(!reversed && obstacle.vertices[vertexIndex].isConcave)
        //        return null;

        //    Vector2 vertex = obstacle.vertices[vertexIndex].position;
        //    float distanceSquared = Vector2.getSquareMagnitude(vertex, position);

        //    if(distanceSquared > range * range)
        //        return null;

        //    float newAmplitude = transferFunction(amplitude, distanceSquared, soundModel);

        //    //float oldAmplitude = 0;
        //    //bool cornerAlreadyExists = soundModel.properties.modelProperties.diffractionRemoveRedundantCorners.value && uniqueCorners.TryGetValue(vertex, out oldAmplitude);
        //    //if(cornerAlreadyExists)
        //    //    if(oldAmplitude >= newAmplitude)
        //    //        return null;

        //    Profiler.diffractionRayTracker++;

        //    if(!Utils.isInVision(position, vertex))
        //        return null;

        //    Vector2[] neighbors = {
        //        obstacle.vertices[vertexIndex].prev.position,
        //        obstacle.vertices[vertexIndex].next.position
        //    };

        //    if(reversed){
        //        Vector2 temp = neighbors[0];
        //        neighbors[0] = neighbors[1];
        //        neighbors[1] = temp;
        //    }

        //    if(!MathUtils.oneWayBitangent(position, vertex, neighbors[0], neighbors[1]))
        //        return null;

        //    Vector2 vector = position - vertex;
        //    Vector2 leftVector = new Vector2(neighbors[0].x - vertex.x, neighbors[0].y - vertex.y);
        //    Vector2 rightVector = new Vector2(neighbors[1].x - vertex.x, neighbors[1].y - vertex.y);
        //    Vector2 otherEdge;

        //    float leftAngle = Mathf.Acos(Vector2.dot(leftVector, vector) / (leftVector.getMagnitude() * vector.getMagnitude())) * Mathf.Rad2Deg;
        //    float rightAngle = Mathf.Acos(Vector2.dot(rightVector, vector) / (rightVector.getMagnitude() * vector.getMagnitude())) * Mathf.Rad2Deg;

        //    if(float.IsNaN(leftAngle))
        //        leftAngle = 0;
        //    if(float.IsNaN(rightAngle))
        //        rightAngle = 0;

        //    float largerAngle = leftAngle > rightAngle ? leftAngle : rightAngle;

        //    Vector2 start;
        //    Vector2 end;

        //    if(largerAngle == leftAngle){
        //        Vector2 direction = new Vector2(neighbors[0].x - vertex.x, neighbors[0].y - vertex.y).normalize();
        //        start = direction;
        //        end = -vector.normalize();
        //        otherEdge = rightVector.normalize();
        //    }else{
        //        Vector2 direction = new Vector2(neighbors[1].x - vertex.x, neighbors[1].y - vertex.y).normalize();
        //        start = -vector.normalize();
        //        end = direction;
        //        otherEdge = leftVector.normalize();
        //    }

        //    Vector2 obstacleStart = start;
        //    Vector2 obstacleEnd = otherEdge;

        //    if((reversed && Utils.getAngleInDegrees(otherEdge, end) >= Utils.getAngleInDegrees(otherEdge, start)) ||
        //        (!reversed && Utils.getAngleInDegrees(otherEdge, end) < Utils.getAngleInDegrees(otherEdge, start))){
        //        obstacleStart = otherEdge;
        //        obstacleEnd = end;
        //    }

        //    DiffractionSource corner = new DiffractionSource(obstacle.vertices[vertexIndex], vertex, newAmplitude, calculateRange(newAmplitude, soundModel), start, end, obstacleStart, obstacleEnd, null);
        //    corner.obstacleDepth = obstacleDepth;

        //    diffractionSources.Add(corner);

        //    if(soundModel.properties.modelProperties.diffractionRemoveRedundantCorners.value){
        //        uniqueCorners[vertex] = newAmplitude;
        //        //if(cornerAlreadyExists)
        //            //TestingAndProfiling.diffractionCornerCount--;
        //    }

        //    return corner;
        //}

        //public void computeReflection(){
        //    int samples = soundModel.properties.modelProperties.reflectionRayCount.value;
        //    int bounceLimit = soundModel.properties.modelProperties.reflectionBounceLimit.value;

        //    for(int i = 0; i < samples; i++) {
        //        float radians = i * 360f / samples * Mathf.Deg2Rad;
        //        castReflectionRay(new UnityEngine.Vector2(position.x, position.y), new UnityEngine.Vector2(Mathf.Cos(radians), Mathf.Sin(radians)), amplitude, bounceLimit);
        //    }
        //}

        //public void castReflectionRay(UnityEngine.Vector2 initialPosition, UnityEngine.Vector2 initialDirection, float initialRange, int bounceLimit) {
        //    if(initialRange < soundModel.properties.modelProperties.reflectionMinimumRangeLimit.value)
        //        return;

        //    UnityEngine.Vector2 currentPosition = initialPosition;
        //    UnityEngine.Vector2 currentDirection = initialDirection.normalized;
        //    float currentRange = initialRange;

        //    int bounceCounter = 0;

        //    float distanceTravelled = 0;

        //    RaycastHit2D hit;
        //    do{
        //        Profiler.reflectionRayTracker++;
        //        hit = Physics2D.Raycast(currentPosition, currentDirection, currentRange, Utils.soundModelLayer);

        //        if(hit.collider == null)
        //            break;

        //        //soundModel.debugger.drawLine(new Vector2(currentPosition), new Vector2(hit.point), 0.2f, 0.5f, ResourceManager.materialColorMagenta);

        //        distanceTravelled += hit.distance;

        //        float reflectionRatio = hit.collider.gameObject.GetComponent<Edge>().material.reflectionRatio;

        //        if(soundModel.properties.debugProperties.useGlobalReflectionRatio.value)
        //            reflectionRatio = soundModel.properties.debugProperties.globalReflectionRatio.value;

        //        currentRange = currentRange - UnityEngine.Vector2.Distance(hit.point, currentPosition);
        //        currentRange *= reflectionRatio;
        //        currentDirection = (currentDirection - 2 * UnityEngine.Vector2.Dot(currentDirection, hit.normal) * hit.normal).normalized;
        //        currentPosition = hit.point + 0.05f * currentDirection;

        //        if(currentRange < soundModel.properties.modelProperties.reflectionMinimumRangeLimit.value)
        //            break;

        //        reflectionSources.Add(new ReflectionSource(new Vector2(currentPosition.x, currentPosition.y), currentRange, new Vector2(currentDirection.x, currentDirection.y), new Vector2(hit.normal.x, hit.normal.y), distanceTravelled, soundModel.properties.modelProperties));
        //        //soundModel.debugger.drawPoint(new Vector2(currentPosition), 2, 1, ResourceManager.materialColorYellow);

        //        if(bounceCounter++ >= bounceLimit)
        //            break;
        //    }while(true);
        //        //soundModel.debugger.drawLine(new Vector2(currentPosition), new Vector2(currentPosition + currentDirection * currentRange), 0.2f, 0.5f, ResourceManager.materialColorMagenta);
        //}

        //private void findMeshCoordinates(){
        //    int sourceX = (int) ((position.x - soundModel.manager.activeGeometry.min.x) / soundModel.manager.geometryProperties.mapPartitionSize.value);
        //    int sourceY = (int) ((position.y - soundModel.manager.activeGeometry.min.y) / soundModel.manager.geometryProperties.mapPartitionSize.value);
        //    int radius = ((int) Mathf.Ceil(range) - 1) / soundModel.manager.geometryProperties.mapPartitionSize.value + 1;

        //    int xStart = Mathf.Max(0, sourceX - radius);
        //    int xEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapWidth - 1, sourceX + radius);
        //    int yStart = Mathf.Max(0, sourceY - radius);
        //    int yEnd = Mathf.Min(soundModel.manager.activeGeometry.partitionedMapHeight - 1, sourceY + radius);

        //    for(int i = xStart; i <= xEnd; i++)
        //        for(int j = yStart; j <= yEnd; j++)
        //            foreach(Geometry.PartitionedVertex partitionedVertex in soundModel.manager.activeGeometry.partitionedMap[i, j])
        //                castRay(partitionedVertex.vertex, partitionedVertex.vertex - position);

        //    float degrees = 10;
        //    for(float i = 0; i < 360; i += degrees){
        //        float radians = i * Mathf.Deg2Rad;
        //        castRay(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)));
        //    }
        //}

        //private void castRay(Vector2 direction){
        //    int offset;
        //    RaycastHit2D[] hits = Utils.raycastAll(position, direction, range, out offset);

        //    for(int i = offset; i < hits.Length; i++){
        //        RaycastHit2D hit = hits[i];
        //        Vector2 point = new Vector2(hit.point);

        //        Edge[] edges;
        //        bool exists = soundModel.manager.activeGeometry.vertexEdgeMap.TryGetValue(point, out edges);

        //        if(!exists){
        //            meshCoordinates.Add(point);
        //            return;
        //        }else{
        //            Vector2[] neighbors = Utils.findNeighbors(point, edges);

        //            if(!MathUtils.oneWayBitangent(position, point, neighbors[0], neighbors[1])){
        //                meshCoordinates.Add(point);
        //                return;
        //            }
        //        }
        //    }

        //    meshCoordinates.Add(position + direction.normalize() * range);
        //}

        //private void castRay(Vector2 vertex, Vector2 direction){
        //    float distance = Vector2.getDistance(position, vertex);
        //    bool instanceAdded = range <= distance;

        //    int offset;
        //    RaycastHit2D[] hits = Utils.raycastAll(position, direction, range, out offset);

        //    for(int i = offset; i < hits.Length; i++){
        //        RaycastHit2D hit = hits[i];
        //        Vector2 point = new Vector2(hit.point);

        //        if(!instanceAdded && distance < hit.distance){
        //            instanceAdded = true;
        //            meshCoordinates.Add(vertex);
        //        }

        //        Edge[] edges;
        //        bool exists = soundModel.manager.activeGeometry.vertexEdgeMap.TryGetValue(point, out edges);

        //        if(!exists)
        //            if(Vector2.getSquareMagnitude(point, vertex) < 0.01f){
        //                edges = soundModel.manager.activeGeometry.vertexEdgeMap[vertex];

        //                Vector2[] neighbors = Utils.findNeighbors(vertex, edges);

        //                if(!MathUtils.oneWayBitangent(position, vertex, neighbors[0], neighbors[1])){
        //                    meshCoordinates.Add(vertex);
        //                    return;
        //                }
        //            }else{
        //                meshCoordinates.Add(point);
        //                return;
        //            }
        //        else{
        //            Vector2[] neighbors = Utils.findNeighbors(point, edges);

        //            if(!MathUtils.oneWayBitangent(position, point, neighbors[0], neighbors[1])){
        //                meshCoordinates.Add(point);
        //                return;
        //            }
        //        }
        //    }

        //    if(!instanceAdded)
        //        meshCoordinates.Add(vertex);

        //    meshCoordinates.Add(position + direction.normalize() * range);
        //}

        //private void sortMeshCoordinates(){
        //    meshCoordinates.Sort((vector2, vector1) => {
        //        if(vector1 == vector2)
        //            return 0;

        //        if(vector1 == position)
        //            return 1;
        //        else if(vector2 == position)
        //            return -1;

        //        Vector2 v1 = vector1 - position;
        //        Vector2 v2 = vector2 - position;

        //        float angle1;
        //        float angle2;

        //        angle1 = Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;
        //        angle2 = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;

        //        if(float.IsNaN(angle1))
        //            angle1 = position.y > v1.y ? 270 : 90;

        //        if(float.IsNaN(angle2))
        //            angle2 = position.y > v2.y ? 270 : 90;

        //        if(angle1 < 0)
        //            angle1 += 360;
        //        if(angle2 < 0)
        //            angle2 += 360;

        //        if(Mathf.Abs(angle1 - angle2) > 0.005f)
        //            if(angle1 < angle2)
        //                return 1;
        //            else if(angle1 > angle2)
        //                return -1;

        //        if(Utils.floatEqual(angle1, 0f)){
        //            if(Utils.floatEqual(angle2, 0f)){
        //                angle1 = angle2;
        //            }else if(Utils.floatEqual(angle2, 360))
        //                angle1 = angle2;
        //        }

        //        if(Utils.floatEqual(angle2, 0f)){
        //            if(Utils.floatEqual(angle1, 0f)){
        //                angle1 = angle2;
        //            }else if(Utils.floatEqual(angle1, 360))
        //                angle1 = angle2;
        //        }

        //        return angle1 > angle2 ? 1 : -1;
        //    });

        //    for(int i = 0; i < meshCoordinates.Count; i++){
        //        Vector2 current = meshCoordinates[i];
        //        Vector2 next = meshCoordinates[(i + 1) % meshCoordinates.Count];

        //        float angleCurrent = Mathf.Atan2(current.y - position.y, current.x - position.x) * Mathf.Rad2Deg;
        //        float angleNext = Mathf.Atan2(next.y - position.y, next.x - position.x) * Mathf.Rad2Deg;

        //        if(float.IsNaN(angleCurrent))
        //            angleCurrent = position.y > current.y ? 270 : 90;

        //        if(float.IsNaN(angleNext))
        //            angleNext = position.y > next.y ? 270 : 90;

        //        if(angleCurrent < 0)
        //            angleCurrent += 360;
        //        if(angleNext < 0)
        //            angleNext += 360;

        //        if(Mathf.Abs(angleCurrent - angleNext) > 0.05f)
        //            continue;

        //        Vector2 currentNeighbor = meshCoordinates[(i - 1 + meshCoordinates.Count) % meshCoordinates.Count];
        //        Vector2 nextNeighbor = meshCoordinates[(i + 2) % meshCoordinates.Count];

        //        Vector2 closest = current;
        //        Vector2 furthest = next;
        //        Vector2 closestNeighbor = currentNeighbor;
        //        Vector2 furthestNeighbor = nextNeighbor;

        //        if(Vector2.getSquareMagnitude(closest, position) > Vector2.getSquareMagnitude(furthest, position)){
        //            closest = next;
        //            furthest = current;
        //            closestNeighbor = nextNeighbor;
        //            furthestNeighbor = currentNeighbor;
        //        }

        //        Edge[] edges;
        //        if(!soundModel.manager.activeGeometry.vertexEdgeMap.TryGetValue(closest, out edges))
        //            continue;

        //        //soundModel.debugger.drawPoint(closest, 3, 0.9f, ResourceManager.materialColorBlue);
        //        //soundModel.debugger.drawPoint(furthest, 3, 0.9f, ResourceManager.materialColorRed);
        //        //soundModel.debugger.drawPoint(closestNeighbor, 3, 0.9f, ResourceManager.materialColorMagenta);
        //        //soundModel.debugger.drawPoint(furthestNeighbor, 3, 0.9f, ResourceManager.materialColorYellow);

        //        foreach(Edge edge in edges){
        //            //float close = Mathf.Abs(edge.edge.substituteInEquation(closestNeighbor.x, closestNeighbor.y));
        //            //float far = Mathf.Abs(edge.edge.substituteInEquation(furthestNeighbor.x, furthestNeighbor.y));

        //            float close;
        //            float far;

        //            edge.edge.findClosestPointsOnLine(closestNeighbor, out close);
        //            edge.edge.findClosestPointsOnLine(furthestNeighbor, out far);

        //            if(close < 0.01f)
        //                break;
        //            else if(far < 0.01f){
        //                meshCoordinates[i] = next;
        //                meshCoordinates[(i + 1) % meshCoordinates.Count] = current;
        //                break;
        //            }
        //        }
        //    }
        //}

        //private void initValues(){
        //    position = new Vector2(soundModel.propertiesObject.transform.position.x, soundModel.propertiesObject.transform.position.z);
        //    amplitude = soundModel.properties.modelProperties.amplitude.value;

        //    range = calculateRange(amplitude, soundModel);

        //    switch(soundModel.properties.modelProperties.soundModelMethod.value){
        //        case SoundModelMethod.Method.OUR_METHOD:
        //            if(soundModel.GetType() == typeof(MobileSoundModel))
        //                soundModelMethod = SoundModelMethod.ourMobileMethod;
        //            else
        //                soundModelMethod = SoundModelMethod.ourStaticMethod;
        //            break;

        //        case SoundModelMethod.Method.EUCLIDEAN:
        //            soundModelMethod = SoundModelMethod.euclideanMethod;
        //            break;
        //    }
        //}

        //public void cleanUp(){
        //    uniqueCorners.Clear();
        //    allDiffractionSources.Clear();
        //    diffractionSources.Clear();
        //    reflectionSources.Clear();
        //    transmissionSources.Clear();
        //    meshCoordinates.Clear();
        //}

        //public override float computeAmplitude(Vector2 point, SoundModel soundModel){
        //    float distance = Vector2.getDistance(position, point);

        //    if(!Utils.isInVision(position, point - position, distance))
        //        return 0;

        //    return transferFunction(amplitude, distance * distance, soundModel);
        //}
        internal float transferFunction(float amplitude, float v, SoundModel soundModel) {
            throw new NotImplementedException();
        }

        internal float transferFunction(float amplitude, Vector2 vector2, DiffractionSource diffractionSource, SoundModel soundModel) {
            throw new NotImplementedException();
        }
    }
}
