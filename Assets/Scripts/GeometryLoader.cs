using UnityEngine;

namespace SoundPropagation{
    public class GeometryLoader: MonoBehaviour{
        [Tooltip("Maps from https://movingai.com/benchmarks/grids.html")]
        public bool loadMapFromFile;
 
        [Tooltip("Relative path from Assets/Resources/Maps/")]
        public string fileName;

        public void load(){
            if(loadMapFromFile)
                loadFromFile();
            else
                loadFromObjects();
        }

        private void loadFromFile(){

        }

        private void loadFromObjects(){

        }zd
    }
}
