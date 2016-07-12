using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.DirectoryServices;
using System.Management;


namespace ADPatchGroups
{
    public partial class MainWindow : Window
    {
        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            //closes the application
            System.Windows.Application.Current.Shutdown();
        }
        
        public void File_Open_Click(object sender, RoutedEventArgs e)
        {
            //opens .txt files full of system names
            OpenFileDialog getList = new OpenFileDialog();
            getList.Title = "Open Text File";
            getList.FileName = "";
            getList.InitialDirectory = @"C:\";

            DialogResult holdIt = getList.ShowDialog();
            string sardine = getList.FileName;
            listViewPatchGroups.ItemsSource = ReadCSV(sardine);
        }


        public static string GetProperty(SearchResult searchResult, string PropertyName)
        {
            if (searchResult.Properties.Contains(PropertyName))
            {
                return searchResult.Properties[PropertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public string getCurrentPatchGroup(string systemName)
        {

            try{
                DirectoryEntry entry = new DirectoryEntry("LDAP://devgac.gulfaero.com");
                DirectorySearcher searcher = new DirectorySearcher(entry);

                searcher.Filter = "(&(objectCategory=computer)(name=" + systemName + "))";

                SearchResult sResultSet = searcher.FindAll()[0];
                return GetProperty(sResultSet, "extensionAttribute1");
            }

            catch{
                DirectoryEntry entry = new DirectoryEntry("LDAP://gac.gulfaero.com");
                DirectorySearcher searcher = new DirectorySearcher(entry);

                searcher.Filter = "(&(objectCategory=computer)(name=" + systemName + "))";

                SearchResult sResultSet = searcher.FindAll()[0];
                return GetProperty(sResultSet, "extensionAttribute1");
            }

 
        }

        public string getCurrentOwner(string systemName)
        {
            try {
                DirectoryEntry entry = new DirectoryEntry("LDAP://devgac.gulfaero.com");
                DirectorySearcher searcher = new DirectorySearcher(entry);

                searcher.Filter = "(&(objectCategory=computer)(name=" + systemName + "))";

                SearchResult sResultSet = searcher.FindAll()[0];
                return GetProperty(sResultSet, "extensionAttribute2");
            }

            catch
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://gac.gulfaero.com");
                DirectorySearcher searcher = new DirectorySearcher(entry);

                searcher.Filter = "(&(objectCategory=computer)(name=" + systemName + "))";

                SearchResult sResultSet = searcher.FindAll()[0];
                return GetProperty(sResultSet, "extensionAttribute2");

            }


        }

        public class systemToPatch
        {
            public string SystemName { get; set; }
            public string CurrentPatchGroup { get; set; }
            public string [] NewPatchGroup { get; set; }
            public string CurrentOwner { get; set; }
            public string SiteCode { get; set; }

            string DEV = "";
            string PROD = "";

            public systemToPatch(string systemName, string extensionAttribute1, string extensionAttribute2, string newPatchGroup, string Domain) {
                SystemName = systemName;
                CurrentPatchGroup = extensionAttribute1;
                CurrentOwner = extensionAttribute2;
                SiteCode = Domain;
                if (SiteCode == DEV)
                {
                    NewPatchGroup = new string[] { "Wed-Test-" + CurrentOwner + "-W", "Thurs-Test-" + CurrentOwner + "-W", "Fri-Test-" + CurrentOwner + "-W", "Sat-Test-" + CurrentOwner + "-W", "Avail-Test-" + CurrentOwner + "-W" };
                }
                if (SiteCode == PROD)
                {
                    NewPatchGroup = new string[] { "Thurs-Prod-" + CurrentOwner + "-W", "Fri-Prod-" + CurrentOwner + "-W", "Sat-Prod-" + CurrentOwner + "-W", "Avail-Prod-" + CurrentOwner + "-W" };
                }
                if (SiteCode == "error")
                {
                    NewPatchGroup = new string[] { "ERROR" };
                }
                
            }
        }

        public IEnumerable<systemToPatch> ReadCSV(string fileName)
        {
            string[] lines = File.ReadAllLines(System.IO.Path.ChangeExtension(fileName, ".csv"));

            return lines.Select(line =>
            {
                string[] data = line.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                return new systemToPatch(data[0], getCurrentPatchGroup(data[0]), getCurrentOwner(data[0]), "", checkSite(data[0]));
            });
        }

        public string checkSite(string computerName)
         { 
             ManagementScope scp = new ManagementScope(string.Format(@"\\{0}\root\ccm", computerName)); 
             ManagementClass cls = new ManagementClass(scp.Path.Path, "sms_client", null);

            try
            {
                // Get current site code. 
                ManagementBaseObject outSiteParams = cls.InvokeMethod("GetAssignedSite", null, null);


                // Display current site code. 
                return outSiteParams["sSiteCode"].ToString();
            }

            catch
            {

                return "error";

            }


         }


    public MainWindow()
        {
            InitializeComponent();
            listViewPatchGroups.ItemsSource = "";

        }
    }
}

