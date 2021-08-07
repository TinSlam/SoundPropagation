using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class ResourcePool{
        private LinkedList<GameObject> freeResources = new LinkedList<GameObject>();
        private List<GameObject> usedResources = new List<GameObject>();

        public ResourcePool(int size, GameObject prefab, string name, Transform parent){
            for(int i = 0; i < size; i++){
                GameObject obj = Object.Instantiate(prefab);
                obj.name = name + "Resource";
                obj.transform.parent = parent;
                obj.SetActive(false);
                freeResources.AddLast(obj);
            }
        }

        public GameObject useResource(){
            if(freeResources.Count == 0)
                return null;

            GameObject resource = freeResources.First.Value;
            freeResources.RemoveFirst();

            usedResources.Add(resource);
            
            resource.SetActive(true);

            return resource;
        }

        public void releaseResource(GameObject resource){
            resource.SetActive(false);
            freeResources.AddFirst(resource);
        }

        public void releaseResources(){
            foreach(GameObject resource in usedResources)
                releaseResource(resource);

            usedResources.Clear();
        }
    }
}
