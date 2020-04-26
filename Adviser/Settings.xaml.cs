using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace Adviser
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        
        public Settings()
        {
            InitializeComponent();
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }
        public void CloseClicked(object sender, RoutedEventArgs args)
        {
            Close();
        }

        private void ok_btn_Click(object sender, RoutedEventArgs e)
        {
            settings_work.SaveSettings(tb_address.Text, tb_name.Text);
            Close();
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tb_address.Text = App.app_settings.ServiceAddress;
            tb_name.Text = App.app_settings.ServiceName;
        }
    }

    public static class settings_work
    {
        const string Settings_fn = "settings.xml";
        public static Result SaveSettings(string serv_addr, string srv_name)
        {
            string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.AppName);

            if (!Directory.Exists(AppFolder))
            {
                Directory.CreateDirectory(AppFolder);
            }


            try
            {
                using (StreamWriter t_w = new StreamWriter(Path.Combine(AppFolder, Settings_fn)))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(SettingsClass));

                    formatter.Serialize(t_w, new SettingsClass { ServiceAddress = serv_addr, ServiceName = srv_name });

                    t_w.Flush();
                };
            }
            catch (Exception e)
            {

                return new Result { Status = false, Description = e.Message };

            }

            return new Result { Status = true };

        }

        public static Result LoadSettings()
        {
            string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.AppName);

            if (!Directory.Exists(AppFolder))
            {
                return new Result { Status = false, Description = "Нет файла настроек!" };
            }

            if (!File.Exists(Path.Combine(AppFolder, Settings_fn)))
            {
                return new Result { Status = false, Description = "Нет файла настроек!" };
            }

            try
            {
                using (StreamReader sr = new StreamReader(Path.Combine(AppFolder, Settings_fn)))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(SettingsClass));

                    App.app_settings = (SettingsClass)formatter.Deserialize(sr);

                };
            }
            catch (Exception e)
            {

                return new Result { Status = false, Description = e.Message };
            }

            return new Result { Status = true };
        }

    }
}
