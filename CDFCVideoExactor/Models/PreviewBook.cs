using CDFCEntities.Files;
using System;
using System.Collections.Generic;

namespace CDFCVideoExactor.Models {
    /// <summary>
    /// 预览文件项模型;
    /// </summary>
    public partial class PreviewBook {
        /// <summary>
        /// 预览文件模型的构造方法;
        /// </summary>
        /// <param name="video">文件</param>
        /// <param name="recoverer">恢复器</param>
        public PreviewBook(Video video,string previewFilePath) {
            if(video == null) {
                EventLogger.Logger.WriteLine("PreviewItem构造错误:参数不得为空!");
                throw new ArgumentNullException("video");
            }
            this.Video = video;
            this.PreviewFilePath = previewFilePath;
        }

        /// <summary>
        /// 预览的文件;
        /// </summary>
        public readonly Video Video;
        
        /// <summary>
        /// 预览的源文件路径;
        /// </summary>
        public readonly string PreviewFilePath;
    }
    public partial class PreviewBook {
        /// <summary>
        /// 当前预览页码;
        /// </summary>
        public PreviewPage CurPage { get; set; }

        /// <summary>
        /// 已经经过的页码;
        /// </summary>
        public List<PreviewPage> PastedPages { get; private set; } = new List<PreviewPage>();

        /// <summary>
        /// 创建一新页;
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public PreviewPage CreatePage(short page) {
            var previewPage = new PreviewPage(page, this);
            PastedPages.Add(previewPage);
            return previewPage;
        }
        
    }
    
}
