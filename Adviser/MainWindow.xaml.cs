//Установщик (при деинсталляции удаление всех папок программы (вопрос об удалении настроек)) - ок
//Отображение восклицательного знака на картинке в трее, когда входящие данные отличаются от предыдущих. - ok
//При изменении данных - уведомление рядом с треем. - ok
//Красивости всякие понапридумать в wpf


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Adviser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon ni = new NotifyIcon();
        Icon ic = new Icon("pilot_10.ico");
        Icon ic_warn = new Icon("pilot_10_warn.ico");

        DataTable dt_old = new DataTable();
        DataTable dt_new = new DataTable();

        //PopUp popup = new PopUp();

        public MainWindow()
        {

  
            this.Visibility = Visibility.Hidden;

            //string[] loc_str_args = Environment.GetCommandLineArgs();
            //string[] loc_sep = { " " };
            //string[] loc_sep_arg;

            //string[] loc_sep_0 = { "\\" };

            //string[] arr_path = Process.GetCurrentProcess().MainModule.FileName.Split(loc_sep_0, StringSplitOptions.None);

            //string loc_folder = "";


            //foreach (string loc_arg in loc_str_args)
            //{
            //    loc_sep_arg = loc_arg.Split(loc_sep, StringSplitOptions.None);
            //    if (loc_sep_arg.Length == 1)
            //    {
            //        if (loc_sep_arg[0] == "-install")
            //        {
            //            for (int i = 0; i < arr_path.Length - 1; i++)
            //            {
            //                loc_folder = System.IO.Path.Combine(loc_folder,arr_path[i]);
            //            }

            //            Process.Start(System.IO.Path.Combine(loc_folder,"Adviser.exe"));
            //            Environment.Exit(0);
            //            break;

            //        }
            //    }
            //}

            InitializeComponent();
                        
            ni.Icon = ic;
            ni.Visible = true;
            ni.DoubleClick += new EventHandler(NotyIconDblclck); ;

            mwHeader.Text = App.AppName;

            
            dt_old.Columns.Add("Name");
            dt_old.Columns.Add("Region");

            dt_new.Columns.Add("Name");
            dt_new.Columns.Add("Region");


            Result res = settings_work.LoadSettings();

            if (!res.Status)
            {
                tBlock_status.Dispatcher.Invoke(() =>
                {
                    tBlock_status.Text = "Status: ERROR (" + res.Description + ")";
                    tBlock_status.Foreground = System.Windows.Media.Brushes.Red;
                });
            }
            
            Task.Factory.StartNew(new Action(() => { Refresh(); }));

             

        }

        public async void Refresh()
        {
            while (true)
            {

                await BuildDataSheet();
                //Thread.Sleep(3600000);
                Thread.Sleep(20000);
            }
        }


        public async Task<int> BuildDataSheet()
        {

            try
            {
                Result res = await new oData_Class().GetBirthDayData();

                if (!res.Status)
                {

                    tBlock_status.Dispatcher.Invoke(() =>
                    {
                        tBlock_status.Text = "Status: ERROR (" + res.Description + ")";
                        tBlock_status.Foreground = System.Windows.Media.Brushes.Red;
                    });
                    ;
                    return 0;
                }
                else
                {
                    tBlock_status.Dispatcher.Invoke(() =>
                    {
                        tBlock_status.Text = "Status: Ok";
                        tBlock_status.Foreground = System.Windows.Media.Brushes.Green;
                    });

                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RemoveAllMU<StackPanel>(UpperStack_Today);
                    RemoveAllMU<StackPanel>(UpperStack_Tomorrow);
                    RemoveAllMU<StackPanel>(UpperStack_Week);
                });

                int Days = 0;
                DateTime bd;

                DataRow newdr;

                int CurrDayOfMonth;
                int BDDayOfMonth;

                bool changes = false;
                IEnumerable<DataRow> dr_set;

                dt_new.Clear();

                foreach (DataRow dr in ((DataTable)res.Data).Rows)
                {
                    bd = DateTime.Parse((string)dr["BirthDay"]);
                    CurrDayOfMonth = (DateTime.Today.Month == 2 && DateTime.Today.Day == 29) ? 28 : DateTime.Today.Day;
                    BDDayOfMonth = (bd.Month == 2 && bd.Day == 29) ? 28 : bd.Day;
                    
                    if (DateTime.Today.Month > bd.Month)
                    {
                        continue;       //уже прошел
                    }
                    else if (DateTime.Today.Month == bd.Month)
                    {
                        if (CurrDayOfMonth > BDDayOfMonth)
                        {
                            continue;       //уже прошел
                        }
                        else
                        {
                            Days = bd.Day - DateTime.Today.Day;
                        }
                    }
                    else
                    {
                        if (bd.Month - DateTime.Today.Month == 1)
                        {
                            Days = (DateTime.Today.Month == 2 ? 28 : DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)) -
                            CurrDayOfMonth + BDDayOfMonth;
                        }
                        else
                        {
                            //больше месяца
                            continue;
                        }
                    }

                    
                                        
                    
                    switch (Days)
                    {
                        case 0:
                            newdr = dt_new.NewRow();
                            newdr["Name"] = dr["Name"];
                            newdr["Region"] = "today";
                            dt_new.Rows.Add(newdr);

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                BuildDataRow((string)dr["Name"], (string)dr["BirthDay"], "(" + (string)dr["Age"] + " лет)", (string)dr["Position"], UpperStack_Today);
                            });

                            if (!changes)
                            {
                                dr_set = from drr in dt_old.AsEnumerable()
                                         where drr.Field<string>("Name") == (string)dr["Name"] && drr.Field<string>("Region") == "today"
                                         select drr;
                                if (dr_set.Count() == 0) changes = true;
                            }


                            break;

                        case 1:
                            newdr = dt_new.NewRow();
                            newdr["Name"] = dr["Name"];
                            newdr["Region"] = "tomorrow";
                            dt_new.Rows.Add(newdr);

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                BuildDataRow((string)dr["Name"], (string)dr["BirthDay"], "(" + (string)dr["Age"] + " лет)", (string)dr["Position"], UpperStack_Tomorrow);
                            });

                            if (!changes)
                            {
                                dr_set = from drr in dt_old.AsEnumerable()
                                         where drr.Field<string>("Name") == (string)dr["Name"] && drr.Field<string>("Region") == "tomorrow"
                                         select drr;
                                if (dr_set.Count() == 0) changes = true;
                            }

                            break;

                        default:
                            if (Days > 7) continue;

                            newdr = dt_new.NewRow();
                            newdr["Name"] = dr["Name"];
                            newdr["Region"] = "week";
                            dt_new.Rows.Add(newdr);

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                BuildDataRow((string)dr["Name"], (string)dr["BirthDay"], "(" + (string)dr["Age"] + " лет)", (string)dr["Position"], UpperStack_Week);
                            });

                            if (!changes)
                            {
                                dr_set = from drr in dt_old.AsEnumerable()
                                         where drr.Field<string>("Name") == (string)dr["Name"] && drr.Field<string>("Region") == "week"
                                         select drr;
                                if (dr_set.Count() == 0) changes = true;
                            }

                            break;
                    }

                }

                if (!changes)
                {
                    foreach (DataRow dr in dt_old.Rows)
                    {
                        dr_set =
                            from drr in dt_new.AsEnumerable()
                            where drr.Field<string>("Name") == (string)dr["Name"] && drr.Field<string>("Region") == (string)dr["Region"]
                            select drr;
                        if (dr_set.Count() == 0) changes = true;
                        break;
                    }

                }

                if (changes) ChangesNotyfication();

                dt_old = dt_new;
                dt_new.Clear();

                //BuildDataRow("Собиздулин Зураб Вахитович", "20.01.1980", "40 лет", "начальник отдела и просто хороший парень", UpperStack_Today);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message,App.AppName);

            }


            return 0;
        }

        public void RemoveAllMU<T>(Object loc_in_cnt)
        {

            object cntr_curr;

            if (typeof(T) == typeof(StackPanel))
            {

                while (((StackPanel)loc_in_cnt).Children.Count > 0)
                {
                    cntr_curr = ((StackPanel)loc_in_cnt).Children[((StackPanel)loc_in_cnt).Children.Count - 1];
                    RemoveAllMU<Grid>(cntr_curr);
                    ((StackPanel)loc_in_cnt).Children.Remove((Grid)cntr_curr);

                }

            }

            if (typeof(T) == typeof(Grid))
            {
                while (((Grid)loc_in_cnt).Children.Count > 0)
                {
                    ((Grid)loc_in_cnt).Children.Remove(((Grid)loc_in_cnt).Children[((Grid)loc_in_cnt).Children.Count - 1]);
                }

            }

        }

        public void BuildDataRow(string name, string birthdate, string age, string position, StackPanel curr_stack)
        {
            Double font_common = 12;
            Double font_additional = 10;

            Grid LowerGrid = new Grid();
            LowerGrid.Height = Double.NaN;

            ColumnDefinition col_space = new ColumnDefinition { Width = new GridLength(15) };
            ColumnDefinition col_name_st = new ColumnDefinition { Width = new GridLength(30) };
            ColumnDefinition col_name_end = new ColumnDefinition { Width = new GridLength(200) };
            ColumnDefinition col_name_birthdate = new ColumnDefinition { Width = new GridLength(90) };
            ColumnDefinition col_name_age = new ColumnDefinition();

            LowerGrid.ColumnDefinitions.Add(col_space);
            LowerGrid.ColumnDefinitions.Add(col_name_st);
            LowerGrid.ColumnDefinitions.Add(col_name_end);
            LowerGrid.ColumnDefinitions.Add(col_name_birthdate);
            LowerGrid.ColumnDefinitions.Add(col_name_age);

            LowerGrid.RowDefinitions.Add(new RowDefinition());
            LowerGrid.RowDefinitions.Add(new RowDefinition());

            TextBlock tb_name = new TextBlock();
            tb_name.FontSize = font_common;
            tb_name.FontStyle = FontStyles.Italic;
            tb_name.Text = name;
            tb_name.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(tb_name, 1);
            Grid.SetColumnSpan(tb_name, 2);
            Grid.SetRow(tb_name, 0);


            TextBlock tb_birthdate = new TextBlock();
            tb_birthdate.FontSize = font_common;
            tb_birthdate.FontStyle = FontStyles.Italic;
            tb_birthdate.Text = birthdate;
            Grid.SetColumn(tb_birthdate, 3);
            Grid.SetRow(tb_birthdate, 0);

            TextBlock tb_age = new TextBlock();
            tb_age.FontSize = font_common;
            tb_age.FontStyle = FontStyles.Italic;
            tb_age.Text = age;
            Grid.SetColumn(tb_age, 4);
            Grid.SetRow(tb_age, 0);

            LowerGrid.Children.Add(tb_name);
            LowerGrid.Children.Add(tb_birthdate);
            LowerGrid.Children.Add(tb_age);


            TextBlock tb_pos = new TextBlock();
            tb_pos.FontSize = font_additional;
            tb_pos.FontStyle = FontStyles.Italic;
            tb_pos.Text = position;
            tb_pos.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(tb_pos, 2);
            Grid.SetRow(tb_pos, 1);
            LowerGrid.Children.Add(tb_pos);

            curr_stack.Children.Add(LowerGrid);

        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
            
        }

        public void CloseClicked(object sender, RoutedEventArgs args)
        {
            //this.Visibility= Visibility.Hidden;
            Environment.Exit(0);
        }
        public void OpenSettings(object sender, RoutedEventArgs args)
        {
            new Settings().ShowDialog();
            BuildDataSheet();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void MinimiseWnd(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            ni.Icon = ic;
            ni.Visible = true;
        }

        public void NotyIconDblclck(object Sender, EventArgs e)
        {

            this.Visibility = Visibility.Visible;
            ni.Visible = false;
        }

        void ChangesNotyfication()
        {
            
            ni.ShowBalloonTip(10000, App.AppName, "Изменились данные", ToolTipIcon.Info);
            ni.Icon = ic_warn;
            ni.Visible = true;
            //popup.Dispatcher.Invoke(() => 
            //{
            //    popup.Show();
            //});
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ni.Visible = false;
            ni.Dispose();
        }
    }
    
}
