using System.Windows.Input;

namespace CDFCVideoExactor.Commands {

    /// <summary>
    /// 主窗体所需与业务逻辑无关的命令;
    /// </summary>
    public static class MainWindowCommands {
        /// <summary>
        ///文件菜单展开的命令;
        /// </summary>
        private static RoutedUICommand menuOpenCommand;
        public static RoutedUICommand MenuOpenCommand {
            get {
                return menuOpenCommand ??
                    (menuOpenCommand = new RoutedUICommand(
                        "MenuOpen(F)", "MenuOpenCommand", typeof(MainWindowCommands)
                            ));
            }
        }
        
    }

}
