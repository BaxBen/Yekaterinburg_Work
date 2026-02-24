using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace RevitAppScriptRevitTS.UI.FirstCommand
{
    public class FirstCommandViewModel
    {
        public double Distance { get; set; }
        public RelayCommand CopyDistance { get; }

        public FirstCommandViewModel(double param)
        {
            Distance = param;
            CopyDistance = new RelayCommand(ExecuteCopyDistance, CanExecuteCopyDistance);
        }


        private void ExecuteCopyDistance(object parameter)
        {
            Clipboard.SetText(Distance.ToString());
        }

        private bool CanExecuteCopyDistance(object parameter)
        {
            return true;
        }
    }
}
