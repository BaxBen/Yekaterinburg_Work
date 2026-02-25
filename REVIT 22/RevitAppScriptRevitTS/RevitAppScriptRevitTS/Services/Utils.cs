using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAppScriptRevitTS.Services.SecondCommand
{
    public class Utils
    {
        /// <summary>
        /// Сбор всех экземпляров семейств трубопроводной арматуры имеющих тот же тип, что и указанный элемент
        /// </summary>
        public static List<FamilyInstance> GetPipeAccessory(Document doc, Element elem)
        {
            List<FamilyInstance> list_Levels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipeAccessory)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>().Where(x => x.GetTypeId().IntegerValue == elem.GetTypeId().IntegerValue)
                .ToList();

            return list_Levels;
        }

        /// <summary>
        /// Поиск самого ближнего эксземпляра и приведения его в Dictionary Id XYZ Distance
        /// </summary>
        public static Dictionary<string, object> GetNearDictionary(Element elem, List<FamilyInstance> listFamilyInstance)
        {
            Element elem2 = null;
            XYZ point1 = GetPointXYZ(elem);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in listFamilyInstance)
            {
                if (item.Id.IntegerValue == elem.Id.IntegerValue) continue;

                point2 = GetPointXYZ(item);
                double distance2 = point1.DistanceTo(point2);

                if (elem2 == null)
                {
                    elem2 = item;
                    distance = distance2;
                }
                else if (distance > distance2)
                {
                    distance = distance2;
                    elem2 = item;
                }
            }
            return ConvertElemenetToDictionary(elem2, point2, distance);
        }

        /// <summary>
        /// Поиск самого дальнего эксземпляра и приведения его в Dictionary Id XYZ Distance
        /// </summary>
        public static Dictionary<string, object> GetFarDictionary(Element elem, List<FamilyInstance> listFamilyInstance)
        {
            Element elem2 = null;
            XYZ point1 = GetPointXYZ(elem);
            XYZ point2 = null;
            double distance = 0;
            foreach (var item in listFamilyInstance)
            {
                if (item.Id.IntegerValue == elem.Id.IntegerValue) continue;
                point2 = GetPointXYZ(item);
                double distance2 = point1.DistanceTo(point2);

                if (elem2 == null)
                {
                    elem2 = item;
                    distance = distance2;
                }
                else if (distance < distance2)
                {
                    distance = distance2;
                    elem2 = item;
                }
            }
            return ConvertElemenetToDictionary(elem2, point2, distance);
        }

        /// <summary>
        /// Конвертирование входных данных в Dictionary Id XYZ Distance
        /// </summary>
        public static Dictionary<string, object> ConvertElemenetToDictionary(Element elem, XYZ point, double distance)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = elem.Id.IntegerValue,
                ["XYZ"] = $"X: {point.X:F3} Y: {point.Y:F3} Z: {point.Z:F3}",
                ["Distance"] = Math.Round(UnitUtils.ConvertFromInternalUnits(distance, UnitTypeId.Meters), 2)
            };
        }

        /// <summary>
        /// Возвращает XYZ
        /// </summary>
        public static XYZ GetPointXYZ(Element elem)
        {
            LocationPoint locationPoint = elem.Location as LocationPoint;
            return locationPoint.Point;
        }
    }
}
