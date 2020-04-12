using System.Windows.Input;

namespace CDFCVideoExactor.Commands {
    public static class DiskScanSettingCommands {
        private static RoutedUICommand startScanCommand;
        public static RoutedUICommand StartScanCommand {
            get {
                if(startScanCommand == null) {
                    startScanCommand = new RoutedUICommand("Start Scan", "StartScanCommand", typeof(DiskScanSettingCommands));
                }
                return startScanCommand;
            }
        }
    }
}
