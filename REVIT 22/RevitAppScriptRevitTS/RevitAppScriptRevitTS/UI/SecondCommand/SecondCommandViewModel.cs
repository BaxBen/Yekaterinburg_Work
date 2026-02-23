using Autodesk.Revit.UI;
using RevitAppScriptRevitTS.Commands;
using RevitAppScriptRevitTS.UI.SecondCommand;
using RevitAppScriptRevitTS.Wrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace RevitAppScriptRevitTS.UI
{
    public class SecondCommandViewModel
    {
        public ObservableCollection<ItemModel> Items { get; set; }
        private static ExternalEvent _externalEvent;
        private static SecondCommandEventHandler _handler;

        public SecondCommandViewModel(UIApplication uiapp, SecondCommandWindow thisView, List<Dictionary<string, object>> listElement)
        {
            //Нужно для отображения нашего окна поверх Revit
            var revitHandle = uiapp.MainWindowHandle;
            WindowInteropHelper helper = new WindowInteropHelper(thisView);
            helper.Owner = revitHandle;
            thisView.Topmost = false;
            LoadItems(listElement);
        }

        private void LoadItems(List<Dictionary<string, object>> listElement)
        {
            Items = new ObservableCollection<ItemModel>();
            foreach (var item in listElement)
            {
                if (item != null)
                {
                    int id = Convert.ToInt32(item["Id"]);
                    string xyz = item["XYZ"]?.ToString() ?? string.Empty;
                    double distance = Convert.ToDouble(item["Distance"]); // Изменено с int на double

                    Items.Add(new ItemModel { Id = id, XYZ = xyz, Distance = distance });
                }
            }
        }
    }
}
