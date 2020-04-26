using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Adviser
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
     
        public static string AppName = "Adviser";

        public static SettingsClass app_settings = new SettingsClass();
     
    }
    public class SettingsClass
    {
        public string ServiceAddress = "";
        public string ServiceName = "";
    }
}
