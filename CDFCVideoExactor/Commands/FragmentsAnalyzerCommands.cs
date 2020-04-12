using System.Windows.Input;

namespace CDFCVideoExactor.Commands {
    //碎片图表分析器使用的命令;
    public static class FragmentsAnalyzerCommands {
        //置为临界区的命令;
        private static RoutedUICommand setAsBorderNodeCommand;
        public static RoutedUICommand SetAsBorderNodeCommand {
            get {
                if(setAsBorderNodeCommand == null) {
                    setAsBorderNodeCommand = new RoutedUICommand(
                        "Set As Border Node","SetAsBordeNodeCommand",typeof(FragmentsAnalyzerCommands)
                    );
                }
                return setAsBorderNodeCommand;
            }
        }
    }
}
