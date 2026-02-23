using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAppScriptRevitTS.Commands;
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitAppScriptRevitTS
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalApplication
    {
        private string AssemblyPath => Assembly.GetExecutingAssembly().Location;


        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }


        public Result OnStartup(UIControlledApplication application)
        {
            SetupPanel(application);
            return Result.Succeeded;
        }


        private void SetupPanel(UIControlledApplication application)
        {
            string tabName = "Тестовое задание";
            application.CreateRibbonTab(tabName);

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Инструменты");

            PushButton button1 = CreateButton(
                panel,
                "Задание1",
                "Первое задание",
                typeof(FirstCommand),
                "Запуск первого задания"
                );

            PushButton button2 = CreateButton(
                panel,
                "Задание2",
                "Второе задание",
                typeof(SecondCommand),
                "Запуск второго задания"
                );
        }


        private PushButton CreateButton(RibbonPanel panel, string name, string text, Type commandType,string toolTip)
        {
            PushButtonData buttonData = new PushButtonData(name, text, AssemblyPath, commandType.FullName);

            // Добавляем подсказку
            buttonData.ToolTip = toolTip;

            PushButton button = panel.AddItem(buttonData) as PushButton;

            return button;
        }
    }
}
