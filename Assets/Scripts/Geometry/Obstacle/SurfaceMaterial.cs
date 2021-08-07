namespace SoundPropagation{
    [System.Serializable]
    public struct SurfaceMaterial{
        public static SurfaceMaterial defaultMaterial = new SurfaceMaterial(0, 0);

        public float reflectionRatio;
        public float transmissionRatio;

        public SurfaceMaterial(float reflectionRatio, float transmissionRatio){
            this.reflectionRatio = reflectionRatio;
            this.transmissionRatio = transmissionRatio;
        }

        public static SurfaceMaterial operator+(SurfaceMaterial mat1, SurfaceMaterial mat2){
            return new SurfaceMaterial(mat1.reflectionRatio + mat2.reflectionRatio, mat1.transmissionRatio + mat2.transmissionRatio);
        }

        public static SurfaceMaterial operator/(SurfaceMaterial mat1, SurfaceMaterial mat2){
            return new SurfaceMaterial(mat1.reflectionRatio / mat2.reflectionRatio, mat1.transmissionRatio / mat2.transmissionRatio);
        }

        public static SurfaceMaterial operator/(SurfaceMaterial mat, float num){
            return new SurfaceMaterial(mat.reflectionRatio / num, mat.transmissionRatio / num);
        }
    }
}
