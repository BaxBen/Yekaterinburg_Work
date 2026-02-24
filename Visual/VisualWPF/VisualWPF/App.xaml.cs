using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAppScriptRevitTS.UI;
using RevitAppScriptRevitTS.UI.FirstCommand;
using RevitAppScriptRevitTS.UI.SecondCommand;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Documents;

namespace VisualWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private List<Dictionary<string, object>> _listItems = new List<Dictionary<string, object>>();
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AddItems();
            SecondCommandWindow window = new SecondCommandWindow();
            SecondCommandViewModel viewModel = new SecondCommandViewModel(_listItems);
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        private void AddItems()
        {
            _listItems.Add(
                new Dictionary<string, object> { ["Id"] = 1, ["XYZ"] = "X: 123 Y:456 Z: 789", ["Distance"] = 0 }
            );
        }
    }

}
