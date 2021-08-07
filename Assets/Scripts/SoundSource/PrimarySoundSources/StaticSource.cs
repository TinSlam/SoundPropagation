namespace SoundPropagation{
    public class StaticSource: PrimarySoundSource{
        public StaticSource(SoundModel soundModel): base(soundModel, SoundModelMethod.ourStaticMethod){

        }
    }
}
