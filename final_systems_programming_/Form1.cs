using System.Runtime.InteropServices;

namespace final_systems_programming_
{

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int shwCmds);
        public double opacity { get; set; }

        private List<ListViewItem> selectedItems = new List<ListViewItem>();

        public Form1()
        {
            InitializeComponent();

            myListView.View = View.Details;
            myListView.Columns.Add("Name", 187);
            myListView.Columns.Add("Boot File", 186);


        }

        private void button1_Click(object sender, EventArgs e)
        {


            if (radioButton1.Checked)
            {
                Form2 form2 = new Form2();
                if (checkBoxPatriot.Checked)
                {
                    form2.patriot = true;
                }
                else form2.patriot = false;
                form2.Show(this);
                ShowWindow(this.Handle, 0);
                //form2.wordsForStop = "stop";
                Global.needExit = true;
            }
            else if (radioButton2.Checked)
            {
                if (textBox1.Text.Length == 0)
                {
                    MessageBox.Show("Input at least 1 key word\nfor exiting hidden mode", "Error (Starting Programm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Form2 form2 = new Form2();
                    if (checkBoxPatriot.Checked)
                    {
                        form2.patriot = true;
                    }
                    else form2.patriot = false;
                    ShowWindow(this.Handle, 0);
                    form2.wordsForStop = textBox1.Text;
                    Global.needExit = true;
                }

            }






            //this.Opacity = opacity;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            if (radioButton1.Checked) textBox1.Enabled = false;
            else if (radioButton2.Checked) textBox1.Enabled = true;


        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void MyListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                selectedItems.Add(e.Item);
            }
            else
            {
                selectedItems.Remove(e.Item);
            }

            button11.Enabled = selectedItems.Count > 0;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            List<ListViewItem> itemsToRemove = new List<ListViewItem>();

            foreach (var selectedItem in selectedItems)
            {
                string name = selectedItem.SubItems[0].Text;
                string bootFile = selectedItem.SubItems[1].Text;

                Global.Processes.blockedProcessesNames.Remove(name);
                Global.Processes.blockedProcessesFileNames.Remove(bootFile);

                itemsToRemove.Add(selectedItem);
            }

            foreach (var itemToRemove in itemsToRemove)
            {
                myListView.Items.Remove(itemToRemove);
            }

            selectedItems.Clear();
            myListView.Refresh();

            UpdateListView(Global.Processes.blockedProcessesNames, Global.Processes.blockedProcessesFileNames);
        }
        public void UpdateListView(List<string> names, List<string> bootFiles)
        {
            myListView.Items.Clear();

            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                string bootFile = bootFiles[i];

                ListViewItem newItem = new ListViewItem(new[] { name, bootFile });
                myListView.Items.Add(newItem);
            }

            myListView.Refresh();
            blockedProcessesCount.Text = Global.Processes.blockedProcessesFileNames.Count().ToString();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            UpdateListView(Global.Processes.blockedProcessesNames, Global.Processes.blockedProcessesFileNames);
        }
    }
}