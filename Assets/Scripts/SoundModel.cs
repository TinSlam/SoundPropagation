using UnityEngine;

namespace SoundPropagation{
    public abstract class SoundModel: MonoBehaviour{
        public GameObject managerObject;
        public GameObject propertiesObject;

        public PrimarySoundSource source;
        public AudioManager audioManager;
        [HideInInspector] public SoundProperties properties;

        private GameObject listenerObject;
        [HideInInspector] public Vector2 listenerDirection;

        [HideInInspector] public Manager manager;
        public SoundModelDebugger debugger;

        private GameObject rangeCircle;
        private GameObject pointObject;
        private GameObject beaconObject;

        protected Vector3 oldPosition;

        public StopWatch totalModelComputationTime = new StopWatch();
        public StopWatch totalSoundComputationTime = new StopWatch();
        public Profiler.ProfilingUnit averageModelComputationTime = new Profiler.ProfilingUnit(10);
        public Profiler.ProfilingUnit averageSoundComputationTime = new Profiler.ProfilingUnit(10);
        public Profiler.ProfilingUnit averageDiffractionSoundComputationTime = new Profiler.ProfilingUnit(10);
        public Profiler.ProfilingUnit averageReflectionSoundComputationTime = new Profiler.ProfilingUnit(10);
        public Profiler.ProfilingUnit averageTransmissionSoundComputationTime = new Profiler.ProfilingUnit(10);

        private bool transitioning = false;
        private bool transitionFirst = false;
        private float transitionTime = 0.5f;
        private float transitionInitialTime;
        private float tempVolume;

        private void OnEnable() {
            transitioning = true;
            transitionFirst = true;
            transitionInitialTime = Time.realtimeSinceStartup;
        }

        private void OnDisable() {
            if(properties != null && properties.modelProperties != null)
                properties.modelProperties.maxVolume.value = tempVolume;
        }

        private bool initialized = false;

        private void Start() {
            init();
        }

        public void init() {
            if(initialized)
                return;

            initialized = true;

            oldPosition = propertiesObject.transform.position;

            properties = propertiesObject.GetComponent<SoundProperties>();
            properties.initialize();

            manager = managerObject.GetComponent<Manager>();
            manager.addSoundSource(this);

            listenerObject = manager.fpsController.transform.GetChild(0).gameObject;

            debugger = new SoundModelDebugger(this);

            source = new StaticSource(this);

            audioManager = new AudioManager(this, gameObject);
            audioManager.playLoop();

            pointObject = Instantiate(ResourceManager.pointPrefab);
            pointObject.name = "Source";
            pointObject.transform.parent = transform;

            rangeCircle = Instantiate(ResourceManager.circlePrefab);
            rangeCircle.name = "RangeCircle";
            rangeCircle.transform.parent = transform;

            createBeaconObject();

            updateModel();
        }

        private void transition() {
            if(!transitioning)
                return;

            if(transitionFirst) {
                tempVolume = properties.modelProperties.maxVolume.value;
                transitionFirst = false;
            }

            float elapsedTime = Time.realtimeSinceStartup - transitionInitialTime;
            if(elapsedTime >= transitionTime) {
                transitioning = false;
                properties.modelProperties.maxVolume.value = tempVolume;
            } else
                properties.modelProperties.maxVolume.value = Mathf.Lerp(0, tempVolume, elapsedTime / transitionTime);
        }

        private int countVertices(Obstacle.ObstacleTreeNode node) {
            int count = 0;

            foreach(Obstacle.Vertex vertex in node.obstacle.vertices)
                if(Vector2.getDistance(vertex.position, source.position) < source.amplitude)
                    count++;

            foreach(Obstacle.ObstacleTreeNode child in node.children)
                count += countVertices(child);

            return count;
        }

        private void countVertices() {
            int count = 0;
            foreach(Obstacle.ObstacleTreeNode node in manager.activeGeometry.obstacleTree)
                count += countVertices(node);

            Debug.Log("Affected Vertices: " + count);
        }

        private void Update() {
            transition();

            if(Input.GetKeyDown(KeyCode.K))
                countVertices();

            rangeCircle.SetActive(properties.debugProperties.visualize.value && properties.debugProperties.visualizeRangeCircle.value);
            beaconObject.SetActive(properties.debugProperties.visualize.value && manager.geometryProperties.renderMode3D.value && properties.debugProperties.visualizeBeacon.value);
            pointObject.SetActive(properties.debugProperties.visualize.value && properties.debugProperties.visualizeSourcePosition.value);

            Timer soundComputationTimer = new Timer();
            totalSoundComputationTime.resume();
            soundComputationTimer.start();

            computeSound();

            soundComputationTimer.stop();
            totalSoundComputationTime.pause();

            averageSoundComputationTime.addNewValue(soundComputationTimer.getTimeElapsed());

            if(properties.audioProperties.isChanged())
                audioManager.updateAudioSources();

            update();
        }

        public abstract void update();

        protected void updateModel() {
            source.computeModel();

            RenderingUtils.drawPoint(source.position, 1, 0.5f, ResourceManager.materialColorRed, pointObject);
            RenderingUtils.drawCircle(source.position, source.range, 0.5f, 0.02f, ResourceManager.materialColorMagenta, rangeCircle);
            beaconObject.transform.position = new Vector3(source.position.x, 0, source.position.y);
        }

        private void computeSound() {
            Vector2 listenerPosition;
            if(manager.geometryProperties.renderMode3D.value) {
                listenerPosition = new Vector2(listenerObject.transform.position.x, listenerObject.transform.position.z);

                if(properties.audioProperties.outputChannels.value == AudioManager.OutputChannels.stereo)
                    listenerDirection = new Vector2(listenerObject.transform.forward.x, listenerObject.transform.forward.z);
            } else
                listenerPosition = Utils.getMousePosition();

            source.soundModelMethod.computeSound(listenerPosition, source);
        }

        public void createBeaconObject() {
            beaconObject = Instantiate(ResourceManager.beaconPrefab);
            beaconObject.name = "Beacon";
            beaconObject.transform.parent = transform;
            beaconObject.transform.localScale = new Vector3(0.2f, 20, 0.2f);
            beaconObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Color");
        }

        private void OnDestroy() {
            if(manager != null)
                manager.removeSoundSource(this);
        }
    }
}
