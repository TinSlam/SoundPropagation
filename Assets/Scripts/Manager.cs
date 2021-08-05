using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace SoundPropagation{
    public partial class Manager: MonoBehaviour{
        public Geometry.Properties geometryProperties;
        //public Debugger.Properties debugProperties;
        //public TestScenario.Properties testProperties;

        private GameObject camera2DObject;
        private GameObject fpsController;

        private void Awake(){
            loadPrefabs();
            loadGeometry();

            geometryProperties.initialize();
            //debugProperties.initialize();
            //testProperties.initialize();

            ResourceManager.initializeResources();

            //debugger = new Debugger(this);

            //sceneDataObject.GetComponent<SceneData>().initialize();

            initialize2D();

            updateGeometry();
            
            //TestScenario.createScenarios(this);
        }

        private void loadPrefabs(){
            GameObject uiObject = new GameObject("UI Prefab");
            uiObject.transform.parent = transform.parent;

            fpsController = transform.parent.GetComponentInChildren<FirstPersonController>(true).gameObject;
        }

        private void loadGeometry(){
            GeometryLoader geometryLoader = transform.parent.GetComponentInChildren<GeometryLoader>();
            geometryLoader.load(this);
            Destroy(geometryLoader.gameObject);
        }

        private void initialize2D(){
            camera2DObject = new GameObject("Camera2D", typeof(Camera), typeof(AudioListener));
            camera2DObject.tag = "MainCamera";
            camera2DObject.transform.parent = transform.parent;

            Vector2 center = activeGeometry.center;
            camera2DObject.transform.position = new Vector3(center.x, 20, center.y);
            camera2DObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            Utils.camera2D = camera2DObject.GetComponent<Camera>();
            Utils.camera2D.backgroundColor = Color.black;
            Utils.camera2D.clearFlags = CameraClearFlags.SolidColor;
            
            Utils.camera2D.orthographic = true;
            Utils.camera2D.orthographicSize = Mathf.Max(activeGeometry.width * Screen.height / Screen.width, activeGeometry.height) / 2 + 1;
        }
    }
}
