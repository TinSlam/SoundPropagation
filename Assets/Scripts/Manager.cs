using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace SoundPropagation{
    public partial class Manager: MonoBehaviour{
        public Geometry.Properties geometryProperties;
        public Debugger.Properties debugProperties;
        //public TestScenario.Properties testProperties;

        private GameObject camera2DObject;
        [HideInInspector] public GameObject fpsController;
        public GameObject debugSoundModel;

        public Debugger debugger;
        //private UI ui;

        [HideInInspector] public List<SoundModel> soundSources = new List<SoundModel>();

        private void Awake(){
            loadPrefabs();
            loadGeometry();

            geometryProperties.initialize();
            debugProperties.initialize();
            //testProperties.initialize();

            ResourceManager.initializeResources();

            debugger = new Debugger(this);

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

        public void addSoundSource(SoundModel source){
            soundSources.Add(source);
        }

        public void removeSoundSource(SoundModel source){
            soundSources.Remove(source);
        }

        public void Start(){
            updateDebugModel();
        }

        private void updateDebugModel(){
            if(debugSoundModel == null){
                if(soundSources.Count != 0)
                    debugger.soundModel = soundSources[0];
            }else
                debugger.soundModel = debugSoundModel.GetComponent<SoundModel>();
            
            if(debugger.soundModel != null)
                debugSoundModel = debugger.soundModel.gameObject;
        }

        private void Update(){
            updateDebugModel();

            debugger.update();
            //if(debugger.soundModel != null)
            //    updateUI();

            if(!geometryProperties.isChanged())
                return;

            if(geometryProperties.activeGeometryIndex.value >= geometries.Count || geometryProperties.activeGeometryIndex.value < 0){
                Debug.Log("Geometry index out of bounds! No change made.");
                return;
            }

            setGeometry(geometries[geometryProperties.activeGeometryIndex.value]);

            updateGeometry();
        }
    }
}
