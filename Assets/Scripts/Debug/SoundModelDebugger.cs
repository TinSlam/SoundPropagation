using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class SoundModelDebugger{
        [System.Serializable]
        public class Properties: SoundPropagation.Properties{
            public PropertyBool visualize;
            public PropertyBool visualizeSourcePosition;
            public PropertyBool visualizeBeacon;
            public PropertyBool visualizeRangeCircle;
            public PropertyBool visualizePrimarySource;
            public PropertyBool visualizeDiffraction;
            public PropertyBool visualizeTransmission;
            public PropertyBool visualizeAllSoundDirections;
            public PropertyBool visualizeWeightedSoundDirection;
            public PropertyBool updateEachFrame;
            public PropertyBool useGlobalTransmissionRatio;
            public PropertyFloat globalTransmissionRatio;
            public PropertyBool useGlobalReflectionRatio;
            public PropertyFloat globalReflectionRatio;
            public PropertyInt pointPoolSize;
            public PropertyInt linePoolSize;
            public PropertyInt circlePoolSize;
            public PropertyInt soundMeshPoolSize;

            public override void initialize(){
                setProperties(new GenericProperty[]{
                    visualize,
                    visualizeSourcePosition,
                    visualizeBeacon,
                    visualizeRangeCircle,
                    visualizePrimarySource,
                    visualizeDiffraction,
                    visualizeTransmission,
                    visualizeAllSoundDirections,
                    visualizeWeightedSoundDirection,
                    updateEachFrame,
                    useGlobalTransmissionRatio,
                    globalTransmissionRatio,
                    useGlobalReflectionRatio,
                    globalReflectionRatio,
                    pointPoolSize,
                    linePoolSize,
                    circlePoolSize,
                    soundMeshPoolSize
                });
            }
        }

        private GameObject debugger;

        private GameObject points;
        private GameObject lines;
        private GameObject circles;
        public GameObject soundMeshes;

        private ResourcePool pointsPool;
        private ResourcePool linesPool;
        private ResourcePool circlesPool;
        private ResourcePool soundMeshesPool;

        public SoundModel soundModel;

        private List<GameObject> renderers = new List<GameObject>();

        public SoundModelDebugger(SoundModel soundModel){
            this.soundModel = soundModel;

            createStructure();
            createResourcePool();
        }

        private void createStructure(){
            debugger = new GameObject("Debugger");
            debugger.transform.parent = soundModel.gameObject.transform;

            points = new GameObject("Points");
            points.transform.parent = debugger.transform;

            lines = new GameObject("Lines");
            lines.transform.parent = debugger.transform;

            circles = new GameObject("Circles");
            circles.transform.parent = debugger.transform;

            soundMeshes = new GameObject("SoundMesh");
            soundMeshes.transform.parent = debugger.transform;
        }

        private void createResourcePool(){
            pointsPool = new ResourcePool(soundModel.properties.debugProperties.pointPoolSize.value, ResourceManager.pointPrefab, "Point", points.transform);
            linesPool = new ResourcePool(soundModel.properties.debugProperties.linePoolSize.value, ResourceManager.linePrefab, "Line", lines.transform);
            circlesPool = new ResourcePool(soundModel.properties.debugProperties.circlePoolSize.value, ResourceManager.circlePrefab, "Circle", circles.transform);
            soundMeshesPool = new ResourcePool(soundModel.properties.debugProperties.soundMeshPoolSize.value, ResourceManager.soundMeshPrefab, "SoundMesh", soundMeshes.transform);
        }

        public void update(){
            cleanUpRenderers();
        }

        private void cleanUpRenderers(){
            pointsPool.releaseResources();
            linesPool.releaseResources();
            circlesPool.releaseResources();
            soundMeshesPool.releaseResources();

            foreach(GameObject renderer in renderers)
                Object.Destroy(renderer);

            renderers.Clear();
        }

        public void drawPoint(UnityEngine.Vector2 point){
            drawPoint(new Vector2(point.x, point.y));
        }

        public void drawPoint(Vector2 point){
            drawPoint(point, 1, 0.05f, ResourceManager.materialColorYellow);
        }

        public void drawPoint(Vector2 point, float size, float height, Material material){
            GameObject renderer = pointsPool.useResource();

            if(renderer == null){
                renderer = Object.Instantiate(ResourceManager.pointPrefab);
                renderer.name = "DebugPoint";
                renderer.transform.parent = points.transform;
            
                RenderingUtils.drawPoint(point, size, height, material, renderer);
            
                renderers.Add(renderer);
            }else
                RenderingUtils.drawPoint(point, size, height, material, renderer);
        }

        public void drawLine(Vector2 start, Vector2 end){
            drawLine(start, end, 0.3f, 0.04f, ResourceManager.materialColorRed);
        }

        public void drawLine(Vector2 start, Vector2 end, float size, float depth, Material material){
            GameObject renderer = linesPool.useResource();
            
            if(renderer == null){
                renderer = Object.Instantiate(ResourceManager.linePrefab);
                renderer.name = "DebugLine";
                renderer.transform.parent = lines.transform;
            
                RenderingUtils.drawLine(start, end, size, depth, material, renderer);

                renderers.Add(renderer);
            }else
                RenderingUtils.drawLine(start, end, size, depth, material, renderer);
        }

        public void drawCircle(Vector2 center, float radius, float size, float depth, Material material){
            GameObject renderer = circlesPool.useResource();
            
            if(renderer == null){
                renderer = Object.Instantiate(ResourceManager.circlePrefab);
                renderer.name = "DebugCircle";
                renderer.transform.parent = circles.transform;

                RenderingUtils.drawCircle(center, radius, size, depth, material, renderer);

                renderers.Add(renderer);
            }else
                RenderingUtils.drawCircle(center, radius, size, depth, material, renderer);
        }

        public void drawSoundMesh(List<Vector2> points, Vector2 position, float amplitude, float maxAmplitude, float depth){
            GameObject renderer = soundMeshesPool.useResource();
            
            if(renderer == null){
                renderer = Object.Instantiate(ResourceManager.soundMeshPrefab);
                renderer.name = "DebugSoundMesh";
                renderer.transform.parent = soundMeshes.transform;

                RenderingUtils.drawSoundMesh(points, position, amplitude, maxAmplitude, depth, soundModel, renderer);

                renderers.Add(renderer);
            }else
                RenderingUtils.drawSoundMesh(points, position, amplitude, maxAmplitude, depth, soundModel, renderer);
        }

        public void drawSoundMeshDiffraction(List<Vector2> points, Vector2 position, float amplitude, float maxAmplitude, float depth, DiffractionSource diffractionSource){
            GameObject renderer = soundMeshesPool.useResource();
            
            if(renderer == null){
                renderer = Object.Instantiate(ResourceManager.soundMeshPrefab);
                renderer.name = "DebugSoundMesh";
                renderer.transform.parent = soundMeshes.transform;

                RenderingUtils.drawSoundMeshDiffraction(points, position, amplitude, maxAmplitude, depth, diffractionSource, soundModel, renderer);

                renderers.Add(renderer);
            }else
                RenderingUtils.drawSoundMeshDiffraction(points, position, amplitude, maxAmplitude, depth, diffractionSource, soundModel, renderer);
        }

        public void createAmplitudeHeatmap(string path, int xStart, int yStart, int xEndExclusive, int yEndExclusive, float granularity){
            int width;
            int height;
            float[,] heatMap = createHeatmap(xStart, yStart, xEndExclusive, yEndExclusive, granularity, out width, out height);

            string data = xStart + "\n" + yStart + "\n" + width + "\n" + height + "\n" + granularity + "\n" + soundModel.properties.modelProperties.amplitude.value;

            for(int i = 0; i < height; i++){
                data += "\n";
                for(int j = 0; j < width; j++)
                    data += heatMap[i, j] + " ";
            }

            FileManager.writeFile("Assets/Resources/Files/Heatmaps/" + path + ".heatmap", data);
        }

        public float[,] createHeatmap(int xStart, int yStart, int xEndExclusive, int yEndExclusive, float granularity, out int width, out int height){
            width = (int) ((xEndExclusive - xStart) / granularity);
            height = (int) ((yEndExclusive - yStart) / granularity);

            float[,] heatMap = new float[height, width];

            for(int i = 0; i < height; i++)
                for(int j = 0; j < width; j++){
                    Vector2 listenerPosition = new Vector2(xStart + j * granularity, yStart + i * granularity);
                    heatMap[i, j] = soundModel.source.soundModelMethod.computeAudio(listenerPosition, soundModel.source, out _);
                }

            return heatMap;
        }
    }
}
