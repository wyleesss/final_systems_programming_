using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Helper;
using static System.Windows.Forms.DataFormats;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using final_systems_programming_;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace final_systems_programming_
{
    public partial class Form2 : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        //keys
        KeyHelper kh = new KeyHelper();
        private bool ctrl, shift, capslock;
        public bool patriot { get; set; }
        public string wordsForStop { get; set; }


        private string lang;
        string currentLanguage;


        List<string> keys = new List<string>();
        Dictionary<string, int> keysCount = new Dictionary<string, int>();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int shwCmds);

        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);
        [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint thread);
        public CultureInfo GetCurrentKeyboardLayout()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                uint foregroundProcess = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                int keyboardLayout = GetKeyboardLayout(foregroundProcess).ToInt32() & 0xFFFF;
                return new CultureInfo(keyboardLayout);
            }
            catch (Exception _)
            {
                return new CultureInfo(1033);
            }
        }
        public Form1 form1 = new Form1();



        //process
        private List<ListViewItem> newItems = new List<ListViewItem>();
        private object lockObject = new object();
        Process[] processes = Process.GetProcesses();
        int selectedID = 0;

        public Form2()
        {
            InitializeComponent();
            processListView.Columns.Add("Process Name", 230, HorizontalAlignment.Center);
            processListView.Columns.Add("ID", 100, HorizontalAlignment.Center);
            processListView.Columns.Add("CPU Usage (%)", 150, HorizontalAlignment.Center);
            processListView.Columns.Add("Memory Usage (MB)", 200, HorizontalAlignment.Center);
            processListView.Columns.Add("Start Time", 100, HorizontalAlignment.Center);
            processListView.Columns.Add("Thread Count", 145, HorizontalAlignment.Center);
            processListView.Font = new Font(processListView.Font, FontStyle.Bold);

            update();

            startButton.Enabled = false;
            stopButton.Select();

            kh.KeyDown += Kh_KeyDown;
            kh.KeyUp += Kh_KeyUp;
            //KeyPress.UseMnemonic = false;
        }

        //processes

        private bool ProcessIsBlocked(Process process)
        {
            string processFileName;

            try
            {
                processFileName = process.StartInfo.FileName;
            }
            catch (Exception)
            {
                processFileName = "N/A";
            }

            return Global.Processes.blockedProcessesFileNames.Contains(processFileName) && Global.Processes.blockedProcessesNames.Contains(process.ProcessName);
        }

        public void update()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var currentTime = DateTime.Now.ToLongTimeString();
                var totalProcesses = 0;

                var searchTerm = searchTextBox.Text.ToLower();

                var updatedItems = new List<ListViewItem>();

                Parallel.ForEach(Process.GetProcesses(), process =>
                {
                    if (string.IsNullOrEmpty(searchTerm) || process.ProcessName.ToLower().Contains(searchTerm))
                    {
                        if (ProcessIsBlocked(process))
                        {
                            process.Kill();
                        }

                        var item = new ListViewItem(process.ProcessName);

                        item.SubItems.Add(process.Id.ToString());
                        item.SubItems.Add(GetProcessorTime(process));
                        item.SubItems.Add(GetWorkingSet(process));
                        item.SubItems.Add(GetStartTime(process));
                        item.SubItems.Add(GetThreadCount(process));

                        updatedItems.Add(item);
                        Interlocked.Increment(ref totalProcesses);
                    }
                });

                UpdateListViewItems(updatedItems);

                if (totalProcesses > 0)
                {
                    defineSelectedProcess();
                }

                UpdateUI(currentTime, totalProcesses);
            });
        }

        private void UpdateListViewItems(List<ListViewItem> updatedItems)
        {
            if (processListView.InvokeRequired)
            {
                processListView.BeginInvoke(new Action(() => UpdateListViewItems(updatedItems)));
                return;
            }

            processListView.BeginUpdate();

            foreach (ListViewItem item in processListView.Items.Cast<ListViewItem>().ToArray())
            {
                var matchingNewItem = updatedItems.FirstOrDefault(i => i.Text == item.Text);

                if (matchingNewItem != null)
                {
                    for (int i = 1; i < item.SubItems.Count; i++)
                    {
                        item.SubItems[i].Text = matchingNewItem.SubItems[i].Text;
                    }

                    updatedItems.Remove(matchingNewItem);
                }
                else
                {
                    processListView.Items.Remove(item);
                }
            }

            processListView.Items.AddRange(updatedItems.ToArray());

            processListView.EndUpdate();
        }

        private void UpdateUI(string currentTime, int totalProcesses)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    timeLabel.Text = currentTime;
                    countLabel.Text = totalProcesses.ToString();
                }));
            }
            catch (Exception) { }
        }

        private string GetProcessorTime(Process process)
        {
            try
            {
                return process.TotalProcessorTime.TotalSeconds.ToString("0.00");
            }
            catch (Exception)
            {
                return "N/A";
            }
        }

        private string GetWorkingSet(Process process)
        {
            try
            {
                return (process.WorkingSet64 / (1024 * 1024)).ToString("0.00");
            }
            catch (Exception)
            {
                return "N/A";
            }
        }

        private string GetStartTime(Process process)
        {
            try
            {
                return process.StartTime.ToShortTimeString();
            }
            catch (Exception)
            {
                return "N/A";
            }
        }

        private string GetThreadCount(Process process)
        {
            try
            {
                return process.Threads.Count.ToString();
            }
            catch (Exception)
            {
                return "N/A";
            }
        }

        private void defineSelectedProcess()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(defineSelectedProcess));
                return;
            }

            if (processListView.Items.Count > 0)
            {
                if (processListView.SelectedItems.Count == 0)
                {
                    selectedID = -1;

                    selectedProcessName.ForeColor = Color.Red;
                    selectedProcessName.Text = "(select process by name)";
                    selectedProcessID.Text = "-";
                    selectedProcessCpuUsage.Text = "-";
                    selectedProcessMemoryUsage.Text = "-";
                    selectedProcessThreadCount.Text = "-";
                    selectedProcessFullStartTime.Text = "-";

                    closeProcessButton.Enabled = false;
                    blockProcessButton.Enabled = false;
                }
                else
                {
                    selectedProcessName.ForeColor = Color.Black;

                    UpdateSelectedProcessUI(processListView.SelectedItems[0]);
                    selectedID = processListView.Items.IndexOf(processListView.SelectedItems[0]);

                    closeProcessButton.Enabled = true;
                    blockProcessButton.Enabled = true;
                    blockProcessButton.ForeColor = Color.LightCoral;

                    try
                    {
                        selectedProcessFullStartTime.Text = processes[selectedID].StartTime.ToShortDateString() +
                            "   -   " + processes[selectedID].StartTime.ToLongTimeString();
                    }
                    catch (Exception)
                    {
                        selectedProcessFullStartTime.Text = "N/A";
                    }
                }
            }
        }

        private void UpdateSelectedProcessUI(ListViewItem selectedItem)
        {
            selectedProcessName.Text = selectedItem.SubItems[0].Text;
            selectedProcessID.Text = selectedItem.SubItems[1].Text;
            selectedProcessCpuUsage.Text = selectedItem.SubItems[2].Text + (selectedItem.SubItems[2].Text != "N/A" ? "%" : "");
            selectedProcessMemoryUsage.Text = selectedItem.SubItems[3].Text + "MB";
            selectedProcessThreadCount.Text = selectedItem.SubItems[5].Text;
        }

        private void timerTick(object sender, EventArgs e)
        {
            update();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            this.timer.Interval = Convert.ToInt16(numericUpDown1.Value) * 1000;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            timer.Stop();
            programmState.Text = "stopped";
            programmState.ForeColor = Color.DarkRed;
            programmState.BackColor = Color.HotPink;

            stopButton.Enabled = false;
            startButton.Enabled = true;

            refreshButton.Enabled = false;

            startButton.Select();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            timer.Start();
            programmState.Text = "running";
            programmState.ForeColor = Color.Blue;
            programmState.BackColor = Color.Aqua;

            timer.Stop();
            timer.Start();

            update();

            startButton.Enabled = false;
            stopButton.Enabled = true;

            refreshButton.Enabled = true;

            stopButton.Select();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
        }

        private void processListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            defineSelectedProcess();
        }

        private void closeProcessButton_Click(object sender, EventArgs e)
        {
            try
            {
                processes[selectedID].Kill();

                timer.Stop();
                timer.Start();

                update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error! (Closing Process)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (searchTextBox.Text != "")
            {
                if (stopButton.Enabled)
                {
                    stopButton.Select();
                }
                else
                {
                    startButton.Select();
                }

                update();
            }

            if (stopButton.Enabled)
            {
                stopButton.Select();
            }
            else
            {
                startButton.Select();
            }
        }

        private void clearSearchButton_Click(object sender, EventArgs e)
        {
            if (searchTextBox.Text != "")
            {
                searchTextBox.Clear();

                if (stopButton.Enabled)
                {
                    stopButton.Select();
                }
                else
                {
                    startButton.Select();
                }

                update();
            }

            if (stopButton.Enabled)
            {
                stopButton.Select();
            }
            else
            {
                startButton.Select();
            }
        }

        private void blockProcessButton_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    Global.Processes.blockedProcessesFileNames.Add(processes[selectedID].StartInfo.FileName);
                }
                catch (Exception)
                {
                    Global.Processes.blockedProcessesFileNames.Add("N/A");
                }

                Global.Processes.blockedProcessesNames.Add(processes[selectedID].ProcessName);

                processes[selectedID].Kill();

                timer.Stop();
                timer.Start();

                MessageBox.Show("Successful.", "Info (Blocking Process)", MessageBoxButtons.OK, MessageBoxIcon.Information);

                update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error! (Blocking Process)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Global.Processes.blockedProcessesFileNames.RemoveAt(Global.Processes.blockedProcessesFileNames.Count - 1);
                Global.Processes.blockedProcessesNames.RemoveAt(Global.Processes.blockedProcessesNames.Count - 1);
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            if (stopButton.Enabled)
            {
                stopButton.Select();
            }
            else
            {
                startButton.Select();
            }
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            if (stopButton.Enabled)
            {
                stopButton.Select();
            }
            else
            {
                startButton.Select();
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            if (stopButton.Enabled)
            {
                stopButton.Select();
            }
            else
            {
                startButton.Select();
            }

            update();

            timer.Stop();
            timer.Start();
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (searchTextBox.Text != "")
                {
                    if (stopButton.Enabled)
                    {
                        stopButton.Select();
                    }
                    else
                    {
                        startButton.Select();
                    }

                    update();
                }

                if (stopButton.Enabled)
                {
                    stopButton.Select();
                }
                else
                {
                    startButton.Select();
                }
            }
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }


        //keys
        private void button1_Click(object sender, EventArgs e)
        {
            form1.UpdateListView(Global.Processes.blockedProcessesNames, Global.Processes.blockedProcessesFileNames);
            Global.needExit = false;
            //ShowWindow(this.Handle, 0);
            kh.KeyDown -= Kh_KeyDown;
            kh.KeyUp -= Kh_KeyUp;
            this.Close();


            ShowWindow(form1.Handle, 1);
            Global.needExit = true;
        }
        private void addColorBlue(string word)
        {
            if (KeyPress.Text.Length > 0)
            {
                KeyPress.Select(KeyPress.Text.Length, 1);
            }

            KeyPress.SelectionColor = System.Drawing.Color.Blue;
            KeyPress.AppendText(word);
        }
        private void addColorBlack(string word)
        {
            if (KeyPress.Text.Length > 0)
            {
                KeyPress.Select(KeyPress.Text.Length, 1);
            }

            KeyPress.SelectionColor = System.Drawing.Color.Black;
            KeyPress.AppendText(word);
        }
        private void changeWord(string oldWord, string newWord)
        {
            Color newColor = Color.Red; // Новий колір для нового слова

            int startIndex = 0;
            while (startIndex < KeyPress.TextLength)
            {
                int index = KeyPress.Find(oldWord, startIndex, RichTextBoxFinds.None);
                if (index != -1)
                {
                    KeyPress.Select(index, oldWord.Length);
                    KeyPress.SelectionColor = newColor;
                    KeyPress.SelectedText = newWord;
                    startIndex = index + newWord.Length;
                }
                else
                {
                    break;
                }
            }
        }

        private void Kh_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey) ctrl = false;
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) shift = false;
            if (e.KeyCode == Keys.CapsLock) capslock = false;
            if (patriot)
            {
                changeWord("Россія", "лайно");
                changeWord("Россії", "лайні");
                changeWord("Росія", "лайно");
                changeWord("россія", "лайно");
                changeWord("росія", "лайно");
                changeWord("Путін", "птн-пнх");
                changeWord("Скабєєва", "зливний бачок");
                changeWord("Соловйов", "вечірній мудозвон");
            }

            string check = KeyPress.Text;
            check = check.Replace("Shift", "");
            wordsForStop += " A&N";
            string[] keyWords = wordsForStop.Split(' ');
            //keyWords.Append("A&N");
            string path = "keysReport.txt";
            foreach (string word in keyWords)
            {
                if (check.Contains(word))
                {
                    try
                    {
                        ShowWindow(this.Handle, 1);
                        Global.needExit = true;
                        UpdateUI(DateTime.Now.ToLongTimeString(), processes.Length);
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        using (StreamWriter writer = new StreamWriter(path))
                        {
                            writer.Write(KeyPress.Text);
                            writer.WriteLine();
                            foreach (var i in keysCount)
                            {
                                writer.WriteLine("{0}: {1}", i.Key, i.Value);
                            }
                        }
                    }
                }
            }


        }

        private void Kh_KeyDown(object sender, KeyEventArgs e)
        {
            bool numLock = Console.NumberLock;
            bool capslock2 = Console.CapsLock;

            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) shift = true;
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey) ctrl = true;
            if (e.KeyCode == Keys.CapsLock) capslock = true;

            CultureInfo lang = GetCurrentKeyboardLayout();


            if (lang.ToString() == "uk-UA") //якщо мова введення українська
            {
                if ((e.KeyValue == 81 && shift && !capslock2) || (e.KeyValue == 81 && !shift && capslock2))
                {
                    keys.Add("Й");
                    addColorBlack("Й");
                    keysCount["Й"] = keysCount.ContainsKey("Й") ? keysCount["Й"] + 1 : 1;
                }
                else if ((e.KeyValue == 81 && !shift && !capslock2) || (e.KeyValue == 81 && shift && capslock2))
                {
                    keys.Add("й");
                    addColorBlack("й");
                    keysCount["й"] = keysCount.ContainsKey("й") ? keysCount["й"] + 1 : 1;
                }
                if ((e.KeyValue == 87 && shift && !capslock2) || (e.KeyValue == 87 && !shift && capslock2))
                {
                    keys.Add("Ц");
                    addColorBlack("Ц");
                    keysCount["Ц"] = keysCount.ContainsKey("Ц") ? keysCount["Ц"] + 1 : 1;
                }
                else if ((e.KeyValue == 87 && !shift && !capslock2) || (e.KeyValue == 87 && shift && capslock2))
                {
                    keys.Add("ц");
                    addColorBlack("ц");
                    keysCount["ц"] = keysCount.ContainsKey("ц") ? keysCount["ц"] + 1 : 1;
                }
                if ((e.KeyValue == 69 && shift && !capslock2) || (e.KeyValue == 69 && !shift && capslock2))
                {
                    keys.Add("У");
                    addColorBlack("У");
                    keysCount["У"] = keysCount.ContainsKey("У") ? keysCount["У"] + 1 : 1;
                }
                else if ((e.KeyValue == 69 && !shift && !capslock2) || (e.KeyValue == 69 && shift && capslock2))
                {
                    keys.Add("у");
                    addColorBlack("у");
                    keysCount["у"] = keysCount.ContainsKey("у") ? keysCount["у"] + 1 : 1;
                }
                if ((e.KeyValue == 82 && shift && !capslock2) || (e.KeyValue == 82 && !shift && capslock2))
                {
                    keys.Add("К");
                    addColorBlack("К");
                    keysCount["К"] = keysCount.ContainsKey("К") ? keysCount["К"] + 1 : 1;
                }
                else if ((e.KeyValue == 82 && !shift && !capslock2) || (e.KeyValue == 82 && shift && capslock2))
                {
                    keys.Add("к");
                    addColorBlack("к");
                    keysCount["к"] = keysCount.ContainsKey("к") ? keysCount["к"] + 1 : 1;
                }
                if ((e.KeyValue == 84 && shift && !capslock2) || (e.KeyValue == 84 && !shift && capslock2))
                {
                    keys.Add("Е");
                    addColorBlack("Е");
                    keysCount["Е"] = keysCount.ContainsKey("Е") ? keysCount["Е"] + 1 : 1;
                }
                else if ((e.KeyValue == 84 && !shift && !capslock2) || (e.KeyValue == 84 && shift && capslock2))
                {
                    keys.Add("е");
                    addColorBlack("е");
                    keysCount["е"] = keysCount.ContainsKey("е") ? keysCount["е"] + 1 : 1;
                }
                if ((e.KeyValue == 89 && shift && !capslock2) || (e.KeyValue == 89 && !shift && capslock2))
                {
                    keys.Add("Н");
                    addColorBlack("Н");
                    keysCount["Н"] = keysCount.ContainsKey("Н") ? keysCount["Н"] + 1 : 1;
                }
                else if ((e.KeyValue == 89 && !shift && !capslock2) || (e.KeyValue == 89 && shift && capslock2))
                {
                    keys.Add("н");
                    addColorBlack("н");
                    keysCount["н"] = keysCount.ContainsKey("н") ? keysCount["н"] + 1 : 1;
                }
                if ((e.KeyValue == 85 && shift && !capslock2) || (e.KeyValue == 85 && !shift && capslock2))
                {
                    keys.Add("Г");
                    addColorBlack("Г");
                    keysCount["Г"] = keysCount.ContainsKey("Г") ? keysCount["Г"] + 1 : 1;
                }
                else if ((e.KeyValue == 85 && !shift && !capslock2) || (e.KeyValue == 85 && shift && capslock2))
                {
                    keys.Add("г");
                    addColorBlack("г");
                    keysCount["г"] = keysCount.ContainsKey("г") ? keysCount["г"] + 1 : 1;
                }
                if ((e.KeyValue == 73 && shift && !capslock2) || (e.KeyValue == 73 && !shift && capslock2))
                {
                    keys.Add("Ш");
                    addColorBlack("Ш");
                    keysCount["Ш"] = keysCount.ContainsKey("Ш") ? keysCount["Ш"] + 1 : 1;
                }
                else if ((e.KeyValue == 73 && !shift && !capslock2) || (e.KeyValue == 73 && shift && capslock2))
                {
                    keys.Add("ш");
                    addColorBlack("ш");
                    keysCount["ш"] = keysCount.ContainsKey("ш") ? keysCount["ш"] + 1 : 1;
                }
                if ((e.KeyValue == 79 && shift && !capslock2) || (e.KeyValue == 79 && !shift && capslock2))
                {
                    keys.Add("Щ");
                    addColorBlack("Щ");
                    keysCount["Щ"] = keysCount.ContainsKey("Щ") ? keysCount["Щ"] + 1 : 1;
                }
                else if ((e.KeyValue == 79 && !shift && !capslock2) || (e.KeyValue == 79 && shift && capslock2))
                {
                    keys.Add("щ");
                    addColorBlack("щ");
                    keysCount["щ"] = keysCount.ContainsKey("щ") ? keysCount["щ"] + 1 : 1;
                }
                if ((e.KeyValue == 80 && shift && !capslock2) || (e.KeyValue == 80 && !shift && capslock2))
                {
                    keys.Add("З");
                    addColorBlack("З");
                    keysCount["З"] = keysCount.ContainsKey("З") ? keysCount["З"] + 1 : 1;
                }
                else if ((e.KeyValue == 80 && !shift && !capslock2) || (e.KeyValue == 80 && shift && capslock2))
                {
                    keys.Add("з");
                    addColorBlack("з");
                    keysCount["з"] = keysCount.ContainsKey("з") ? keysCount["з"] + 1 : 1;
                }
                if ((e.KeyValue == 219 && shift && !capslock2) || (e.KeyValue == 219 && !shift && capslock2))
                {
                    keys.Add("Х");
                    addColorBlack("Х");
                    keysCount["Х"] = keysCount.ContainsKey("Х") ? keysCount["Х"] + 1 : 1;
                }
                else if ((e.KeyValue == 219 && !shift && !capslock2) || (e.KeyValue == 219 && shift && capslock2))
                {
                    keys.Add("х");
                    addColorBlack("х");
                    keysCount["х"] = keysCount.ContainsKey("х") ? keysCount["х"] + 1 : 1;
                }
                if ((e.KeyValue == 221 && shift && !capslock2) || (e.KeyValue == 221 && !shift && capslock2))
                {
                    keys.Add("Ї");
                    addColorBlack("Ї");
                    keysCount["Ї"] = keysCount.ContainsKey("Ї") ? keysCount["Ї"] + 1 : 1;
                }
                else if ((e.KeyValue == 221 && !shift && !capslock2) || (e.KeyValue == 221 && shift && capslock2))
                {
                    keys.Add("ї");
                    addColorBlack("ї");
                    keysCount["ї"] = keysCount.ContainsKey("ї") ? keysCount["ї"] + 1 : 1;
                }
                if ((e.KeyValue == 65 && shift && !capslock2) || (e.KeyValue == 65 && !shift && capslock2))
                {
                    keys.Add("Ф");
                    addColorBlack("Ф");
                    keysCount["Ф"] = keysCount.ContainsKey("Ф") ? keysCount["Ф"] + 1 : 1;
                }
                else if ((e.KeyValue == 65 && !shift && !capslock2) || (e.KeyValue == 65 && shift && capslock2))
                {
                    keys.Add("ф");
                    addColorBlack("ф");
                    keysCount["ф"] = keysCount.ContainsKey("ф") ? keysCount["ф"] + 1 : 1;
                }
                if ((e.KeyValue == 83 && shift && !capslock2) || (e.KeyValue == 83 && !shift && capslock2))
                {
                    keys.Add("І");
                    addColorBlack("І");
                    keysCount["І"] = keysCount.ContainsKey("І") ? keysCount["І"] + 1 : 1;
                }
                else if ((e.KeyValue == 83 && !shift && !capslock2) || (e.KeyValue == 83 && shift && capslock2))
                {
                    keys.Add("і");
                    addColorBlack("і");
                    keysCount["і"] = keysCount.ContainsKey("і") ? keysCount["і"] + 1 : 1;
                }
                if ((e.KeyValue == 68 && shift && !capslock2) || (e.KeyValue == 68 && !shift && capslock2))
                {
                    keys.Add("В");
                    addColorBlack("В");
                    keysCount["В"] = keysCount.ContainsKey("В") ? keysCount["В"] + 1 : 1;
                }
                else if ((e.KeyValue == 68 && !shift && !capslock2) || (e.KeyValue == 68 && shift && capslock2))
                {
                    keys.Add("в");
                    addColorBlack("в");
                    keysCount["в"] = keysCount.ContainsKey("в") ? keysCount["в"] + 1 : 1;
                }
                if ((e.KeyValue == 70 && shift && !capslock2) || (e.KeyValue == 70 && !shift && capslock2))
                {
                    keys.Add("А");
                    addColorBlack("А");
                    keysCount["А"] = keysCount.ContainsKey("А") ? keysCount["А"] + 1 : 1;
                }
                else if ((e.KeyValue == 70 && !shift && !capslock2) || (e.KeyValue == 70 && shift && capslock2))
                {
                    keys.Add("а");
                    addColorBlack("а");
                    keysCount["а"] = keysCount.ContainsKey("а") ? keysCount["а"] + 1 : 1;
                }
                if ((e.KeyValue == 71 && shift && !capslock2) || (e.KeyValue == 71 && !shift && capslock2))
                {
                    keys.Add("П");
                    addColorBlack("П");
                    keysCount["П"] = keysCount.ContainsKey("П") ? keysCount["П"] + 1 : 1;
                }
                else if ((e.KeyValue == 71 && !shift && !capslock2) || (e.KeyValue == 71 && shift && capslock2))
                {
                    keys.Add("п");
                    addColorBlack("п");
                    keysCount["п"] = keysCount.ContainsKey("п") ? keysCount["п"] + 1 : 1;
                }
                if ((e.KeyValue == 72 && shift && !capslock2) || (e.KeyValue == 72 && !shift && capslock2))
                {
                    keys.Add("Р");
                    addColorBlack("Р");
                    keysCount["Р"] = keysCount.ContainsKey("Р") ? keysCount["Р"] + 1 : 1;
                }
                else if ((e.KeyValue == 72 && !shift && !capslock2) || (e.KeyValue == 72 && shift && capslock2))
                {
                    keys.Add("р");
                    addColorBlack("р");
                    keysCount["р"] = keysCount.ContainsKey("р") ? keysCount["р"] + 1 : 1;
                }
                if ((e.KeyValue == 74 && shift && !capslock2) || (e.KeyValue == 74 && !shift && capslock2))
                {
                    keys.Add("О");
                    addColorBlack("О");
                    keysCount["О"] = keysCount.ContainsKey("О") ? keysCount["О"] + 1 : 1;
                }
                else if ((e.KeyValue == 74 && !shift && !capslock2) || (e.KeyValue == 74 && shift && capslock2))
                {
                    keys.Add("о");
                    addColorBlack("о");
                    keysCount["о"] = keysCount.ContainsKey("о") ? keysCount["о"] + 1 : 1;
                }
                if ((e.KeyValue == 75 && shift && !capslock2) || (e.KeyValue == 75 && !shift && capslock2))
                {
                    keys.Add("Л");
                    addColorBlack("Л");
                    keysCount["Л"] = keysCount.ContainsKey("Л") ? keysCount["Л"] + 1 : 1;
                }
                else if ((e.KeyValue == 75 && !shift && !capslock2) || (e.KeyValue == 75 && shift && capslock2))
                {
                    keys.Add("л");
                    addColorBlack("л");
                    keysCount["л"] = keysCount.ContainsKey("л") ? keysCount["л"] + 1 : 1;
                }
                if ((e.KeyValue == 76 && shift && !capslock2) || (e.KeyValue == 76 && !shift && capslock2))
                {
                    keys.Add("Д");
                    addColorBlack("Д");
                    keysCount["Д"] = keysCount.ContainsKey("Д") ? keysCount["Д"] + 1 : 1;
                }
                else if ((e.KeyValue == 76 && !shift && !capslock2) || (e.KeyValue == 76 && shift && capslock2))
                {
                    keys.Add("д");
                    addColorBlack("д");
                    keysCount["д"] = keysCount.ContainsKey("д") ? keysCount["д"] + 1 : 1;
                }
                if ((e.KeyValue == 186 && shift && !capslock2) || (e.KeyValue == 186 && !shift && capslock2))
                {
                    keys.Add("Ж");
                    addColorBlack("Ж");
                    keysCount["Ж"] = keysCount.ContainsKey("Ж") ? keysCount["Ж"] + 1 : 1;
                }
                else if ((e.KeyValue == 186 && !shift && !capslock2) || (e.KeyValue == 186 && shift && capslock2))
                {
                    keys.Add("ж");
                    addColorBlack("ж");
                    keysCount["ж"] = keysCount.ContainsKey("ж") ? keysCount["ж"] + 1 : 1;
                }
                if ((e.KeyValue == 222 && shift && !capslock2) || (e.KeyValue == 222 && !shift && capslock2))
                {
                    keys.Add("Є");
                    addColorBlack("Є");
                    keysCount["Є"] = keysCount.ContainsKey("Є") ? keysCount["Є"] + 1 : 1;
                }
                else if ((e.KeyValue == 222 && !shift && !capslock2) || (e.KeyValue == 222 && shift && capslock2))
                {
                    keys.Add("є");
                    addColorBlack("є");
                    keysCount["є"] = keysCount.ContainsKey("є") ? keysCount["є"] + 1 : 1;
                }
                if ((e.KeyValue == 220 && shift && !capslock2) || (e.KeyValue == 220 && !shift && capslock2))
                {
                    keys.Add("/");
                    addColorBlack("/");
                    keysCount["/"] = keysCount.ContainsKey("/") ? keysCount["/"] + 1 : 1;
                }
                else if ((e.KeyValue == 220 && !shift && !capslock2) || (e.KeyValue == 220 && shift && capslock2))
                {
                    keys.Add("\\");
                    addColorBlack("\\");
                    keysCount["\\"] = keysCount.ContainsKey("\\") ? keysCount["\\"] + 1 : 1;
                }
                if ((e.KeyValue == 226 && shift && !capslock2) || (e.KeyValue == 226 && !shift && capslock2))
                {
                    keys.Add("Ґ");
                    addColorBlack("Ґ");
                    keysCount["Ґ"] = keysCount.ContainsKey("Ґ") ? keysCount["Ґ"] + 1 : 1;
                }
                else if ((e.KeyValue == 226 && !shift && !capslock2) || (e.KeyValue == 226 && shift && capslock2))
                {
                    keys.Add("ґ");
                    addColorBlack("ґ");
                    keysCount["ґ"] = keysCount.ContainsKey("ґ") ? keysCount["ґ"] + 1 : 1;
                }
                if ((e.KeyValue == 90 && shift && !capslock2) || (e.KeyValue == 90 && !shift && capslock2))
                {
                    keys.Add("Я");
                    addColorBlack("Я");
                    keysCount["Я"] = keysCount.ContainsKey("Я") ? keysCount["Я"] + 1 : 1;
                }
                else if ((e.KeyValue == 90 && !shift && !capslock2) || (e.KeyValue == 90 && shift && capslock2))
                {
                    keys.Add("я");
                    addColorBlack("я");
                    keysCount["я"] = keysCount.ContainsKey("я") ? keysCount["я"] + 1 : 1;
                }
                if ((e.KeyValue == 88 && shift && !capslock2) || (e.KeyValue == 88 && !shift && capslock2))
                {
                    keys.Add("Ч");
                    addColorBlack("Ч");
                    keysCount["Ч"] = keysCount.ContainsKey("Ч") ? keysCount["Ч"] + 1 : 1;
                }
                else if ((e.KeyValue == 88 && !shift && !capslock2) || (e.KeyValue == 88 && shift && capslock2))
                {
                    keys.Add("ч");
                    addColorBlack("ч");
                    keysCount["ч"] = keysCount.ContainsKey("ч") ? keysCount["ч"] + 1 : 1;
                }
                if ((e.KeyValue == 67 && shift && !capslock2) || (e.KeyValue == 67 && !shift && capslock2))
                {
                    keys.Add("С");
                    addColorBlack("С");
                    keysCount["С"] = keysCount.ContainsKey("С") ? keysCount["С"] + 1 : 1;
                }
                else if ((e.KeyValue == 67 && !shift && !capslock2) || (e.KeyValue == 67 && shift && capslock2))
                {
                    keys.Add("с");
                    addColorBlack("с");
                    keysCount["с"] = keysCount.ContainsKey("с") ? keysCount["с"] + 1 : 1;
                }
                if ((e.KeyValue == 86 && shift && !capslock2) || (e.KeyValue == 86 && !shift && capslock2))
                {
                    keys.Add("М");
                    addColorBlack("М");
                    keysCount["М"] = keysCount.ContainsKey("М") ? keysCount["М"] + 1 : 1;
                }
                else if ((e.KeyValue == 86 && !shift && !capslock2) || (e.KeyValue == 86 && shift && capslock2))
                {
                    keys.Add("м");
                    addColorBlack("м");
                    keysCount["м"] = keysCount.ContainsKey("м") ? keysCount["м"] + 1 : 1;
                }
                if ((e.KeyValue == 66 && shift && !capslock2) || (e.KeyValue == 66 && !shift && capslock2))
                {
                    keys.Add("И");
                    addColorBlack("И");
                    keysCount["И"] = keysCount.ContainsKey("И") ? keysCount["И"] + 1 : 1;
                }
                else if ((e.KeyValue == 66 && !shift && !capslock2) || (e.KeyValue == 66 && shift && capslock2))
                {
                    keys.Add("и");
                    addColorBlack("и");
                    keysCount["и"] = keysCount.ContainsKey("и") ? keysCount["и"] + 1 : 1;
                }
                if ((e.KeyValue == 78 && shift && !capslock2) || (e.KeyValue == 78 && !shift && capslock2))
                {
                    keys.Add("Т");
                    addColorBlack("Т");
                    keysCount["Т"] = keysCount.ContainsKey("Т") ? keysCount["Т"] + 1 : 1;
                }
                else if ((e.KeyValue == 78 && !shift && !capslock2) || (e.KeyValue == 78 && shift && capslock2))
                {
                    keys.Add("т");
                    addColorBlack("т");
                    keysCount["т"] = keysCount.ContainsKey("т") ? keysCount["т"] + 1 : 1;
                }
                if ((e.KeyValue == 77 && shift && !capslock2) || (e.KeyValue == 77 && !shift && capslock2))
                {
                    keys.Add("Ь");
                    addColorBlack("Ь");
                    keysCount["Ь"] = keysCount.ContainsKey("Ь") ? keysCount["Ь"] + 1 : 1;
                }
                else if ((e.KeyValue == 77 && !shift && !capslock2) || (e.KeyValue == 77 && shift && capslock2))
                {
                    keys.Add("ь");
                    addColorBlack("ь");
                    keysCount["ь"] = keysCount.ContainsKey("ь") ? keysCount["ь"] + 1 : 1;
                }
                if ((e.KeyValue == 188 && shift && !capslock2) || (e.KeyValue == 188 && !shift && capslock2))
                {
                    keys.Add("Б");
                    addColorBlack("Б");
                    keysCount["Б"] = keysCount.ContainsKey("Б") ? keysCount["Б"] + 1 : 1;
                }
                else if ((e.KeyValue == 188 && !shift && !capslock2) || (e.KeyValue == 188 && shift && capslock2))
                {
                    keys.Add("б");
                    addColorBlack("б");
                    keysCount["б"] = keysCount.ContainsKey("б") ? keysCount["б"] + 1 : 1;
                }
                if ((e.KeyValue == 190 && shift && !capslock2) || (e.KeyValue == 190 && !shift && capslock2))
                {
                    keys.Add("Ю");
                    addColorBlack("Ю");
                    keysCount["Ю"] = keysCount.ContainsKey("Ю") ? keysCount["Ю"] + 1 : 1;
                }
                else if ((e.KeyValue == 190 && !shift && !capslock2) || (e.KeyValue == 190 && shift && capslock2))
                {
                    keys.Add("ю");
                    addColorBlack("ю");
                    keysCount["ю"] = keysCount.ContainsKey("ю") ? keysCount["ю"] + 1 : 1;
                }
                if ((e.KeyValue == 191 && shift && !capslock2) || (e.KeyValue == 191 && !shift && capslock2))
                {
                    keys.Add(",");
                    addColorBlack(",");
                    keysCount[","] = keysCount.ContainsKey(",") ? keysCount[","] + 1 : 1;
                }
                else if ((e.KeyValue == 191 && !shift && !capslock2) || (e.KeyValue == 191 && shift && capslock2))
                {
                    keys.Add(".");
                    addColorBlack(".");
                    keysCount["."] = keysCount.ContainsKey(".") ? keysCount["."] + 1 : 1;
                }
                if ((e.KeyValue == 192 && shift && !capslock2) || (e.KeyValue == 192 && !shift && capslock2))
                {
                    keys.Add("₴");
                    addColorBlack("₴");
                    keysCount["₴"] = keysCount.ContainsKey("₴") ? keysCount["₴"] + 1 : 1;
                }
                else if ((e.KeyValue == 192 && !shift && !capslock2) || (e.KeyValue == 192 && shift && capslock2))
                {
                    keys.Add("\'");
                    addColorBlack("\'");
                    keysCount["\'"] = keysCount.ContainsKey("\'") ? keysCount["\'"] + 1 : 1;
                }
                //////////////////////////////////////////////////////////////////////////////Далі змін в умовах немає
                if (e.KeyValue == 49 && (shift || capslock2))
                {
                    keys.Add("!");
                    addColorBlack("!");
                    keysCount["!"] = keysCount.ContainsKey("!") ? keysCount["!"] + 1 : 1;
                }
                else if (e.KeyValue == 49 && !shift)
                {
                    keys.Add("1");
                    addColorBlack("1");
                    keysCount["1"] = keysCount.ContainsKey("1") ? keysCount["1"] + 1 : 1;
                }
                if (e.KeyValue == 50 && (shift || capslock2))
                {
                    keys.Add("\"");
                    addColorBlack("\"");
                    keysCount["\""] = keysCount.ContainsKey("\"") ? keysCount["\""] + 1 : 1;
                }
                else if (e.KeyValue == 50 && !shift)
                {
                    keys.Add("2");
                    addColorBlack("2");
                    keysCount["2"] = keysCount.ContainsKey("2") ? keysCount["2"] + 1 : 1;
                }
                if (e.KeyValue == 51 && (shift || capslock2))
                {
                    keys.Add("№");
                    addColorBlack("№");
                    keysCount["№"] = keysCount.ContainsKey("№") ? keysCount["№"] + 1 : 1;
                }
                else if (e.KeyValue == 51 && !shift)
                {
                    keys.Add("3");
                    addColorBlack("3");
                    keysCount["3"] = keysCount.ContainsKey("3") ? keysCount["3"] + 1 : 1;
                }
                if (e.KeyValue == 52 && (shift || capslock2))
                {
                    keys.Add(";");
                    addColorBlack(";");
                    keysCount[";"] = keysCount.ContainsKey(";") ? keysCount[";"] + 1 : 1;
                }
                else if (e.KeyValue == 52 && !shift)
                {
                    keys.Add("4");
                    addColorBlack("4");
                    keysCount["4"] = keysCount.ContainsKey("4") ? keysCount["4"] + 1 : 1;
                }
                if (e.KeyValue == 53 && (shift || capslock2))
                {
                    keys.Add("%");
                    addColorBlack("%");
                    keysCount["%"] = keysCount.ContainsKey("%") ? keysCount["%"] + 1 : 1;
                }
                else if (e.KeyValue == 53 && !shift)
                {
                    keys.Add("5");
                    addColorBlack("5");
                    keysCount["5"] = keysCount.ContainsKey("5") ? keysCount["5"] + 1 : 1;
                }
                if (e.KeyValue == 54 && (shift || capslock2))
                {
                    keys.Add(":");
                    addColorBlack(":");
                    keysCount[":"] = keysCount.ContainsKey(":") ? keysCount[":"] + 1 : 1;
                }
                else if (e.KeyValue == 54 && !shift)
                {
                    keys.Add("6");
                    addColorBlack("6");
                    keysCount["6"] = keysCount.ContainsKey("6") ? keysCount["6"] + 1 : 1;
                }
                if (e.KeyValue == 55 && (shift || capslock2))
                {
                    keys.Add("?");
                    addColorBlack("?");
                    keysCount["?"] = keysCount.ContainsKey("?") ? keysCount["?"] + 1 : 1;
                }
                else if (e.KeyValue == 55 && !shift)
                {
                    keys.Add("7");
                    addColorBlack("7");
                    keysCount["7"] = keysCount.ContainsKey("7") ? keysCount["7"] + 1 : 1;
                }
                if (e.KeyValue == 56 && (shift || capslock2))
                {
                    keys.Add("*");
                    addColorBlack("*");
                    keysCount["*"] = keysCount.ContainsKey("*") ? keysCount["*"] + 1 : 1;
                }
                else if (e.KeyValue == 56 && !shift)
                {
                    keys.Add("8");
                    addColorBlack("8");
                    keysCount["8"] = keysCount.ContainsKey("8") ? keysCount["8"] + 1 : 1;
                }
                if (e.KeyValue == 57 && (shift || capslock2))
                {
                    keys.Add("(");
                    addColorBlack("(");
                    keysCount["("] = keysCount.ContainsKey("(") ? keysCount["("] + 1 : 1;
                }
                else if (e.KeyValue == 57 && !shift)
                {
                    keys.Add("9");
                    addColorBlack("9");
                    keysCount["9"] = keysCount.ContainsKey("9") ? keysCount["9"] + 1 : 1;
                }
                if (e.KeyValue == 48 && (shift || capslock2))
                {
                    keys.Add(")");
                    addColorBlack(")");
                    keysCount[")"] = keysCount.ContainsKey(")") ? keysCount[")"] + 1 : 1;
                }
                else if (e.KeyValue == 48 && !shift)
                {
                    keys.Add("0");
                    addColorBlack("0");
                    keysCount["0"] = keysCount.ContainsKey("0") ? keysCount["0"] + 1 : 1;
                }
                if (e.KeyValue == 189 && (shift || capslock2))
                {
                    keys.Add("_");
                    addColorBlack("_");
                    keysCount["_"] = keysCount.ContainsKey("_") ? keysCount["_"] + 1 : 1;
                }
                else if (e.KeyValue == 189 && !shift)
                {
                    keys.Add("-");
                    addColorBlack("-");
                    keysCount["-"] = keysCount.ContainsKey("-") ? keysCount["-"] + 1 : 1;
                }
                if (e.KeyValue == 187 && (shift || capslock2))
                {
                    keys.Add("+");
                    addColorBlack("+");
                    keysCount["+"] = keysCount.ContainsKey("+") ? keysCount["+"] + 1 : 1;
                }
                else if (e.KeyValue == 187 && !shift)
                {
                    keys.Add("=");
                    addColorBlack("=");
                    keysCount["="] = keysCount.ContainsKey("=") ? keysCount["="] + 1 : 1;
                }

                //Other keyys
                if (e.KeyValue == 32 && (shift || capslock2) || e.KeyValue == 32 && !shift)
                {
                    keys.Add(" ");
                    addColorBlack(" ");
                    keysCount["Space"] = keysCount.ContainsKey("Space") ? keysCount["Space"] + 1 : 1;

                }
                if (e.KeyValue == 27)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Esc"))
                    {
                        keys.Add("Esc");
                        addColorBlue("Esc");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Esc") keys.Add("Esc");
                    keysCount["Esc"] = keysCount.ContainsKey("Esc") ? keysCount["Esc"] + 1 : 1;
                }
                if (e.KeyValue == 162 || e.KeyValue == 163)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Ctrl"))
                    {
                        keys.Add("Ctrl");
                        addColorBlue("Ctrl");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Ctrl") keys.Add("Ctrl");
                    keysCount["Ctrl"] = keysCount.ContainsKey("Ctrl") ? keysCount["Ctrl"] + 1 : 1;
                }
                if (e.KeyValue == 160 || e.KeyValue == 161)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Shift"))
                    {
                        keys.Add("Shift");
                        addColorBlue("Shift");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Shift") keys.Add("Shift");
                    keysCount["Shift"] = keysCount.ContainsKey("Shift") ? keysCount["Shift"] + 1 : 1;
                }
                if (e.KeyValue == 9)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Tab"))
                    {
                        keys.Add("Tab");
                        addColorBlue("Tab");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Tab") keys.Add("Tab");
                    keysCount["Tab"] = keysCount.ContainsKey("Tab") ? keysCount["Tab"] + 1 : 1;
                }
                if (e.KeyValue == 20)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("CapsLock"))
                    {
                        keys.Add("CapsLock");
                        addColorBlue("CapsLock");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "CapsLock") keys.Add("CapsLock");
                    keysCount["CapsLock"] = keysCount.ContainsKey("CapsLock") ? keysCount["CapsLock"] + 1 : 1;
                }
                if (e.KeyValue == 164)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Alt"))
                    {
                        keys.Add("Alt");
                        addColorBlue("Alt");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Alt") keys.Add("Alt");
                    keysCount["Alt"] = keysCount.ContainsKey("Alt") ? keysCount["Alt"] + 1 : 1;
                }
                //Keys F1-F12
                if (e.KeyValue >= 112 && e.KeyValue <= 123)
                {
                    if (keys.Count >= 0 && !keys.Contains(e.KeyCode.ToString()))
                    {
                        keys.Add(e.KeyCode.ToString());
                        addColorBlue(e.KeyCode.ToString());
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != e.KeyCode.ToString()) keys.Add(e.KeyCode.ToString());
                    keysCount[e.KeyCode.ToString()] = keysCount.ContainsKey(e.KeyCode.ToString()) ? keysCount[e.KeyCode.ToString()] + 1 : 1;
                }
                if (e.KeyValue == 8)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Back"))
                    {
                        keys.Add("Back");
                        addColorBlue("Back");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Back") keys.Add("Back");
                    keysCount["Back"] = keysCount.ContainsKey("Back") ? keysCount["Back"] + 1 : 1;
                }
                if (e.KeyValue == 13)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Enter"))
                    {
                        keys.Add("Enter");
                        addColorBlue("Enter");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Enter") keys.Add("Enter");
                    keysCount["Enter"] = keysCount.ContainsKey("Enter") ? keysCount["Enter"] + 1 : 1;
                }
                if (e.KeyValue == 44)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("PrtScn"))
                    {
                        keys.Add("PrtScn");
                        addColorBlue("PrtScn");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "PrtScn") keys.Add("PrtScn");
                    keysCount["PrtScn"] = keysCount.ContainsKey("PrtScn") ? keysCount["PrtScn"] + 1 : 1;
                }
                if (e.KeyValue == 145)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Scroll"))
                    {
                        keys.Add("Scroll");
                        addColorBlue("Scroll");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Scroll") keys.Add("Scroll");
                    keysCount["Scroll"] = keysCount.ContainsKey("Scroll") ? keysCount["Scroll"] + 1 : 1;
                }
                if (e.KeyValue == 19)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Pause"))
                    {
                        keys.Add("Pause");
                        addColorBlue("Pause");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Pause") keys.Add("Pause");
                    keysCount["Pause"] = keysCount.ContainsKey("Pause") ? keysCount["Pause"] + 1 : 1;
                }
                if (e.KeyValue == 45)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Ins"))
                    {
                        keys.Add("Ins");
                        addColorBlue("Ins");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Ins") keys.Add("Ins");
                    keysCount["Ins"] = keysCount.ContainsKey("Ins") ? keysCount["Ins"] + 1 : 1;
                }
                if (e.KeyValue == 46)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Del"))
                    {
                        keys.Add("Del");
                        addColorBlue("Del");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Del") keys.Add("Del");
                    keysCount["Del"] = keysCount.ContainsKey("Del") ? keysCount["Del"] + 1 : 1;
                }
                if (e.KeyValue == 36)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Home"))
                    {
                        keys.Add("Home");
                        addColorBlue("Home");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Home") keys.Add("Home");
                    keysCount["Home"] = keysCount.ContainsKey("Home") ? keysCount["Home"] + 1 : 1;
                }
                if (e.KeyValue == 35)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("End"))
                    {
                        keys.Add("End");
                        addColorBlue("End");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "End") keys.Add("End");
                    keysCount["End"] = keysCount.ContainsKey("End") ? keysCount["End"] + 1 : 1;
                }
                if (e.KeyValue == 33)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("PageUp"))
                    {
                        keys.Add("PageUp");
                        addColorBlue("PageUp");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "PageUp") keys.Add("PageUp");
                    keysCount["PageUp"] = keysCount.ContainsKey("PageUp") ? keysCount["PageUp"] + 1 : 1;
                }
                if (e.KeyValue == 34)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("PageDown"))
                    {
                        keys.Add("PageDown");
                        addColorBlue("PageDown");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "PageDown") keys.Add("PageDown");
                    keysCount["PageDown"] = keysCount.ContainsKey("PageDown") ? keysCount["PageDown"] + 1 : 1;
                }
                if (e.KeyValue == 38)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("↑"))
                    {
                        keys.Add("↑");
                        addColorBlue("↑");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "↑") keys.Add("↑");
                    keysCount["↑"] = keysCount.ContainsKey("↑") ? keysCount["↑"] + 1 : 1;
                }
                if (e.KeyValue == 37)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("←"))
                    {
                        keys.Add("←");
                        addColorBlue("←");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "←") keys.Add("←");
                    keysCount["←"] = keysCount.ContainsKey("←") ? keysCount["←"] + 1 : 1;
                }
                if (e.KeyValue == 39)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("→"))
                    {
                        keys.Add("→");
                        addColorBlue("→");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "→") keys.Add("→");
                    keysCount["→"] = keysCount.ContainsKey("→") ? keysCount["→"] + 1 : 1;
                }
                if (e.KeyValue == 40)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("↓"))
                    {
                        keys.Add("↓");
                        addColorBlue("↓");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "↓") keys.Add("↓");
                    keysCount["↓"] = keysCount.ContainsKey("↓") ? keysCount["↓"] + 1 : 1;
                }
                if (numLock)
                {
                    if (e.KeyValue == 96 && (shift || capslock2))
                    {
                        keys.Add("Ins");
                        addColorBlue("Ins");
                        keysCount["Ins"] = keysCount.ContainsKey("Ins") ? keysCount["Ins"] + 1 : 1;
                    }
                    else if (e.KeyValue == 96 && !shift)
                    {
                        keys.Add("0");
                        addColorBlack("0");
                        keysCount["0"] = keysCount.ContainsKey("0") ? keysCount["0"] + 1 : 1;
                    }
                    if (e.KeyValue == 97 && (shift || capslock2))
                    {
                        keys.Add("End");
                        addColorBlue("End");
                        keysCount["End"] = keysCount.ContainsKey("End") ? keysCount["End"] + 1 : 1;
                    }
                    else if (e.KeyValue == 97 && !shift)
                    {
                        keys.Add("1");
                        addColorBlack("1");
                        keysCount["1"] = keysCount.ContainsKey("1") ? keysCount["1"] + 1 : 1;
                    }
                    if (e.KeyValue == 98 && (shift || capslock2))
                    {
                        keys.Add("↓");
                        addColorBlue("↓");
                        keysCount["↓"] = keysCount.ContainsKey("↓") ? keysCount["↓"] + 1 : 1;
                    }
                    else if (e.KeyValue == 98 && !shift)
                    {
                        keys.Add("2");
                        addColorBlack("2");
                        keysCount["2"] = keysCount.ContainsKey("2") ? keysCount["2"] + 1 : 1;
                    }
                    if (e.KeyValue == 99 && (shift || capslock2))
                    {
                        keys.Add("PageDown");
                        addColorBlue("PageDown");
                        keysCount["PageDown"] = keysCount.ContainsKey("PageDown") ? keysCount["PageDown"] + 1 : 1;
                    }
                    else if (e.KeyValue == 99 && !shift)
                    {
                        keys.Add("3");
                        addColorBlack("3");
                        keysCount["3"] = keysCount.ContainsKey("3") ? keysCount["3"] + 1 : 1;
                    }
                    if (e.KeyValue == 100 && (shift || capslock2))
                    {
                        keys.Add("←");
                        addColorBlue("←");
                        keysCount["←"] = keysCount.ContainsKey("←") ? keysCount["←"] + 1 : 1;
                    }
                    else if (e.KeyValue == 100 && !shift)
                    {
                        keys.Add("4");
                        addColorBlack("4");
                        keysCount["4"] = keysCount.ContainsKey("4") ? keysCount["4"] + 1 : 1;
                    }
                    //При натисканні Shift+NumPad5 з активованим NumLock потрібно детальніше перевірити обробку цієї ситуації
                    if (e.KeyValue == 101 && (shift || capslock2))
                    {
                        keys.Add("Clear");
                        addColorBlue("Clear");
                        keysCount["Clear"] = keysCount.ContainsKey("Clear") ? keysCount["Clear"] + 1 : 1;
                    }
                    else if (e.KeyValue == 101 && !shift)
                    {
                        keys.Add("5");
                        addColorBlack("5");
                        keysCount["5"] = keysCount.ContainsKey("5") ? keysCount["5"] + 1 : 1;
                    }
                    if (e.KeyValue == 102 && (shift || capslock2))
                    {
                        keys.Add("→");
                        addColorBlue("→");
                        keysCount["→"] = keysCount.ContainsKey("→") ? keysCount["→"] + 1 : 1;
                    }
                    else if (e.KeyValue == 102 && !shift)
                    {
                        keys.Add("6");
                        addColorBlack("6");
                        keysCount["6"] = keysCount.ContainsKey("6") ? keysCount["6"] + 1 : 1;
                    }
                    if (e.KeyValue == 103 && (shift || capslock2))
                    {
                        keys.Add("Home");
                        addColorBlue("Home");
                        keysCount["Home"] = keysCount.ContainsKey("Home") ? keysCount["Home"] + 1 : 1;
                    }
                    else if (e.KeyValue == 103 && !shift)
                    {
                        keys.Add("7");
                        addColorBlack("7");
                        keysCount["7"] = keysCount.ContainsKey("7") ? keysCount["7"] + 1 : 1;
                    }
                    if (e.KeyValue == 104 && (shift || capslock2))
                    {
                        keys.Add("↑");
                        addColorBlue("↑");
                        keysCount["↑"] = keysCount.ContainsKey("↑") ? keysCount["↑"] + 1 : 1;
                    }
                    else if (e.KeyValue == 104 && !shift)
                    {
                        keys.Add("8");
                        addColorBlack("8");
                        keysCount["8"] = keysCount.ContainsKey("8") ? keysCount["8"] + 1 : 1;
                    }
                    if (e.KeyValue == 105 && (shift || capslock2))
                    {
                        keys.Add("PageUp");
                        addColorBlue("PageUp");
                        keysCount["PageUp"] = keysCount.ContainsKey("PageUp") ? keysCount["PageUp"] + 1 : 1;
                    }
                    else if (e.KeyValue == 105 && !shift)
                    {
                        keys.Add("9");
                        addColorBlack("9");
                        keysCount["9"] = keysCount.ContainsKey("9") ? keysCount["9"] + 1 : 1;
                    }
                    if (e.KeyValue == 111)
                    {
                        keys.Add("/");
                        addColorBlack("/");
                        keysCount["/"] = keysCount.ContainsKey("/") ? keysCount["/"] + 1 : 1;
                    }
                    if (e.KeyValue == 106)
                    {
                        keys.Add("*");
                        addColorBlack("*");
                        keysCount["*"] = keysCount.ContainsKey("*") ? keysCount["*"] + 1 : 1;
                    }
                    if (e.KeyValue == 107)
                    {
                        keys.Add("+");
                        addColorBlack("+");
                        keysCount["+"] = keysCount.ContainsKey("+") ? keysCount["+"] + 1 : 1;
                    }
                    if (e.KeyValue == 109)
                    {
                        keys.Add("-");
                        addColorBlack("-");
                        keysCount["-"] = keysCount.ContainsKey("-") ? keysCount["-"] + 1 : 1;
                    }
                    if (e.KeyValue == 110)
                    {
                        keys.Add("Del");
                        addColorBlue("Del");
                        keysCount["Del"] = keysCount.ContainsKey("Del") ? keysCount["Del"] + 1 : 1;
                    }

                }
                else if (!numLock)
                {
                    if (e.KeyValue == 12 && (shift || capslock2))
                    {
                        if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Clear"))
                        {
                            keys.Add("Clear");
                            addColorBlue("Clear");
                        }
                        else if (keys.Count > 0 && keys[keys.Count - 1] != "Clear") keys.Add("Clear");
                        keysCount["Clear"] = keysCount.ContainsKey("Clear") ? keysCount["Clear"] + 1 : 1;
                    }
                    else if (e.KeyValue == 12 && !shift)
                    {
                        keys.Add("5");
                        addColorBlack("5");
                        keysCount["5"] = keysCount.ContainsKey("5") ? keysCount["5"] + 1 : 1;
                    }
                    if (e.KeyValue == 110)
                    {
                        keys.Add("Del");
                        addColorBlue("Del");
                        keysCount["Del"] = keysCount.ContainsKey("Del") ? keysCount["Del"] + 1 : 1;
                    }
                    if (e.KeyValue == 111)
                    {
                        keys.Add("/");
                        addColorBlack("/");
                        keysCount["/"] = keysCount.ContainsKey("/") ? keysCount["/"] + 1 : 1;
                    }
                    if (e.KeyValue == 106)
                    {
                        keys.Add("*");
                        addColorBlack("*");
                        keysCount["*"] = keysCount.ContainsKey("*") ? keysCount["*"] + 1 : 1;
                    }
                    if (e.KeyValue == 107)
                    {
                        keys.Add("+");
                        addColorBlack("+");
                        keysCount["+"] = keysCount.ContainsKey("+") ? keysCount["+"] + 1 : 1;
                    }
                    if (e.KeyValue == 109)
                    {
                        keys.Add("-");
                        addColorBlack("-");
                        keysCount["-"] = keysCount.ContainsKey("-") ? keysCount["-"] + 1 : 1;
                    }
                }
                //keys.Add(e.KeyCode);
                //keys.Add(e.KeyValue);
                //keys.Add(numLock);

            }
            if (lang.ToString() == "en-US") //якщо мова введення англійська
            {
                if ((e.KeyValue == 81 && shift && !capslock2) || (e.KeyValue == 81 && !shift && capslock2))
                {
                    keys.Add("Q");
                    addColorBlack("Q");
                    keysCount["Q"] = keysCount.ContainsKey("Q") ? keysCount["Q"] + 1 : 1;
                }
                else if ((e.KeyValue == 81 && !shift && !capslock2) || (e.KeyValue == 81 && shift && capslock2))
                {
                    keys.Add("q");
                    addColorBlack("q");
                    keysCount["q"] = keysCount.ContainsKey("q") ? keysCount["q"] + 1 : 1;
                }
                if ((e.KeyValue == 87 && shift && !capslock2) || (e.KeyValue == 87 && !shift && capslock2))
                {
                    keys.Add("W");
                    addColorBlack("W");
                    keysCount["W"] = keysCount.ContainsKey("W") ? keysCount["W"] + 1 : 1;
                }
                else if ((e.KeyValue == 87 && !shift && !capslock2) || (e.KeyValue == 87 && shift && capslock2))
                {
                    keys.Add("w");
                    addColorBlack("w");
                    keysCount["w"] = keysCount.ContainsKey("w") ? keysCount["w"] + 1 : 1;
                }
                if ((e.KeyValue == 69 && shift && !capslock2) || (e.KeyValue == 69 && !shift && capslock2))
                {
                    keys.Add("E");
                    addColorBlack("E");
                    keysCount["E"] = keysCount.ContainsKey("E") ? keysCount["E"] + 1 : 1;
                }
                else if ((e.KeyValue == 69 && !shift && !capslock2) || (e.KeyValue == 69 && shift && capslock2))
                {
                    keys.Add("e");
                    addColorBlack("e");
                    keysCount["e"] = keysCount.ContainsKey("e") ? keysCount["e"] + 1 : 1;
                }
                if ((e.KeyValue == 82 && shift && !capslock2) || (e.KeyValue == 82 && !shift && capslock2))
                {
                    keys.Add("R");
                    addColorBlack("R");
                    keysCount["R"] = keysCount.ContainsKey("R") ? keysCount["R"] + 1 : 1;
                }
                else if ((e.KeyValue == 82 && !shift && !capslock2) || (e.KeyValue == 82 && shift && capslock2))
                {
                    keys.Add("r");
                    addColorBlack("r");
                    keysCount["r"] = keysCount.ContainsKey("r") ? keysCount["r"] + 1 : 1;
                }
                if ((e.KeyValue == 84 && shift && !capslock2) || (e.KeyValue == 84 && !shift && capslock2))
                {
                    keys.Add("T");
                    addColorBlack("T");
                    keysCount["T"] = keysCount.ContainsKey("T") ? keysCount["T"] + 1 : 1;
                }
                else if ((e.KeyValue == 84 && !shift && !capslock2) || (e.KeyValue == 84 && shift && capslock2))
                {
                    keys.Add("t");
                    addColorBlack("t");
                    keysCount["t"] = keysCount.ContainsKey("t") ? keysCount["t"] + 1 : 1;
                }
                if ((e.KeyValue == 89 && shift && !capslock2) || (e.KeyValue == 89 && !shift && capslock2))
                {
                    keys.Add("Y");
                    addColorBlack("Y");
                    keysCount["Y"] = keysCount.ContainsKey("Y") ? keysCount["Y"] + 1 : 1;
                }
                else if ((e.KeyValue == 89 && !shift && !capslock2) || (e.KeyValue == 89 && shift && capslock2))
                {
                    keys.Add("y");
                    addColorBlack("y");
                    keysCount["y"] = keysCount.ContainsKey("y") ? keysCount["y"] + 1 : 1;
                }
                if ((e.KeyValue == 85 && shift && !capslock2) || (e.KeyValue == 85 && !shift && capslock2))
                {
                    keys.Add("U");
                    addColorBlack("U");
                    keysCount["U"] = keysCount.ContainsKey("U") ? keysCount["U"] + 1 : 1;
                }
                else if ((e.KeyValue == 85 && !shift && !capslock2) || (e.KeyValue == 85 && shift && capslock2))
                {
                    keys.Add("u");
                    addColorBlack("u");
                    keysCount["u"] = keysCount.ContainsKey("u") ? keysCount["u"] + 1 : 1;
                }
                if ((e.KeyValue == 73 && shift && !capslock2) || (e.KeyValue == 73 && !shift && capslock2))
                {
                    keys.Add("I");
                    addColorBlack("I");
                    keysCount["I"] = keysCount.ContainsKey("I") ? keysCount["I"] + 1 : 1;
                }
                else if ((e.KeyValue == 73 && !shift && !capslock2) || (e.KeyValue == 73 && shift && capslock2))
                {
                    keys.Add("i");
                    addColorBlack("i");
                    keysCount["i"] = keysCount.ContainsKey("i") ? keysCount["i"] + 1 : 1;
                }
                if ((e.KeyValue == 79 && shift && !capslock2) || (e.KeyValue == 79 && !shift && capslock2))
                {
                    keys.Add("O");
                    addColorBlack("O");
                    keysCount["O"] = keysCount.ContainsKey("O") ? keysCount["O"] + 1 : 1;
                }
                else if ((e.KeyValue == 79 && !shift && !capslock2) || (e.KeyValue == 79 && shift && capslock2))
                {
                    keys.Add("o");
                    addColorBlack("o");
                    keysCount["o"] = keysCount.ContainsKey("o") ? keysCount["o"] + 1 : 1;
                }
                if ((e.KeyValue == 80 && shift && !capslock2) || (e.KeyValue == 80 && !shift && capslock2))
                {
                    keys.Add("P");
                    addColorBlack("P");
                    keysCount["P"] = keysCount.ContainsKey("P") ? keysCount["P"] + 1 : 1;
                }
                else if ((e.KeyValue == 80 && !shift && !capslock2) || (e.KeyValue == 80 && shift && capslock2))
                {
                    keys.Add("p");
                    addColorBlack("p");
                    keysCount["p"] = keysCount.ContainsKey("p") ? keysCount["p"] + 1 : 1;
                }
                if ((e.KeyValue == 219 && shift && !capslock2) || (e.KeyValue == 219 && !shift && capslock2))
                {
                    keys.Add("{");
                    addColorBlack("{");
                    keysCount["{"] = keysCount.ContainsKey("{") ? keysCount["{"] + 1 : 1;
                }
                else if ((e.KeyValue == 219 && !shift && !capslock2) || (e.KeyValue == 219 && shift && capslock2))
                {
                    keys.Add("[");
                    addColorBlack("[");
                    keysCount["["] = keysCount.ContainsKey("[") ? keysCount["["] + 1 : 1;
                }
                if ((e.KeyValue == 221 && shift && !capslock2) || (e.KeyValue == 221 && !shift && capslock2))
                {
                    keys.Add("}");
                    addColorBlack("}");
                    keysCount["}"] = keysCount.ContainsKey("}") ? keysCount["}"] + 1 : 1;
                }
                else if ((e.KeyValue == 221 && !shift && !capslock2) || (e.KeyValue == 221 && shift && capslock2))
                {
                    keys.Add("]");
                    addColorBlack("]");
                    keysCount["]"] = keysCount.ContainsKey("]") ? keysCount["]"] + 1 : 1;
                }
                if ((e.KeyValue == 65 && shift && !capslock2) || (e.KeyValue == 65 && !shift && capslock2))
                {
                    keys.Add("A");
                    addColorBlack("A");
                    keysCount["A"] = keysCount.ContainsKey("A") ? keysCount["A"] + 1 : 1;
                }
                else if ((e.KeyValue == 65 && !shift && !capslock2) || (e.KeyValue == 65 && shift && capslock2))
                {
                    keys.Add("a");
                    addColorBlack("a");
                    keysCount["a"] = keysCount.ContainsKey("a") ? keysCount["a"] + 1 : 1;
                }
                if ((e.KeyValue == 83 && shift && !capslock2) || (e.KeyValue == 83 && !shift && capslock2))
                {
                    keys.Add("S");
                    addColorBlack("S");
                    keysCount["S"] = keysCount.ContainsKey("S") ? keysCount["S"] + 1 : 1;
                }
                else if ((e.KeyValue == 83 && !shift && !capslock2) || (e.KeyValue == 83 && shift && capslock2))
                {
                    keys.Add("s");
                    addColorBlack("s");
                    keysCount["s"] = keysCount.ContainsKey("s") ? keysCount["s"] + 1 : 1;
                }
                if ((e.KeyValue == 68 && shift && !capslock2) || (e.KeyValue == 68 && !shift && capslock2))
                {
                    keys.Add("D");
                    addColorBlack("D");
                    keysCount["D"] = keysCount.ContainsKey("D") ? keysCount["D"] + 1 : 1;
                }
                else if ((e.KeyValue == 68 && !shift && !capslock2) || (e.KeyValue == 68 && shift && capslock2))
                {
                    keys.Add("d");
                    addColorBlack("d");
                    keysCount["d"] = keysCount.ContainsKey("d") ? keysCount["d"] + 1 : 1;
                }
                if ((e.KeyValue == 70 && shift && !capslock2) || (e.KeyValue == 70 && !shift && capslock2))
                {
                    keys.Add("F");
                    addColorBlack("F");
                    keysCount["F"] = keysCount.ContainsKey("F") ? keysCount["F"] + 1 : 1;
                }
                else if ((e.KeyValue == 70 && !shift && !capslock2) || (e.KeyValue == 70 && shift && capslock2))
                {
                    keys.Add("f");
                    addColorBlack("f");
                    keysCount["f"] = keysCount.ContainsKey("f") ? keysCount["f"] + 1 : 1;
                }
                if ((e.KeyValue == 71 && shift && !capslock2) || (e.KeyValue == 71 && !shift && capslock2))
                {
                    keys.Add("G");
                    addColorBlack("G");
                    keysCount["G"] = keysCount.ContainsKey("G") ? keysCount["G"] + 1 : 1;
                }
                else if ((e.KeyValue == 71 && !shift && !capslock2) || (e.KeyValue == 71 && shift && capslock2))
                {
                    keys.Add("g");
                    addColorBlack("g");
                    keysCount["g"] = keysCount.ContainsKey("g") ? keysCount["g"] + 1 : 1;
                }
                if ((e.KeyValue == 72 && shift && !capslock2) || (e.KeyValue == 72 && !shift && capslock2))
                {
                    keys.Add("H");
                    addColorBlack("H");
                    keysCount["H"] = keysCount.ContainsKey("H") ? keysCount["H"] + 1 : 1;
                }
                else if ((e.KeyValue == 72 && !shift && !capslock2) || (e.KeyValue == 72 && shift && capslock2))
                {
                    keys.Add("h");
                    addColorBlack("h");
                    keysCount["h"] = keysCount.ContainsKey("h") ? keysCount["h"] + 1 : 1;
                }
                if ((e.KeyValue == 74 && shift && !capslock2) || (e.KeyValue == 74 && !shift && capslock2))
                {
                    keys.Add("J");
                    addColorBlack("J");
                    keysCount["J"] = keysCount.ContainsKey("J") ? keysCount["J"] + 1 : 1;
                }
                else if ((e.KeyValue == 74 && !shift && !capslock2) || (e.KeyValue == 74 && shift && capslock2))
                {
                    keys.Add("j");
                    addColorBlack("j");
                    keysCount["j"] = keysCount.ContainsKey("j") ? keysCount["j"] + 1 : 1;
                }
                if ((e.KeyValue == 75 && shift && !capslock2) || (e.KeyValue == 75 && !shift && capslock2))
                {
                    keys.Add("K");
                    addColorBlack("K");
                    keysCount["K"] = keysCount.ContainsKey("K") ? keysCount["K"] + 1 : 1;
                }
                else if ((e.KeyValue == 75 && !shift && !capslock2) || (e.KeyValue == 75 && shift && capslock2))
                {
                    keys.Add("k");
                    addColorBlack("k");
                    keysCount["k"] = keysCount.ContainsKey("k") ? keysCount["k"] + 1 : 1;
                }
                if ((e.KeyValue == 76 && shift && !capslock2) || (e.KeyValue == 76 && !shift && capslock2))
                {
                    keys.Add("L");
                    addColorBlack("L");
                    keysCount["L"] = keysCount.ContainsKey("L") ? keysCount["L"] + 1 : 1;
                }
                else if ((e.KeyValue == 76 && !shift && !capslock2) || (e.KeyValue == 76 && shift && capslock2))
                {
                    keys.Add("l");
                    addColorBlack("l");
                    keysCount["l"] = keysCount.ContainsKey("l") ? keysCount["l"] + 1 : 1;
                }
                if ((e.KeyValue == 186 && shift && !capslock2) || (e.KeyValue == 186 && !shift && capslock2))
                {
                    keys.Add(":");
                    addColorBlack(":");
                    keysCount[":"] = keysCount.ContainsKey(":") ? keysCount[":"] + 1 : 1;
                }
                else if ((e.KeyValue == 186 && !shift && !capslock2) || (e.KeyValue == 186 && shift && capslock2))
                {
                    keys.Add(";");
                    addColorBlack(";");
                    keysCount[";"] = keysCount.ContainsKey(";") ? keysCount[";"] + 1 : 1;
                }
                if ((e.KeyValue == 222 && shift && !capslock2) || (e.KeyValue == 222 && !shift && capslock2))
                {
                    keys.Add("\"");
                    addColorBlack("\"");
                    keysCount["\""] = keysCount.ContainsKey("\"") ? keysCount["\""] + 1 : 1;
                }
                else if ((e.KeyValue == 222 && !shift && !capslock2) || (e.KeyValue == 222 && shift && capslock2))
                {
                    keys.Add("\'");
                    addColorBlack("\'");
                    keysCount["\'"] = keysCount.ContainsKey("\'") ? keysCount["\'"] + 1 : 1;
                }
                if ((e.KeyValue == 220 && shift && !capslock2) || (e.KeyValue == 220 && !shift && capslock2))
                {
                    keys.Add("|");
                    addColorBlack("|");
                    keysCount["|"] = keysCount.ContainsKey("|") ? keysCount["|"] + 1 : 1;
                }
                else if ((e.KeyValue == 220 && !shift && !capslock2) || (e.KeyValue == 220 && shift && capslock2))
                {
                    keys.Add("\\");
                    addColorBlack("\\");
                    keysCount["\\"] = keysCount.ContainsKey("\\") ? keysCount["\\"] + 1 : 1;
                }
                if ((e.KeyValue == 226 && shift && !capslock2) || (e.KeyValue == 226 && !shift && capslock2))
                {
                    keys.Add("|");
                    addColorBlack("|");
                    keysCount["|"] = keysCount.ContainsKey("|") ? keysCount["|"] + 1 : 1;
                }
                else if ((e.KeyValue == 226 && !shift && !capslock2) || (e.KeyValue == 226 && shift && capslock2))
                {
                    keys.Add("\\");
                    addColorBlack("\\");
                    keysCount["\\"] = keysCount.ContainsKey("\\") ? keysCount["\\"] + 1 : 1;
                }
                if ((e.KeyValue == 90 && shift && !capslock2) || (e.KeyValue == 90 && !shift && capslock2))
                {
                    keys.Add("Z");
                    addColorBlack("Z");
                    keysCount["Z"] = keysCount.ContainsKey("Z") ? keysCount["Z"] + 1 : 1;
                }
                else if ((e.KeyValue == 90 && !shift && !capslock2) || (e.KeyValue == 90 && shift && capslock2))
                {
                    keys.Add("z");
                    addColorBlack("z");
                    keysCount["z"] = keysCount.ContainsKey("z") ? keysCount["z"] + 1 : 1;
                }
                if ((e.KeyValue == 88 && shift && !capslock2) || (e.KeyValue == 88 && !shift && capslock2))
                {
                    keys.Add("X");
                    addColorBlack("X");
                    keysCount["X"] = keysCount.ContainsKey("X") ? keysCount["X"] + 1 : 1;
                }
                else if ((e.KeyValue == 88 && !shift && !capslock2) || (e.KeyValue == 88 && shift && capslock2))
                {
                    keys.Add("x");
                    addColorBlack("x");
                    keysCount["x"] = keysCount.ContainsKey("x") ? keysCount["x"] + 1 : 1;
                }
                if ((e.KeyValue == 67 && shift && !capslock2) || (e.KeyValue == 67 && !shift && capslock2))
                {
                    keys.Add("C");
                    addColorBlack("C");
                    keysCount["C"] = keysCount.ContainsKey("C") ? keysCount["C"] + 1 : 1;
                }
                else if ((e.KeyValue == 67 && !shift && !capslock2) || (e.KeyValue == 67 && shift && capslock2))
                {
                    keys.Add("c");
                    addColorBlack("c");
                    keysCount["c"] = keysCount.ContainsKey("c") ? keysCount["c"] + 1 : 1;
                }
                if ((e.KeyValue == 86 && shift && !capslock2) || (e.KeyValue == 86 && !shift && capslock2))
                {
                    keys.Add("V");
                    addColorBlack("V");
                    keysCount["V"] = keysCount.ContainsKey("V") ? keysCount["V"] + 1 : 1;
                }
                else if ((e.KeyValue == 86 && !shift && !capslock2) || (e.KeyValue == 86 && shift && capslock2))
                {
                    keys.Add("v");
                    addColorBlack("v");
                    keysCount["v"] = keysCount.ContainsKey("v") ? keysCount["v"] + 1 : 1;
                }
                if ((e.KeyValue == 66 && shift && !capslock2) || (e.KeyValue == 66 && !shift && capslock2))
                {
                    keys.Add("B");
                    addColorBlack("B");
                    keysCount["B"] = keysCount.ContainsKey("B") ? keysCount["B"] + 1 : 1;
                }
                else if ((e.KeyValue == 66 && !shift && !capslock2) || (e.KeyValue == 66 && shift && capslock2))
                {
                    keys.Add("b");
                    addColorBlack("b");
                    keysCount["b"] = keysCount.ContainsKey("b") ? keysCount["b"] + 1 : 1;
                }
                if ((e.KeyValue == 78 && shift && !capslock2) || (e.KeyValue == 78 && !shift && capslock2))
                {
                    keys.Add("N");
                    addColorBlack("N");
                    keysCount["N"] = keysCount.ContainsKey("N") ? keysCount["N"] + 1 : 1;
                }
                else if ((e.KeyValue == 78 && !shift && !capslock2) || (e.KeyValue == 78 && shift && capslock2))
                {
                    keys.Add("n");
                    addColorBlack("n");
                    keysCount["n"] = keysCount.ContainsKey("n") ? keysCount["n"] + 1 : 1;
                }
                if ((e.KeyValue == 77 && shift && !capslock2) || (e.KeyValue == 77 && !shift && capslock2))
                {
                    keys.Add("M");
                    addColorBlack("M");
                    keysCount["M"] = keysCount.ContainsKey("M") ? keysCount["M"] + 1 : 1;
                }
                else if ((e.KeyValue == 77 && !shift && !capslock2) || (e.KeyValue == 77 && shift && capslock2))
                {
                    keys.Add("m");
                    addColorBlack("m");
                    keysCount["m"] = keysCount.ContainsKey("m") ? keysCount["m"] + 1 : 1;
                }
                if ((e.KeyValue == 188 && shift && !capslock2) || (e.KeyValue == 188 && !shift && capslock2))
                {
                    keys.Add("<");
                    addColorBlack("<");
                    keysCount["<"] = keysCount.ContainsKey("<") ? keysCount["<"] + 1 : 1;
                }
                else if ((e.KeyValue == 188 && !shift && !capslock2) || (e.KeyValue == 188 && shift && capslock2))
                {
                    keys.Add(",");
                    addColorBlack(",");
                    keysCount[","] = keysCount.ContainsKey(",") ? keysCount[","] + 1 : 1;
                }
                if ((e.KeyValue == 190 && shift && !capslock2) || (e.KeyValue == 190 && !shift && capslock2))
                {
                    keys.Add(">");
                    addColorBlack(">");
                    keysCount[">"] = keysCount.ContainsKey(">") ? keysCount[">"] + 1 : 1;
                }
                else if ((e.KeyValue == 190 && !shift && !capslock2) || (e.KeyValue == 190 && shift && capslock2))
                {
                    keys.Add(".");
                    addColorBlack(".");
                    keysCount["."] = keysCount.ContainsKey(".") ? keysCount["."] + 1 : 1;
                }
                if ((e.KeyValue == 191 && shift && !capslock2) || (e.KeyValue == 191 && !shift && capslock2))
                {
                    keys.Add("?");
                    addColorBlack("?");
                    keysCount["?"] = keysCount.ContainsKey("?") ? keysCount["?"] + 1 : 1;
                }
                else if ((e.KeyValue == 191 && !shift && !capslock2) || (e.KeyValue == 191 && shift && capslock2))
                {
                    keys.Add("/");
                    addColorBlack("/");
                    keysCount["/"] = keysCount.ContainsKey("/") ? keysCount["/"] + 1 : 1;
                }
                if ((e.KeyValue == 192 && shift && !capslock2) || (e.KeyValue == 192 && !shift && capslock2))
                {
                    keys.Add("~");
                    addColorBlack("~");
                    keysCount["~"] = keysCount.ContainsKey("~") ? keysCount["~"] + 1 : 1;
                }
                else if ((e.KeyValue == 192 && !shift && !capslock2) || (e.KeyValue == 192 && shift && capslock2))
                {
                    keys.Add("`");
                    addColorBlack("`");
                    keysCount["`"] = keysCount.ContainsKey("`") ? keysCount["`"] + 1 : 1;
                }
                ///////////////////////////Далі змін в умовах немає
                if (e.KeyValue == 49 && (shift || capslock2))
                {
                    keys.Add("!");
                    addColorBlack("!");
                    keysCount["!"] = keysCount.ContainsKey("!") ? keysCount["!"] + 1 : 1;
                }
                else if (e.KeyValue == 49 && !shift)
                {
                    keys.Add("1");
                    addColorBlack("1");
                    keysCount["1"] = keysCount.ContainsKey("1") ? keysCount["1"] + 1 : 1;
                }
                if (e.KeyValue == 50 && (shift || capslock2))
                {
                    keys.Add("@");
                    addColorBlack("@");
                    keysCount["@"] = keysCount.ContainsKey("@") ? keysCount["@"] + 1 : 1;
                }
                else if (e.KeyValue == 50 && !shift)
                {
                    keys.Add("2");
                    addColorBlack("2");
                    keysCount["2"] = keysCount.ContainsKey("2") ? keysCount["2"] + 1 : 1;
                }
                if (e.KeyValue == 51 && (shift || capslock2))
                {
                    keys.Add("#");
                    addColorBlack("#");
                    keysCount["#"] = keysCount.ContainsKey("#") ? keysCount["#"] + 1 : 1;
                }
                else if (e.KeyValue == 51 && !shift)
                {
                    keys.Add("3");
                    addColorBlack("3");
                    keysCount["3"] = keysCount.ContainsKey("3") ? keysCount["3"] + 1 : 1;
                }
                if (e.KeyValue == 52 && (shift || capslock2))
                {
                    keys.Add("$");
                    addColorBlack("$");
                    keysCount["$"] = keysCount.ContainsKey("$") ? keysCount["$"] + 1 : 1;
                }
                else if (e.KeyValue == 52 && !shift)
                {
                    keys.Add("4");
                    addColorBlack("4");
                    keysCount["4"] = keysCount.ContainsKey("4") ? keysCount["4"] + 1 : 1;
                }
                if (e.KeyValue == 53 && (shift || capslock2))
                {
                    keys.Add("%");
                    addColorBlack("%");
                    keysCount["%"] = keysCount.ContainsKey("%") ? keysCount["%"] + 1 : 1;
                }
                else if (e.KeyValue == 53 && !shift)
                {
                    keys.Add("5");
                    addColorBlack("5");
                    keysCount["5"] = keysCount.ContainsKey("5") ? keysCount["5"] + 1 : 1;
                }
                if (e.KeyValue == 54 && (shift || capslock2))
                {
                    keys.Add("^");
                    addColorBlack("^");
                    keysCount["^"] = keysCount.ContainsKey("^") ? keysCount["^"] + 1 : 1;
                }
                else if (e.KeyValue == 54 && !shift)
                {
                    keys.Add("6");
                    addColorBlack("6");
                    keysCount["6"] = keysCount.ContainsKey("6") ? keysCount["6"] + 1 : 1;
                }
                if (e.KeyValue == 55 && (shift || capslock2))
                {
                    keys.Add("&");
                    addColorBlack("&");
                    keysCount["&"] = keysCount.ContainsKey("&") ? keysCount["&"] + 1 : 1;
                }
                else if (e.KeyValue == 55 && !shift)
                {
                    keys.Add("7");
                    addColorBlack("7");
                    keysCount["7"] = keysCount.ContainsKey("7") ? keysCount["7"] + 1 : 1;
                }
                if (e.KeyValue == 56 && (shift || capslock2))
                {
                    keys.Add("*");
                    addColorBlack("*");
                    keysCount["*"] = keysCount.ContainsKey("*") ? keysCount["*"] + 1 : 1;
                }
                else if (e.KeyValue == 56 && !shift)
                {
                    keys.Add("8");
                    addColorBlack("8");
                    keysCount["8"] = keysCount.ContainsKey("8") ? keysCount["8"] + 1 : 1;
                }
                if (e.KeyValue == 57 && (shift || capslock2))
                {
                    keys.Add("(");
                    addColorBlack("(");
                    keysCount["("] = keysCount.ContainsKey("(") ? keysCount["("] + 1 : 1;
                }
                else if (e.KeyValue == 57 && !shift)
                {
                    keys.Add("9");
                    addColorBlack("9");
                    keysCount["9"] = keysCount.ContainsKey("9") ? keysCount["9"] + 1 : 1;
                }
                if (e.KeyValue == 48 && (shift || capslock2))
                {
                    keys.Add(")");
                    addColorBlack(")");
                    keysCount[")"] = keysCount.ContainsKey(")") ? keysCount[")"] + 1 : 1;
                }
                else if (e.KeyValue == 48 && !shift)
                {
                    keys.Add("0");
                    addColorBlack("0");
                    keysCount["0"] = keysCount.ContainsKey("0") ? keysCount["0"] + 1 : 1;
                }
                if (e.KeyValue == 189 && (shift || capslock2))
                {
                    keys.Add("_");
                    addColorBlack("_");
                    keysCount["_"] = keysCount.ContainsKey("_") ? keysCount["_"] + 1 : 1;
                }
                else if (e.KeyValue == 189 && !shift)
                {
                    keys.Add("-");
                    addColorBlack("-");
                    keysCount["-"] = keysCount.ContainsKey("-") ? keysCount["-"] + 1 : 1;
                }
                if (e.KeyValue == 187 && (shift || capslock2))
                {
                    keys.Add("+");
                    addColorBlack("+");
                    keysCount["+"] = keysCount.ContainsKey("+") ? keysCount["+"] + 1 : 1;
                }
                else if (e.KeyValue == 187 && !shift)
                {
                    keys.Add("=");
                    addColorBlack("=");
                    keysCount["="] = keysCount.ContainsKey("=") ? keysCount["="] + 1 : 1;
                }

                //Other keyys
                if (e.KeyValue == 32 && (shift || capslock2) || e.KeyValue == 32 && !shift)
                {
                    keys.Add(" ");
                    keysCount["Space"] = keysCount.ContainsKey("Space") ? keysCount["Space"] + 1 : 1;

                }
                if (e.KeyValue == 27)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Esc"))
                    {
                        keys.Add("Esc");
                        addColorBlue("Esc");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Esc") keys.Add("Esc");
                    keysCount["Esc"] = keysCount.ContainsKey("Esc") ? keysCount["Esc"] + 1 : 1;
                }
                if (e.KeyValue == 162 || e.KeyValue == 163)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Ctrl"))
                    {
                        keys.Add("Ctrl");
                        addColorBlue("Ctrl");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Ctrl") keys.Add("Ctrl");
                    keysCount["Ctrl"] = keysCount.ContainsKey("Ctrl") ? keysCount["Ctrl"] + 1 : 1;
                }
                if (e.KeyValue == 160 || e.KeyValue == 161)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Shift"))
                    {
                        keys.Add("Shift");
                        addColorBlue("Shift");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Shift") keys.Add("Shift");
                    keysCount["Shift"] = keysCount.ContainsKey("Shift") ? keysCount["Shift"] + 1 : 1;
                }
                if (e.KeyValue == 9)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Tab"))
                    {
                        keys.Add("Tab");
                        addColorBlue("Tab");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Tab") keys.Add("Tab");
                    keysCount["Tab"] = keysCount.ContainsKey("Tab") ? keysCount["Tab"] + 1 : 1;
                }
                if (e.KeyValue == 20)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("CapsLock"))
                    {
                        keys.Add("CapsLock");
                        addColorBlue("CapsLock");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "CapsLock") keys.Add("CapsLock");
                    keysCount["CapsLock"] = keysCount.ContainsKey("CapsLock") ? keysCount["CapsLock"] + 1 : 1;
                }
                if (e.KeyValue == 164)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Alt"))
                    {
                        keys.Add("Alt");
                        addColorBlue("Alt");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Alt") keys.Add("Alt");
                    keysCount["Alt"] = keysCount.ContainsKey("Alt") ? keysCount["Alt"] + 1 : 1;
                }
                //Keys F1-F12
                if (e.KeyValue >= 112 && e.KeyValue <= 123)
                {
                    if (keys.Count >= 0 && !keys.Contains(e.KeyCode.ToString()))
                    {
                        keys.Add(e.KeyCode.ToString());
                        addColorBlue(e.KeyCode.ToString());
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != e.KeyCode.ToString()) keys.Add(e.KeyCode.ToString());
                    keysCount[e.KeyCode.ToString()] = keysCount.ContainsKey(e.KeyCode.ToString()) ? keysCount[e.KeyCode.ToString()] + 1 : 1;
                }
                if (e.KeyValue == 8)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Back"))
                    {
                        keys.Add("Back");
                        addColorBlue("Back");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Back") keys.Add("Back");
                    keysCount["Back"] = keysCount.ContainsKey("Back") ? keysCount["Back"] + 1 : 1;
                }
                if (e.KeyValue == 13)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Enter"))
                    {
                        keys.Add("Enter");
                        addColorBlue("Enter");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Enter") keys.Add("Enter");
                    keysCount["Enter"] = keysCount.ContainsKey("Enter") ? keysCount["Enter"] + 1 : 1;
                }
                if (e.KeyValue == 44)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("PrtScn"))
                    {
                        keys.Add("PrtScn");
                        addColorBlue("PrtScn");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "PrtScn") keys.Add("PrtScn");
                    keysCount["PrtScn"] = keysCount.ContainsKey("PrtScn") ? keysCount["PrtScn"] + 1 : 1;
                }
                if (e.KeyValue == 145)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Scroll"))
                    {
                        keys.Add("Scroll");
                        addColorBlue("Scroll");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Scroll") keys.Add("Scroll");
                    keysCount["Scroll"] = keysCount.ContainsKey("Scroll") ? keysCount["Scroll"] + 1 : 1;
                }
                if (e.KeyValue == 19)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Pause"))
                    {
                        keys.Add("Pause");
                        addColorBlue("Pause");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Pause") keys.Add("Pause");
                    keysCount["Pause"] = keysCount.ContainsKey("Pause") ? keysCount["Pause"] + 1 : 1;
                }
                if (e.KeyValue == 45)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Ins"))
                    {
                        keys.Add("Ins");
                        addColorBlue("Ins");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Ins") keys.Add("Ins");
                    keysCount["Ins"] = keysCount.ContainsKey("Ins") ? keysCount["Ins"] + 1 : 1;
                }
                if (e.KeyValue == 46)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Del"))
                    {
                        keys.Add("Del");
                        addColorBlue("Del");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Del") keys.Add("Del");
                    keysCount["Del"] = keysCount.ContainsKey("Del") ? keysCount["Del"] + 1 : 1;
                }
                if (e.KeyValue == 36)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Home"))
                    {
                        keys.Add("Home");
                        addColorBlue("Home");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "Home") keys.Add("Home");
                    keysCount["Home"] = keysCount.ContainsKey("Home") ? keysCount["Home"] + 1 : 1;
                }
                if (e.KeyValue == 35)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("End"))
                    {
                        keys.Add("End");
                        addColorBlue("End");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "End") keys.Add("End");
                    keysCount["End"] = keysCount.ContainsKey("End") ? keysCount["End"] + 1 : 1;
                }
                if (e.KeyValue == 33)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("PageUp"))
                    {
                        keys.Add("PageUp");
                        addColorBlue("PageUp");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "PageUp") keys.Add("PageUp");
                    keysCount["PageUp"] = keysCount.ContainsKey("PageUp") ? keysCount["PageUp"] + 1 : 1;
                }
                if (e.KeyValue == 34)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("PageDown"))
                    {
                        keys.Add("PageDown");
                        addColorBlue("PageDown");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "PageDown") keys.Add("PageDown");
                    keysCount["PageDown"] = keysCount.ContainsKey("PageDown") ? keysCount["PageDown"] + 1 : 1;
                }
                if (e.KeyValue == 38)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("↑"))
                    {
                        keys.Add("↑");
                        addColorBlue("↑");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "↑") keys.Add("↑");
                    keysCount["↑"] = keysCount.ContainsKey("↑") ? keysCount["↑"] + 1 : 1;
                }
                if (e.KeyValue == 37)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("←"))
                    {
                        keys.Add("←");
                        addColorBlue("←");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "←") keys.Add("←");
                    keysCount["←"] = keysCount.ContainsKey("←") ? keysCount["←"] + 1 : 1;
                }
                if (e.KeyValue == 39)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("→"))
                    {
                        keys.Add("→");
                        addColorBlue("→");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "→") keys.Add("→");
                    keysCount["→"] = keysCount.ContainsKey("→") ? keysCount["→"] + 1 : 1;
                }
                if (e.KeyValue == 40)
                {
                    if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("↓"))
                    {
                        keys.Add("↓");
                        addColorBlue("↓");
                    }
                    else if (keys.Count > 0 && keys[keys.Count - 1] != "↓") keys.Add("↓");
                    keysCount["↓"] = keysCount.ContainsKey("↓") ? keysCount["↓"] + 1 : 1;
                }
                if (numLock)
                {
                    if (e.KeyValue == 96 && (shift || capslock2))
                    {
                        keys.Add("Ins");
                        addColorBlue("Ins");
                        keysCount["Ins"] = keysCount.ContainsKey("Ins") ? keysCount["Ins"] + 1 : 1;
                    }
                    else if (e.KeyValue == 96 && !shift)
                    {
                        keys.Add("0");
                        addColorBlack("0");
                        keysCount["0"] = keysCount.ContainsKey("0") ? keysCount["0"] + 1 : 1;
                    }
                    if (e.KeyValue == 110 && (shift || capslock2))
                    {
                        keys.Add("Del");
                        addColorBlue("Del");
                        keysCount["Del"] = keysCount.ContainsKey("Del") ? keysCount["Del"] + 1 : 1;
                    }
                    else if (e.KeyValue == 110 && !shift)
                    {
                        keys.Add(".");
                        addColorBlack(".");
                        keysCount["."] = keysCount.ContainsKey(".") ? keysCount["."] + 1 : 1;
                    }
                    if (e.KeyValue == 97 && (shift || capslock2))
                    {
                        keys.Add("End");
                        addColorBlue("End");
                        keysCount["End"] = keysCount.ContainsKey("End") ? keysCount["End"] + 1 : 1;
                    }
                    else if (e.KeyValue == 97 && !shift)
                    {
                        keys.Add("1");
                        addColorBlack("1");
                        keysCount["1"] = keysCount.ContainsKey("1") ? keysCount["1"] + 1 : 1;
                    }
                    if (e.KeyValue == 98 && (shift || capslock2))
                    {
                        keys.Add("↓");
                        addColorBlue("↓");
                        keysCount["↓"] = keysCount.ContainsKey("↓") ? keysCount["↓"] + 1 : 1;
                    }
                    else if (e.KeyValue == 98 && !shift)
                    {
                        keys.Add("2");
                        addColorBlack("2");
                        keysCount["2"] = keysCount.ContainsKey("2") ? keysCount["2"] + 1 : 1;
                    }
                    if (e.KeyValue == 99 && (shift || capslock2))
                    {
                        keys.Add("PageDown");
                        addColorBlue("PageDown");
                        keysCount["PageDown"] = keysCount.ContainsKey("PageDown") ? keysCount["PageDown"] + 1 : 1;
                    }
                    else if (e.KeyValue == 99 && !shift)
                    {
                        keys.Add("3");
                        addColorBlack("3");
                        keysCount["3"] = keysCount.ContainsKey("3") ? keysCount["3"] + 1 : 1;
                    }
                    if (e.KeyValue == 100 && (shift || capslock2))
                    {
                        keys.Add("←");
                        addColorBlue("←");
                        keysCount["←"] = keysCount.ContainsKey("←") ? keysCount["←"] + 1 : 1;
                    }
                    else if (e.KeyValue == 100 && !shift)
                    {
                        keys.Add("4");
                        addColorBlack("4");
                        keysCount["4"] = keysCount.ContainsKey("4") ? keysCount["4"] + 1 : 1;
                    }
                    //При натисканні Shift+NumPad5 з активованим NumLock потрібно детальніше перевірити обробку цієї ситуації
                    if (e.KeyValue == 101 && (shift || capslock2))
                    {
                        keys.Add("Clear");
                        addColorBlue("Clear");
                        keysCount["Clear"] = keysCount.ContainsKey("Clear") ? keysCount["Clear"] + 1 : 1;
                    }
                    else if (e.KeyValue == 101 && !shift)
                    {
                        keys.Add("5");
                        addColorBlack("5");
                        keysCount["5"] = keysCount.ContainsKey("5") ? keysCount["5"] + 1 : 1;
                    }
                    if (e.KeyValue == 102 && (shift || capslock2))
                    {
                        keys.Add("→");
                        addColorBlue("→");
                        keysCount["→"] = keysCount.ContainsKey("→") ? keysCount["→"] + 1 : 1;
                    }
                    else if (e.KeyValue == 102 && !shift)
                    {
                        keys.Add("6");
                        addColorBlack("6");
                        keysCount["6"] = keysCount.ContainsKey("6") ? keysCount["6"] + 1 : 1;
                    }
                    if (e.KeyValue == 103 && (shift || capslock2))
                    {
                        keys.Add("Home");
                        addColorBlue("Home");
                        keysCount["Home"] = keysCount.ContainsKey("Home") ? keysCount["Home"] + 1 : 1;
                    }
                    else if (e.KeyValue == 103 && !shift)
                    {
                        keys.Add("7");
                        addColorBlack("7");
                        keysCount["7"] = keysCount.ContainsKey("7") ? keysCount["7"] + 1 : 1;
                    }
                    if (e.KeyValue == 104 && (shift || capslock2))
                    {
                        keys.Add("↑");
                        addColorBlue("↑");
                        keysCount["↑"] = keysCount.ContainsKey("↑") ? keysCount["↑"] + 1 : 1;
                    }
                    else if (e.KeyValue == 104 && !shift)
                    {
                        keys.Add("8");
                        addColorBlack("8");
                        keysCount["8"] = keysCount.ContainsKey("8") ? keysCount["8"] + 1 : 1;
                    }
                    if (e.KeyValue == 105 && (shift || capslock2))
                    {
                        keys.Add("PageUp");
                        addColorBlue("PageUp");
                        keysCount["PageUp"] = keysCount.ContainsKey("PageUp") ? keysCount["PageUp"] + 1 : 1;
                    }
                    else if (e.KeyValue == 105 && !shift)
                    {
                        keys.Add("9");
                        addColorBlack("9");
                        keysCount["9"] = keysCount.ContainsKey("9") ? keysCount["9"] + 1 : 1;
                    }
                    if (e.KeyValue == 111)
                    {
                        keys.Add("/");
                        addColorBlack("/");
                        keysCount["/"] = keysCount.ContainsKey("/") ? keysCount["/"] + 1 : 1;
                    }
                    if (e.KeyValue == 106)
                    {
                        keys.Add("*");
                        addColorBlack("*");
                        keysCount["*"] = keysCount.ContainsKey("*") ? keysCount["*"] + 1 : 1;
                    }
                    if (e.KeyValue == 107)
                    {
                        keys.Add("+");
                        addColorBlack("+");
                        keysCount["+"] = keysCount.ContainsKey("+") ? keysCount["+"] + 1 : 1;
                    }
                    if (e.KeyValue == 109)
                    {
                        keys.Add("-");
                        addColorBlack("-");
                        keysCount["-"] = keysCount.ContainsKey("-") ? keysCount["-"] + 1 : 1;
                    }

                }
                else if (!numLock)
                {
                    if (e.KeyValue == 12 && (shift || capslock2))
                    {
                        if (KeyPress.Text.Length >= 0 && !KeyPress.Text.EndsWith("Clear"))
                        {
                            keys.Add("Clear");
                            addColorBlue("Clear");
                        }
                        else if (keys.Count > 0 && keys[keys.Count - 1] != "Clear") keys.Add("Clear");
                        keysCount["Clear"] = keysCount.ContainsKey("Clear") ? keysCount["Clear"] + 1 : 1;
                    }
                    else if (e.KeyValue == 12 && !shift)
                    {
                        keys.Add("5");
                        addColorBlack("5");
                        keysCount["5"] = keysCount.ContainsKey("5") ? keysCount["5"] + 1 : 1;
                    }
                    if (e.KeyValue == 110 && (shift || capslock2))
                    {
                        keys.Add("Del");
                        addColorBlue("Del");
                        keysCount["Del"] = keysCount.ContainsKey("Del") ? keysCount["Del"] + 1 : 1;
                    }
                    else if (e.KeyValue == 110 && !shift)
                    {
                        keys.Add("Del");
                        addColorBlue("Del");
                        keysCount["Del"] = keysCount.ContainsKey("Del") ? keysCount["Del"] + 1 : 1;
                    }
                    if (e.KeyValue == 111)
                    {
                        keys.Add("/");
                        addColorBlack("/");
                        keysCount["/"] = keysCount.ContainsKey("/") ? keysCount["/"] + 1 : 1;
                    }
                    if (e.KeyValue == 106)
                    {
                        keys.Add("*");
                        addColorBlack("*");
                        keysCount["*"] = keysCount.ContainsKey("*") ? keysCount["*"] + 1 : 1;
                    }
                    if (e.KeyValue == 107)
                    {
                        keys.Add("+");
                        addColorBlack("+");
                        keysCount["+"] = keysCount.ContainsKey("+") ? keysCount["+"] + 1 : 1;
                    }
                    if (e.KeyValue == 109)
                    {
                        keys.Add("-");
                        addColorBlack("-");
                        keysCount["-"] = keysCount.ContainsKey("-") ? keysCount["-"] + 1 : 1;
                    }
                }
            }


            //KeyPress.Text = "";
            /*foreach (var key in keys)
            {

                KeyPress.Text += key;

            }*/
            //KeyPress.Text += e.KeyValue;
        }

        static string GetCurrentDateTimeString()
        {
            DateTime currentDateTime = DateTime.Now;

            string formattedDateTime = currentDateTime.ToString("dd.MM.yyyy HH:mm:ss");

            return formattedDateTime;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = "keysReport.txt";
            string path2 = "processReport.txt";
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("All keys inputed:");
                writer.Write(KeyPress.Text);
                writer.WriteLine("");
                writer.WriteLine("Each key input count:");
                foreach (var i in keysCount)
                {
                    writer.WriteLine("{0}: {1}", i.Key, i.Value);
                }
            }
            using (StreamWriter writer = new StreamWriter(path2))
            {
                writer.WriteLine("{0,-50} {1,-20} {2,-20} {3,-20} {4,-20} {5,-19} {6,-20}", "Process Name", "ID", "CPU Usage (%)", "Memory Usage (MB)", "Start Time", "Thread Count", "Full Start Time");

                foreach (Process process in processes)
                {
                    writer.Write("{0,-50} {1,-20} {2,-20} {3,-20} {4,-20} {5,-20}", process.ProcessName.ToString(), process.Id.ToString(), GetProcessorTime(process), GetWorkingSet(process), GetStartTime(process), GetThreadCount(process));
                    try
                    {
                        writer.Write(String.Format(process.StartTime.ToShortDateString() + "   -   " + processes[selectedID].StartTime.ToLongTimeString()));
                    }
                    catch (Exception)
                    {
                        writer.Write("N/A");
                    }
                    writer.WriteLine("");
                }
            }
            if (System.IO.File.Exists(path))
            {
                Process notepadProcess = new();
                notepadProcess.StartInfo.FileName = "notepad.exe";
                notepadProcess.StartInfo.Arguments = path;
                notepadProcess.Start();

                SetWindowText(notepadProcess.MainWindowHandle, new string("Processes Report " + GetCurrentDateTimeString()));
            }
            if (System.IO.File.Exists(path2))
            {
                Process notepadProcess = new();
                notepadProcess.StartInfo.FileName = "notepad.exe";
                notepadProcess.StartInfo.Arguments = path2;
                notepadProcess.Start();

                SetWindowText(notepadProcess.MainWindowHandle, new string("Processes Report " + GetCurrentDateTimeString()));
            }
        }
    }
}