using System.Windows;

namespace CDFCCultures.Managers {
    public static class ResourceManager {
        public static string FindResourceString(string keyName) {
            var resource = Application.Current.TryFindResource(keyName);
            if (resource != null) {
                return resource.ToString();
            }
            else {
                return string.Empty;
            }
        }

        
    }
}
