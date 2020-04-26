using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Simple.OData.Client;
using System.Xml.Linq;

namespace Adviser
{
    class oData_Class
    {
        public async Task<Result> GetBirthDayData()
        {
            //var client = new ODataClient("http://192.168.1.221:188/zup_1/odata/standard.odata/");

            if (App.app_settings.ServiceAddress == "")
            {
                return new Result { Status=false, Description = "Не указан адрес подключения в настройках!" };
            }

            if (App.app_settings.ServiceName == "")
            {
                return new Result { Status = false, Description = "Не указано имя сервиса в настройках!" };
            }

            var client = new ODataClient("http://" + App.app_settings.ServiceAddress + "/" + App.app_settings.ServiceName + "/odata/standard.odata/");

            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("BirthDay");
            dt.Columns.Add("Age");
            dt.Columns.Add("Position");

            //http://192.168.1.221:188/zup_1/odata/standard.odata/$metadata
            var metadata = await client.GetMetadataDocumentAsync();

            XDocument xDoc = XDocument.Parse(metadata);

            
            DataTable md_lst = new DataTable();
            md_lst.Columns.Add("Name");
            md_lst.Columns.Add("Value");

            RecParseMetadata(xDoc, md_lst);

            bool HasEntity = false;
            foreach (DataRow dr in md_lst.Rows)
            {
                if (dr["Name"].ToString() == "Name" && dr["Value"].ToString() == "Catalog_Сотрудники")
                {
                    HasEntity = true;
                    break;
                }
            }

            if (!HasEntity)
            {
                return new Result { Status = false, Description = "На стороне сирвиса не доступна сущность \"Catalog_Сотрудники\"" };
            }

            HasEntity = false;
            foreach (DataRow dr in md_lst.Rows)
            {
                if (dr["Name"].ToString() == "Name" && dr["Value"].ToString() == "InformationRegister_СостоянияСотрудников")
                {
                    HasEntity = true;
                    break;
                }
            }

            if (!HasEntity)
            {
                return new Result { Status = false, Description = "На стороне сирвиса не доступна сущность \"InformationRegister_СостоянияСотрудников\"" };
            }

            var rg_employees_statuses = await client.For("InformationRegister_СостоянияСотрудников").Function("SliceLast").ExecuteAsArrayAsync();

            List<IDictionary<string, object>> lst_rg_employees_statuses = rg_employees_statuses.ToList();

            lst_rg_employees_statuses.Sort(new GFG());

            var rg_slice_last_statelist = await client.For("InformationRegister_КадроваяИсторияСотрудников_RecordType").Function("SliceLast").ExecuteAsArrayAsync();

            var cat_employees = await client.For("Catalog_Сотрудники").FindEntriesAsync();

            var cat_phys = await client.For("Catalog_ФизическиеЛица").FindEntriesAsync();

            var cat_staff = await client.For("Catalog_Должности").FindEntriesAsync();

            string employee_key;
            string phys_key;
            DataRow new_dr;
            bool f0 = false;

            foreach (var entry in lst_rg_employees_statuses)
            {
                employee_key = entry["Сотрудник_Key"].ToString();

                foreach (var cat_empl_entry in cat_employees)
                {
                    if (employee_key == cat_empl_entry["Ref_Key"].ToString())
                    {
                        phys_key = cat_empl_entry["ФизическоеЛицо_Key"].ToString();

                        f0 = false;
                        foreach (var phys_entry in cat_phys)
                        {
                            if (phys_entry["Ref_Key"].ToString() == phys_key)
                            {
                                if (dt.Select("Name = '" + phys_entry["ФИО"].ToString()+"'").Length > 0)
                                {
                                    f0 = true;
                                    break;
                                };

                                int a = 0;
                                new_dr = dt.NewRow();
                                new_dr["Name"] = phys_entry["ФИО"].ToString();
                                new_dr["BirthDay"] = ((DateTime)phys_entry["ДатаРождения"]).ToString("dd.MM.yyyy");


                                new_dr["Age"] = CalcAge(phys_entry["ДатаРождения"].ToString(), DateTime.Now.ToString("dd.MM.yyyy"));


                                foreach (var rg_state_entry in rg_slice_last_statelist)
                                {
                                    if (rg_state_entry["ФизическоеЛицо_Key"].ToString() == phys_key)
                                    {
                                        foreach (var cat_staff_entry in cat_staff)
                                        {
                                            if (rg_state_entry["Должность_Key"].ToString() == cat_staff_entry["Ref_Key"].ToString())
                                            {
                                                new_dr["Position"] = cat_staff_entry["Description"].ToString();

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }

                                dt.Rows.Add(new_dr);
                                
                                f0 = true;
                                break;
                            }

                        }

                        if (f0) break;
                    }
                }

            }
                        
            DataView dv = dt.DefaultView;
            dv.Sort = "Name asc";
            

            return new Result { Status = true, Data= dv.ToTable() };

        }

        
        int CalcAge(string Date1, string Date2)
        {

            DateTime d1 = DateTime.Parse(Date1);
            DateTime d2 = DateTime.Parse(Date2);

            if (d1.DayOfYear < d2.DayOfYear)
            {
                return d2.Year - d1.Year;
            }
            else
            {
                return (d2.Year - d1.Year)-1;
            }

            
        }

        void RecParseMetadata(object obj, DataTable out_list)
        {
            DataRow r1;

            if (obj.GetType() == typeof(XDocument))
            {
                IEnumerable<XElement> list = ((XDocument)obj).Elements();
                foreach (XElement xmle in list)
                {
                    RecParseMetadata(xmle, out_list);
                }
            }
            
            else if (obj.GetType() == typeof(XElement))
            {
                foreach (XElement child in ((XElement)obj).Elements())
                {
                    
                    foreach (XAttribute attribute in child.Attributes())
                    {
                        if (attribute.Name == "Name")
                        {
                            r1 = out_list.NewRow();
                            r1["Name"] = attribute.Name.ToString();
                            r1["Value"] = attribute.Value.ToString();
                            out_list.Rows.Add(r1);
                        }

                    }


                    RecParseMetadata(child, out_list);
                }

            }


            return;
        }
    }

    class GFG : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            //if (((IDictionary<string, string>)x)["Год"] == "" || ((IDictionary<string, string>)y)["Год"] == "")
            //{
            //    return 0;
            //}

            // CompareTo() method 

            DateTime d1 = DateTime.Parse(((Dictionary<string, object>)x)["Год"].ToString());
            DateTime d2 = DateTime.Parse(((Dictionary<string, object>)y)["Год"].ToString());
            return DateTime.Compare(d2,d1);

        }
    }

}
