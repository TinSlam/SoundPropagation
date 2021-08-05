using UnityEngine;

namespace SoundPropagation{
    public class Manager: MonoBehaviour{
        private Camera camera2D;

        private void Start(){
            createPrefabs();
            loadGeometry();
        }

        private void createPrefabs(){
            GameObject geometryObject = new GameObject("Sound Geometry");
            geometryObject.transform.parent = transform.parent;

            GameObject uiObject = new GameObject("UI Prefab");
            uiObject.transform.parent = transform.parent;
        }

        private void loadGeometry(){
            GeometryLoader geometryLoader = transform.parent.GetComponentInChildren<GeometryLoader>();
            geometryLoader.load();
            Destroy(geometryLoader.gameObject);
        }
    }
}
