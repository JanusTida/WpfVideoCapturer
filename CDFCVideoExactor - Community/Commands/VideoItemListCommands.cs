using System.Windows.Input;

namespace CDFCVideoExactor.Commands {
    /// <summary>
    /// 文件列表的命令;
    /// </summary>
    public static  class VideoItemListCommands {
        //在图表中显示文件碎片的命令;
        private static RoutedUICommand showFragmentPositionCommand;
        public static RoutedCommand ShowFragmentPositionCommand {
            get {
                if(showFragmentPositionCommand == null) {
                    showFragmentPositionCommand = new RoutedUICommand(
                        "ShowPosition(F)", "ShowFragmentPosCommand", typeof(VideoItemListCommands),
                            new InputGestureCollection(
                                new InputGesture[] {
                            //        new KeyGesture(Key.F,ModifierKeys.Alt)
                                }
                            ));
                }
                return showFragmentPositionCommand;
            }
        }

        //恢复文件的命令;
        private static RoutedUICommand recoverFileCommand;
        public static RoutedUICommand RecoverFileCommand {
            get {
                if(recoverFileCommand == null) {
                    recoverFileCommand = new RoutedUICommand(
                        "Recover File","RecoverFileCommand",typeof(VideoItemListCommands),
                        new InputGestureCollection(
                            new InputGesture[] {
                                new KeyGesture (Key.R,ModifierKeys.Control)
                            }
                        )
                    );
                }
                return recoverFileCommand;
            }
        }
    }
}
