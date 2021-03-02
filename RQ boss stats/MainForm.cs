using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RQ_boss_stats
{
    public partial class MainForm : Form
    {
        private string
            _boss,
            _patternGlobal,
            _patternKill,
            _patternDamage,
            _copyFile = @"RQ Boss Stats\Temp.txt",
            _copyDirectory = @"RQ Boss Stats";

        int _j,
            _sumDamage,
            _maxDamage,
            _damage,
            _firstI,
            _lastI,
            _lastFound;

        bool _found;

        string[] _strs,
            _damageArr;

        double _DPS;

        DateTime _timeStart,
            _timeEnd,
            dt;
        TimeSpan _timeRespawn;

        public MainForm()
        {
            InitializeComponent();
            WriteAllToRegistry();
            FillUnitList();
            StaticMethods._reference = StaticMethods.rkeySaves.GetValue("last file")?.ToString();
            if (File.Exists(StaticMethods._reference))
            {
                ReferenceBox.Text = StaticMethods._reference;
            }
            else
            {
                StaticMethods._reference = null;
            }
            BossChooseBox.SelectedItem = StaticMethods.rkeySaves.GetValue("last boss")?.ToString();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Registry.CurrentUser.DeleteSubKey(@"Software\RQ Boss Stats\Units", false);
            if (StaticMethods._reference != null)
            {
                StaticMethods.rkeySaves.SetValue("last file", StaticMethods._reference);
            }
            if (_boss != null)
            {
                StaticMethods.rkeySaves.SetValue("last boss", _boss);
            }
            Directory.Delete(_copyDirectory, true);
        }

        private void StartSearch_Button_Click(object sender, EventArgs e)
        {
            Init();
            dataGridView1.Rows.Clear();

            if (File.Exists(StaticMethods._reference) && StaticMethods._reference != null)
            {
                Directory.CreateDirectory(_copyDirectory);
                if (File.Exists(_copyFile))
                {
                    File.Delete(_copyFile);
                }
                File.Copy(StaticMethods._reference, _copyFile);

                if (_timeRespawn == TimeSpan.Zero)
                {
                    _timeRespawn = new TimeSpan(1, 0, 0);
                }
                _strs = File.ReadAllLines(_copyFile);
                _lastFound = 0;
                do
                {
                    Searching();
                } while (_found);

                if (_lastFound == 0)
                {
                    string message = ($"В выбранном файле записи о боссе {_boss} не найдены");
                    StaticMethods.MessageInfo(message);
                }
                CountRespawn();
            }
            else
            {
                StaticMethods.MessageInfo("Файл не найден");
            }
        }
        private void ReferenceBox_DoubleClick(object sender, EventArgs e)
        {
            DialogResult file = openFileDialog1.ShowDialog();
            if (file == DialogResult.OK)
            {
                StaticMethods._reference = openFileDialog1.FileName;
                ReferenceBox.Text = StaticMethods._reference;
            }
        }
        private void BossChooseBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = StaticMethods.SearchUnitInRegistry_ToString(BossChooseBox.SelectedItem.ToString());
            string[] strs = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            _boss = BossChooseBox.SelectedItem.ToString();
            _timeRespawn = new TimeSpan(Convert.ToInt32(strs[strs.Length - 2]), Convert.ToInt32(strs[strs.Length - 1]), 0);
        }

        private void WriteAllToRegistry()
        {
            string[] units = new string[] { "Баксбакуалануксивайе 5 5",
"Древний Энт 1 28",
"Королева Крыс 2 5",
"Хьюго 5 5",
"Деструктор 5 5",
"Архон 4 5",
"Зверомор 4 5",
"Пружинка 8 5",
"Эдвард 4 5",
"Воко 10 5",
"Гигантская Тортолла 5 5",
"Денгур Кровавый топор 5 5",
"Альфа Самец 2 0",
"Советник Остина 2 0",
"Мега Ирекс 2 0",
"Богатый Упырь 2 0",
"Богатый Упырь (Ворлакс) 1 10",
"Пламярык 4 0",
"Королева Термитов 2 0",
"Фараон 2 0",
"Хозяин 2 0"};
            StaticMethods.WriteUnitsToRegistry(units);
        }
        private void FillUnitList()
        {
            string[] units = StaticMethods.GetUnitsFromRegistry();
            for (int i = 0; i < units.Length; i++)
            {
                BossChooseBox.Items.Add(StaticMethods.GetName(units[i]));
            }
        }
        private void Searching()
        {
            bool breakAll = false;
            _found = false;
            _sumDamage = 0;
            _maxDamage = 0;
            for (int i = _lastFound; i < _strs.Length; i++)
            {
                if (Regex.IsMatch(_strs[i], _patternGlobal))
                {
                    _damageArr = Regex.Match(_strs[i], _patternDamage).Value.Split(new char[] { ' ' });
                    _damage = Convert.ToInt32(_damageArr[1]);

                    if (!_found)
                    {
                        if (_damage == 0)
                        {
                            continue;
                        }
                        else
                        {
                            RememberThisTime(i, ref _timeStart, ref _firstI);
                            _found = true;
                        }
                    }
                    else
                    {
                        DateTime tempDT1 = ToDateTime(_strs[i]);
                        _lastFound = i;
                        while (i < _strs.Length && Regex.IsMatch(_strs[i], _patternGlobal))
                        {
                            DateTime tempDT2 = ToDateTime(_strs[i]);

                            if (tempDT2 - tempDT1 > _timeRespawn)
                            {
                                _lastFound = i;
                                i--;
                                RememberThisTime(i, ref _timeEnd, ref _lastI);
                                breakAll = true;
                                break;
                            }
                            else
                            {
                                tempDT1 = tempDT2;
                                i++;
                            }
                        }
                        i--;

                        if (i == _strs.Length - 1)
                        {
                            CheckLastLine(i);
                            _lastFound = i + 1;
                            break;
                        }
                        else
                        {
                            dt = ToDateTime(_strs[i]);
                            if (dt - _timeStart < _timeRespawn)
                            {
                                _lastFound = i + 1;
                                RememberThisTime(i, ref _timeEnd, ref _lastI);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                if (breakAll)
                {
                    break;
                }
            }

            if (_found)
            {
                CheckDamage();
                Output();
            }
        }
        private void CheckDamage()
        {
            for (int i = _firstI; i <= _lastI; i++)
            {
                if (Regex.IsMatch(_strs[i], _patternGlobal))
                {
                    _damageArr = Regex.Match(_strs[i], _patternDamage).Value.Split(new char[] { ' ' });
                    _damage = Convert.ToInt32(_damageArr[1]);
                    _sumDamage += _damage;

                    if (_maxDamage < _damage)
                    {
                        _maxDamage = _damage;
                    }
                }
            }
            _DPS = Math.Round((double)_sumDamage / (_timeEnd - _timeStart).TotalSeconds, 1);
        }
        private void Init()
        {
            StaticMethods._reference = ReferenceBox.Text;
            _j = 0;
            _firstI = default;
            _lastI = default;
            _patternGlobal = @"\d+/\d+ \d{2}:\d{2}:\d{2}'><TD colspan=2>Вы нанесли \d+ урона. Цель: " + _boss;
            _patternDamage = @"нанесли \d+ урона";
            _patternKill = @"\d+/\d+ \d{2}:\d{2}:\d{2}'><TD colspan=2>" + _boss + " погибает.";
            _timeStart = default;
            _timeEnd = default;
        }

        private void Output()
        {
            dataGridView1.Rows.Add();
            dataGridView1[0, _j].Value = _timeStart;
            dataGridView1[1, _j].Value = _timeEnd;
            dataGridView1[2, _j].Value = _timeEnd - _timeStart;
            dataGridView1[3, _j].Value = _sumDamage;
            dataGridView1[4, _j].Value = _maxDamage;
            dataGridView1[5, _j].Value = _DPS;
            _j++;
        }
        private void CheckLastLine(int i)
        {
            if (Regex.IsMatch(_strs[i], _patternGlobal))
            {
                dt = ToDateTime(_strs[i]);
                if (dt - _timeStart < _timeRespawn)
                {
                    _timeEnd = dt;
                    _lastI = i;
                }
                else
                {
                    CheckDamage();
                    Output();
                    _damageArr = Regex.Match(_strs[i], _patternDamage).Value.Split(new char[] { ' ' });
                    _damage = Convert.ToInt32(_damageArr[1]);
                    dataGridView1.Rows.Add();
                    dataGridView1[0, _j].Value = "Далее найдена всего одна запись лога: ";
                    dataGridView1[1, _j].Value = dt;
                    dataGridView1[2, _j].Value = "0";
                    dataGridView1[3, _j].Value = _damage;
                    _found = false;
                }
            }
        }
        private void RememberThisTime(int i, ref DateTime dt, ref int lastOrFirst)
        {
            dt = ToDateTime(_strs[i]);
            lastOrFirst = i;
        }
        private void CountRespawn()
        {
            List<DateTime> bossKills = new List<DateTime>();
            for (int i = 0; i < _strs.Length; i++)
            {
                if (Regex.IsMatch(_strs[i], _patternKill))
                {
                    bossKills.Add(ToDateTime(_strs[i]));
                }
            }

            for (int i = 0; i < bossKills.Count; i++)
            {
                for (int j = 0; j < _j; j++)
                {
                    if (bossKills[i] - Convert.ToDateTime(dataGridView1[1, j].Value) < _timeRespawn && bossKills[i] - Convert.ToDateTime(dataGridView1[1, j].Value) > TimeSpan.Zero)
                    {
                        dataGridView1[1, j].Value = bossKills[i];
                    }
                }
            }

            for (int j = 0; j < _j; j++)
            {
                dataGridView1[6, j].Value = Convert.ToDateTime(dataGridView1[1, j].Value) + _timeRespawn;
            }
        }
        private DateTime ToDateTime(string input)
        {
            string patternTime = @"\d{2}:\d{2}:\d{2}";
            string patternDate = @"\d+/\d+";

            string[] timeStrs = Regex.Match(input, patternTime).Value.Split(new char[] { ':' });
            string[] dateStrs = Regex.Match(input, patternDate).Value.Split(new char[] { '/' });

            int day = Convert.ToInt32(dateStrs[1]);
            int month = Convert.ToInt32(dateStrs[0]);
            int hour = Convert.ToInt32(timeStrs[0]);
            int minute = Convert.ToInt32(timeStrs[1]);
            int second = Convert.ToInt32(timeStrs[2]);

            return new DateTime(2020, month, day, hour, minute, second);
        }
    }
}
