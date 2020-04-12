using System.Collections.Generic;

namespace CDFCVideoExactor.Models {
    /// <summary>
    /// 预览页模型;
    /// </summary>
    public class PreviewPage {
        /// <summary>
        /// 预览页模型的构方法；
        /// </summary>
        /// <param name="page">此页的页码</param>
        /// <param name="book">所属的预览文件</param>
        internal PreviewPage(short page, PreviewBook book) {
            this.Page = page;
            this.PreviewBook = book;
        }

        /// <summary>
        /// 所属的预览书;
        /// </summary>
        public PreviewBook PreviewBook { get; private set; }

        /// <summary>
        /// 此页页码;
        /// </summary>
        public short Page { get; private set; }

        /// <summary>
        /// 已生成图像的路径;
        /// </summary>
        public List<PreviewImg> PreviewImgs { get; private set; } = new List<PreviewImg>();
    }
}
