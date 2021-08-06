using System.Collections;
namespace SoundPropagation{
    public class StaticSoundModel: SoundModel{
        public override void update(){
            bool propertiesChange = properties.modelProperties.isChanged();
            bool debugPropertiesChange = properties.debugProperties.isChanged();

            if(!properties.debugProperties.updateEachFrame.value && propertiesObject.transform.position == oldPosition && !propertiesChange && !debugPropertiesChange)
                return;

            oldPosition = propertiesObject.transform.position;

            debugger.update();
            updateModel();
        }
    }
}
