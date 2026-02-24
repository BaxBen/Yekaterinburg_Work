using Autodesk.Revit.UI;
using RevitAppScriptRevitTS.Commands;
using RevitAppScriptRevitTS.UI.SecondCommand;
using RevitAppScriptRevitTS.Wrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace RevitAppScriptRevitTS.UI
{
    public class SecondCommandViewModel
    {
        public ObservableCollection<ItemModel> Items { get; set; }

        public SecondCommandViewModel(List<Dictionary<string, object>> listElement)
        {
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
                    double distance = Convert.ToDouble(item["Distance"]);

                    Items.Add(new ItemModel { Id = id, XYZ = xyz, Distance = distance });
                }
            }
        }
    }
}
