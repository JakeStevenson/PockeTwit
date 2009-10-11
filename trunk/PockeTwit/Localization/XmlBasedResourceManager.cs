using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace PockeTwit.Localization
{
    public static class XmlBasedResourceManager
    {
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();
        private static readonly string Directory;
        private static readonly string NameBase;

        static XmlBasedResourceManager()
        {
            Directory = String.Concat(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "\\Localization\\");
            NameBase = "PockeTwit";
            CultureInfo = CultureInfo.CurrentUICulture;
        }

        private static CultureInfo _cultureInfo;
        public static CultureInfo CultureInfo
        {
            get
            {
                return _cultureInfo;
            }
            set
            {
                if (_cultureInfo != value)
                {
                    _cultureInfo = value;
                    InitLanguageDictionary();
                }
            }
        }

        public static ReadOnlyCollection<System.Globalization.CultureInfo> AvailableCultures()
        {
            List<System.Globalization.CultureInfo> result = new List<CultureInfo>();
            String[] files = System.IO.Directory.GetFiles(Directory, String.Concat(NameBase, "_*.xml"));

            foreach (string fileName in files)
            {
                try
                {
                    string cultureInfoString = fileName.Substring(fileName.LastIndexOf('_') + 1);
                    cultureInfoString = (cultureInfoString.Split('.'))[0];
                    System.Globalization.CultureInfo cultureInfo = new CultureInfo(cultureInfoString);
                    result.Add(cultureInfo);
                }
                catch()
                {
                    //Unable to use this translation file
                }
            }
            return result.AsReadOnly();
        }
        private static string _loadedFile = String.Empty;

        private static void InitLanguageDictionary()
        {
            var fileName = String.Concat(Directory, NameBase, "_", _cultureInfo.Name, ".xml");
            if (!File.Exists(fileName))
            {
                // try fallback to neutral language
                fileName = String.Concat(Directory, NameBase, "_", _cultureInfo.TwoLetterISOLanguageName, ".xml");
                if (!File.Exists(fileName))
                {
                    // give up
                    Cache.Clear();
                    _loadedFile = String.Empty;
                    _cultureInfo = new CultureInfo("en");
                    return;
                }
                else
                {
                    _cultureInfo = new CultureInfo(_cultureInfo.TwoLetterISOLanguageName);
                }
            }
     
            if (_loadedFile == fileName)
                return;

            Cache.Clear();
            var xmlReader = new XmlTextReader(fileName);

            try
            {
                // skip root node
                xmlReader.MoveToContent();

                while (xmlReader.Read())
                {
                    if (xmlReader.MoveToContent() != XmlNodeType.Element) continue;

                    var name = xmlReader.GetAttribute("name");
                    if (String.IsNullOrEmpty(name)) continue;

                    var value = xmlReader.GetAttribute("localized");
                    if (!String.IsNullOrEmpty(value))
                        Cache.Add(name, value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Malformed resource: caught {0} in line {1}", ex.GetType(), xmlReader.LineNumber));
            }
            finally
            {
                xmlReader.Close();
                _loadedFile = fileName;
            }
        }

        public static string GetString(string nameAndDefault)
        {
            return GetString(nameAndDefault, nameAndDefault);
        }

        public static string GetString(string name, string defaultValue)
        {
            string result;
            return !Cache.TryGetValue(name, out result) ? defaultValue : result;
        }

        private static void LocalizeMenuItem(MenuItem item)
        {
            item.Text = GetString(item.Text, item.Text);
            if (item.MenuItems == null) return;
            foreach (MenuItem subItem in item.MenuItems)
            {
                LocalizeMenuItem(subItem);
            }
        }

        public static void LocalizeForm(Control parent)
        {
            if (parent is Form)
            {
                var form = (Form)parent;
                form.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString(form.Text);
                LocalizeMenu(form);
            }
            LocalizeControlAndChildren(parent);
        }
        public static void LocalizeMenu(Form form)
        {
            if (form.Menu == null) return;
            foreach (MenuItem item in form.Menu.MenuItems)
            {
                LocalizeMenuItem(item);
            }
        }

        private static void LocalizeControlAndChildren(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                if ((control is Label) || (control is CheckBox) ||
                    (control is RadioButton) || (control is LinkLabel) ||
                    (control is Form))
                {
                    control.Text = GetString(control.Text, control.Text);
                    continue;
                }
                else if (control is TabControl)
                {
                    foreach (TabPage page in (control as TabControl).TabPages)
                    {
                        page.Text = GetString(page.Text);
                        LocalizeControlAndChildren(page);
                    }
                }
                else if (control is ComboBox)
                {
                    ComboBox box = control as ComboBox;
                    if (box.Tag!= null && box.Tag.ToString() == "AutoLocalize")
                    {
                        for (int i = 0; i < box.Items.Count; i++)
                        {
                            box.Items[i] = GetString(box.Items[i].ToString());
                        }
                    }
                }

                if (control.Controls.Count > 0)
                {
                    LocalizeControlAndChildren(control);
                }
            }
        }
    }
}
