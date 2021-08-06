using UnityEngine;

namespace SoundPropagation{
    public abstract class GenericProperty{
        public abstract void initialize();
        public abstract bool isChanged();
        public abstract void update();
    }

    public class Property<T>: GenericProperty{
        private T oldValue;
        public T value;

        public override void initialize(){
            oldValue = value;
        }

        public override bool isChanged(){
            return !value.Equals(oldValue);
        }

        public override void update(){
            oldValue = value;
        }
    }

    [System.Serializable] public class PropertyBool: Property<bool>{}
    [System.Serializable] public class PropertyInt: Property<int>{}
    [System.Serializable] public class PropertyFloat: Property<float>{}
    [System.Serializable] public class PropertyFloatArray: Property<float[]>{}
    [System.Serializable] public class PropertyVector2: Property<Vector2>{}
    [System.Serializable] public class PropertyString: Property<string>{}
    [System.Serializable] public class PropertyAudioClip: Property<AudioClip>{}
    [System.Serializable] public class PropertySoundModelMethodEnum: Property<SoundModelMethod.Method> { }
    [System.Serializable] public class PropertyTransferFunctionEnum: Property<SoundSourceBase.TransferFunctionMethod> { }
    [System.Serializable] public class PropertyOutputChannelsEnum: Property<AudioManager.OutputChannels> { }

    public abstract class Properties{
        private GenericProperty[] properties;

        public abstract void initialize();

        public void setProperties(GenericProperty[] properties){
            this.properties = properties;

            foreach(GenericProperty property in properties)
                property.initialize();
        }

        public bool isChanged(){
            bool isChanged = false;

            foreach(GenericProperty property in properties){
                if(property.isChanged())
                    isChanged = true;
                property.update();
            }

            return isChanged;
        }
    }
}
