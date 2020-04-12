namespace CDFCVideoExactor.Models {
    /// <summary>
    /// 预览帧实体;
    /// </summary>
    public class PreviewImg  {
        public PreviewImg(string imagePath) {
            this.imagePath = imagePath;
        }
        private string imagePath;
        public  string ImagePath {
            get {
                return imagePath;
            }
            
        }
    }
}
