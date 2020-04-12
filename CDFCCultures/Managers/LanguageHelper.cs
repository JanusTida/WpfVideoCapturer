using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using EventLogger;

namespace CDFCCultures.Managers {
    public static class LanguageHelper {
        //是否被更改;
        private static bool modified = true;
        public const string DefaultLanguage = "zh_CN";
        private const string Cons_LanguageType = "LanguageType";
        private static string language;
        public static string Language {
            get {
                //若被修改或者是第一次启动;
                if (modified) {
                    language = ConfigurationManager.AppSettings[Cons_LanguageType];
                    if(language == null) {
                        Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        cfa.AppSettings.Settings.Add(Cons_LanguageType, DefaultLanguage);
                        cfa.Save();
                        return DefaultLanguage;
                    }
					modified = false;
                }
                return language;
            }
            set {
                language = value;
                try {
                    Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    cfa.AppSettings.Settings[Cons_LanguageType].Value = value;
                    cfa.Save();
                    modified = true;
                }
                catch {
                    
                }
                
            }
        }

        public static void LoadLanguage(this Collection<ResourceDictionary> dictArr,string moduleName) {
            ResourceDictionary localLanResr = new ResourceDictionary();
            try {
                var curlan = Language;
                Logger.WriteLine("当前语言:" + curlan);
                var lanPath = AppDomain.CurrentDomain.BaseDirectory + "Languages/";
                if (curlan != DefaultLanguage) {
                    ResourceDictionary curLanResr = null;
                    foreach (var item in dictArr) {
                        if (item.Source.ToString().EndsWith($"{moduleName}.xaml")) {
                            curLanResr = item;
                        }
                    }
                    localLanResr.Source = new Uri($"{lanPath}{curlan}/{moduleName}.xaml");
                    if (curLanResr != null) {
                        Logger.WriteLine("默认语言:" + curlan);
                        dictArr.Remove(curLanResr);
                    }

                    dictArr.Add(localLanResr);
                }
            }
            catch(Exception ex) {
                Logger.WriteLine("错误:" + ex.Message);
            }
        }
    }
}
