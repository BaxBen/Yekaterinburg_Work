using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAppScriptRevitTS.Services;
using RevitAppScriptRevitTS.UI.FirstCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAppScriptRevitTS.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class FirstCommand : CommandBase
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = GetUIDocument(commandData);
            Document doc = GetDocument(commandData);

            IList<Reference> selectedReferences = new List<Reference>();

            try 
            {
                //фильтр для выбора только труб
                selectedReferences = uidoc.Selection.PickObjects(ObjectType.Element, new FilterPipes());
            }
            catch 
            {
                return Result.Succeeded;
            }

            if (selectedReferences.Count == 0)
            {
                TaskDialog.Show("Ошибка", "Не выбран элемент");
                return Result.Succeeded;
            }

            IList<ElementId> selectedPipeIds = selectedReferences.Select(n => n.ElementId).ToList();

            if (ArePipesInSystem(selectedPipeIds, doc))
            {
                if (ArePipesConnected(selectedPipeIds, doc))
                {
                    double pipeLengths = Math.Round(CalculatePipeLengths(selectedPipeIds, doc), 2);

                    FirstCommandWindow window = new FirstCommandWindow();
                    FirstCommandViewModel viewModel = new FirstCommandViewModel(pipeLengths);
                    window.DataContext = viewModel;
                    window.Show();

                    //TaskDialog.Show("Вывод", $"Длина выбранного участка {pipeLengths} мм.");
                    //CopyToClipboard($"{pipeLengths}");
                }
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Копирует значение длины выделенного участка в буфер обмена
        /// </summary>
        public void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        /// <summary>
        /// Проверяет, относятся ли все выбранные трубы к одной системе и зацикленная ли система.
        /// </summary>
        public bool ArePipesInSystem(IList<ElementId> selectedPipeIds, Document doc)
        {
            ElementId firstSystemId = null;

            foreach (ElementId id in selectedPipeIds)
            {
                Element element = doc.GetElement(id);
                Pipe pipe = element as Pipe;
                MEPSystem mepSystem = pipe.MEPSystem;

                if (mepSystem == null)
                {
                    TaskDialog.Show("Ошибка", "Выбрана зацикленная система");
                    return false;
                }
                ;

                if (firstSystemId == null)
                {
                    firstSystemId = mepSystem.Id;
                }
                else if (mepSystem.Id.IntegerValue != firstSystemId.IntegerValue)
                {
                    TaskDialog.Show("Ошибка", "Выбраны разные системы");
                    return false;
                }
            }

            return firstSystemId != null;
        }

        /// <summary>
        /// Проверяет, соединены ли все выбранные трубы между собой через коннекторы.
        /// </summary>
        public bool ArePipesConnected(IList<ElementId> selectedPipeIds, Document doc)
        {
            if (selectedPipeIds.Count == 1) return true;

            Dictionary<Pipe, List<int>> pipeConnections = new Dictionary<Pipe, List<int>>();
            List<int> listPipeIsConecter = new List<int>();

            foreach (ElementId id in selectedPipeIds)
            {
                Pipe pipe = (Pipe)doc.GetElement(id);
                List<int> listConectorInt = GetConnectedElementIds(pipe);
                pipeConnections[pipe] = listConectorInt;
            }

            foreach (var dict1 in pipeConnections)
            {
                foreach (var dict2 in pipeConnections)
                {
                    if (dict1.Key == dict2.Key) break;

                    foreach (var conectorInt in dict1.Value)
                    {
                        if (dict2.Value.Contains(conectorInt))
                        {
                            listPipeIsConecter.Add(conectorInt);
                            break;
                        }
                    }
                }
            }

            foreach (var items in pipeConnections)
            {
                bool chekConect = false;
                foreach (var item in items.Value)
                {
                    if (listPipeIsConecter.Contains(item))
                    {
                        chekConect = true;
                    }
                }
                if (!chekConect)
                {
                    TaskDialog.Show("Ошибка", $"Не все трубы соедены через конектор");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Получает список ID элементов в формате int, с которыми соединена выбранная труба.
        /// </summary>
        private List<int> GetConnectedElementIds(Pipe pipe)
        {
            List<int> listConectorInt = new List<int>();
            ConnectorSet connectors1 = pipe.ConnectorManager.Connectors;
            foreach (Connector conn1 in connectors1)
            {
                // Проверяем все подключенные коннекторы
                foreach (Connector refConnector in conn1.AllRefs)
                {
                    if (!(refConnector.Owner is Pipe) && refConnector.ConnectorType == ConnectorType.End)
                    {
                        listConectorInt.Add(refConnector.Owner.Id.IntegerValue);
                    }
                }
            }
            return listConectorInt;
        }

        /// <summary>
        /// Вычисляет суммарную длину всех выбранных труб в миллиметрах.
        /// </summary>
        private double CalculatePipeLengths(IList<ElementId> selectedPipeIds, Document doc)
        {
            double length = 0;
            foreach (ElementId id in selectedPipeIds)
            {
                Pipe pipe = (Pipe)doc.GetElement(id);
                double lengthParam = pipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                length += UnitUtils.ConvertFromInternalUnits(lengthParam, UnitTypeId.Millimeters);
            }

            return length;
        }
    }
}
