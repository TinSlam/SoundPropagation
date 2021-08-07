using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class Debugger{
        [System.Serializable]
        public class Properties: SoundPropagation.Properties{
            public PropertyBool mouseFollow2D;
            public PropertyFloat cameraTranslationSpeed;
            public PropertyInt pointPoolSize;
            public PropertyInt linePoolSize;
            public PropertyInt circlePoolSize;

            public override void initialize(){
                setProperties(new GenericProperty[]{
                    mouseFollow2D,
                    cameraTranslationSpeed,
                    pointPoolSize,
                    linePoolSize,
                    circlePoolSize
                });
            }
        }

        private GameObject debugger;

        private GameObject points;
        private GameObject lines;
        private GameObject circles;

        private ResourcePool pointsPool;
        private ResourcePool linesPool;
        private ResourcePool circlesPool;

        private Manager manager;
        public SoundModel soundModel;

        private List<GameObject> renderers = new List<GameObject>();

        private Vector2 storedMousePosition;

        public Debugger(Manager manager){
            this.manager = manager;

            createStructure();
            createResourcePool();

            //storedMousePosition = FileManager.readPoint("mouse");
        }

        private void createStructure(){
            debugger = new GameObject("Debugger");
            debugger.transform.parent = manager.gameObject.transform.parent;

            points = new GameObject("Points");
            points.transform.parent = debugger.transform;

            lines = new GameObject("Lines");
            lines.transform.parent = debugger.transform;

            circles = new GameObject("Circles");
            circles.transform.parent = debugger.transform;
        }

        private void createResourcePool(){
            pointsPool = new ResourcePool(manager.debugProperties.pointPoolSize.value, ResourceManager.pointPrefab, "Point", points.transform);
            linesPool = new ResourcePool(manager.debugProperties.linePoolSize.value, ResourceManager.linePrefab, "Line", lines.transform);
            circlesPool = new ResourcePool(manager.debugProperties.circlePoolSize.value, ResourceManager.circlePrefab, "Circle", circles.transform);
        }

        public void update(){
            cleanUpRenderers();

            camera2DControl();

            sourceSwitch();

            if(soundModel == null)
                return;

            pollEvents();

            followMouse();
        }

        private void sourceSwitch(){
            for(int i = 0; i < 9; i++)
                if(Input.GetKeyDown(KeyCode.Alpha1 + i))
                    if(i < manager.soundSources.Count){
                        soundModel = manager.soundSources[i];
                        return;
                    }
        }

        private void followMouse(){
            if(!manager.geometryProperties.renderMode3D.value && manager.debugProperties.mouseFollow2D.value){
                Vector2 mouse = Utils.getMousePosition();
                soundModel.propertiesObject.transform.position = new Vector3(mouse.x, 0, mouse.y);
            }
        }

        private void cleanUpRenderers(){
            pointsPool.releaseResources();
            linesPool.releaseResources();
            circlesPool.releaseResources();

            foreach(GameObject renderer in renderers)
                Object.Destroy(renderer);

            renderers.Clear();
        }

        private void pollEvents(){
            sourcePositionControl();

            if(Input.GetKey(KeyCode.KeypadPlus))
                soundModel.properties.modelProperties.amplitude.value += 1;

            if(Input.GetKey(KeyCode.KeypadMinus))
                soundModel.properties.modelProperties.amplitude.value = Mathf.Max(0, soundModel.properties.modelProperties.amplitude.value - 1);

            if(Input.GetKeyDown(KeyCode.V))
                soundModel.properties.debugProperties.visualize.value = !soundModel.properties.debugProperties.visualize.value;

            if(Input.GetKeyDown(KeyCode.C))
                soundModel.properties.modelProperties.diffraction.value = !soundModel.properties.modelProperties.diffraction.value;

            if(Input.GetKeyDown(KeyCode.T))
                soundModel.properties.modelProperties.transmission.value = !soundModel.properties.modelProperties.transmission.value;
        }

        private void sourcePositionControl(){
            //if(Input.GetKeyDown(KeyCode.Minus)){
            //    storedMousePosition = Utils.getMousePosition();
            //    FileManager.storePoint(storedMousePosition, "mouse");
            //}

            if(Input.GetKeyDown(KeyCode.Space) && !manager.geometryProperties.renderMode3D.value)
                manager.debugProperties.mouseFollow2D.value = !manager.debugProperties.mouseFollow2D.value;

            //if(Input.GetKeyDown(KeyCode.RightBracket)){
            //    soundModel.propertiesObject.transform.position = new Vector3(storedMousePosition.x, 0, storedMousePosition.y);
            //    manager.debugProperties.mouseFollow2D.value = false;
            //}

            float speed = 10 * Time.deltaTime;
            if(Input.GetKey(KeyCode.UpArrow))
                soundModel.propertiesObject.transform.position += new Vector3(0, 0, 1) * speed;
            if(Input.GetKey(KeyCode.LeftArrow))
                soundModel.propertiesObject.transform.position += new Vector3(-1, 0, 0) * speed;
            if(Input.GetKey(KeyCode.DownArrow))
                soundModel.propertiesObject.transform.position += new Vector3(0, 0, -1) * speed;
            if(Input.GetKey(KeyCode.RightArrow))
                soundModel.propertiesObject.transform.position += new Vector3(1, 0, 0) * speed;
        }

        private void camera2DControl(){
            if(!manager.geometryProperties.renderMode3D.value){
                float speed = manager.debugProperties.cameraTranslationSpeed.value * Time.deltaTime;
                if(Input.GetKey(KeyCode.D))
                    Utils.camera2D.transform.Translate(new Vector3(speed, 0, 0));
                if(Input.GetKey(KeyCode.A))
                    Utils.camera2D.transform.Translate(new Vector3(-speed, 0, 0));
                if(Input.GetKey(KeyCode.W))
                    Utils.camera2D.transform.Translate(new Vector3(0, speed, 0));
                if(Input.GetKey(KeyCode.S))
                    Utils.camera2D.transform.Translate(new Vector3(0, -speed, 0));
                if(Input.GetKey(KeyCode.R))
                    Utils.camera2D.orthographicSize = Mathf.Max(1, Utils.camera2D.orthographicSize - speed);
                if(Input.GetKey(KeyCode.F))
                    Utils.camera2D.orthographicSize += speed;

                if(Input.GetKeyDown(KeyCode.M))
                    Debug.Log("Mouse Position: " + Utils.getMousePosition());
            }
        }

        public void drawPoint(UnityEngine.Vector2 point){
            drawPoint(Vector2.convert(point));
        }

        public void drawPoint(Vector2 point){
            drawPoint(point, 1, 0.01f, ResourceManager.materialColorYellow);
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
    }
}
