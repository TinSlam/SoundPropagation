using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public partial class Manager{
        [HideInInspector] public Geometry activeGeometry;
        private List<Geometry> geometries = new List<Geometry>();

        public void addGeometry(Geometry geometry){
            geometries.Add(geometry);

            geometry.gameObject.SetActive(false);

            if(geometries.Count == 1)
                setGeometry(geometry);
        }

        public void setGeometry(Geometry geometry){
            activeGeometry = geometry;

            foreach(Geometry geo in geometries)
                geo.gameObject.SetActive(geo == activeGeometry);
        }

        public void setGeometry(){
            setGeometry(geometries[geometryProperties.activeGeometryIndex.value]);
        }

        private void updateGeometry(){
            activeGeometry.updateRenderMode(geometryProperties.renderMode3D.value);

            fpsController.SetActive(geometryProperties.renderMode3D.value);
            camera2DObject.SetActive(!geometryProperties.renderMode3D.value);

            if(geometryProperties.renderMode3D.value){
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }else{
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
