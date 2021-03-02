using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RQ_boss_stats
{
    static class StaticMethods
    {
        public static string _reference;
        public static string _registryUnits = @"Software\RQ Boss Stats\Units";
        public static string _registrySaves = @"Software\RQ Boss Stats\Saves";
        public static RegistryKey rkeyUnits = Registry.CurrentUser.CreateSubKey(_registryUnits);
        public static RegistryKey rkeySaves = Registry.CurrentUser.CreateSubKey(_registrySaves);
        public static string GetName(string line)
        {
            string[] lines = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] lineName = new string[lines.Length - 2];
            for (int i = 0; i < lineName.Length; i++)
            {
                lineName[i] = lines[i];
            }
            return string.Join(" ", lineName);
        }
        public static void WriteUnitsToRegistry(string[] strs)
        {
            for (int i = 0; i < strs.Length; i++)
            {
                rkeyUnits.SetValue($"boss{i + 1}", strs[i]);
            }
        }
        public static string[] GetUnitsFromRegistry()
        {
            string[] units = new string[rkeyUnits.ValueCount];
            for (int i = 0; i < units.Length; i++)
            {
                units[i] = rkeyUnits.GetValue($"boss{i + 1}").ToString();
            }
            return units;
        }
        public static void MessageInfo(string mess)
        {
            MessageBox.Show(mess, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static string SearchUnitInRegistry_ToString (string unit)
        {
            string str = String.Empty;
            for (int i = 0; i < rkeyUnits.ValueCount; i++)
            {
                if(rkeyUnits.GetValue($"boss{i+1}").ToString().Contains(unit))
                {
                    str = rkeyUnits.GetValue($"boss{i + 1}").ToString();
                    break;
                }
            }
            return str;
        }
    }
}
