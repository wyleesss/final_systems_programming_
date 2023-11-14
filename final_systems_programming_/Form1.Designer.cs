namespace final_systems_programming_
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;
            if (Global.needExit)
            {
                Environment.Exit(0);
            }
        }


        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button7 = new Button();
            label2 = new Label();
            label1 = new Label();
            button6 = new Button();
            button1 = new Button();
            button2 = new Button();
            label3 = new Label();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            checkBoxPatriot = new CheckBox();
            button8 = new Button();
            label4 = new Label();
            textBox1 = new TextBox();
            button9 = new Button();
            radioButton2 = new RadioButton();
            radioButton1 = new RadioButton();
            myListView = new ListView();
            button10 = new Button();
            label5 = new Label();
            label6 = new Label();
            blockedProcessesCount = new Label();
            button11 = new Button();
            SuspendLayout();
            // 
            // button7
            // 
            button7.BackColor = SystemColors.ControlLight;
            button7.Enabled = false;
            button7.Location = new Point(427, 53);
            button7.Name = "button7";
            button7.Size = new Size(415, 382);
            button7.TabIndex = 35;
            button7.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.BackColor = SystemColors.MenuBar;
            label2.Font = new Font("Yu Gothic UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(534, 11);
            label2.Name = "label2";
            label2.Size = new Size(234, 33);
            label2.TabIndex = 33;
            label2.Text = "Process Tracking";
            // 
            // label1
            // 
            label1.BackColor = SystemColors.MenuBar;
            label1.Font = new Font("Yu Gothic UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(130, 10);
            label1.Name = "label1";
            label1.Size = new Size(170, 33);
            label1.TabIndex = 32;
            label1.Text = "Key Tracking";
            // 
            // button6
            // 
            button6.BackColor = SystemColors.ControlDark;
            button6.Enabled = false;
            button6.Location = new Point(2, 53);
            button6.Name = "button6";
            button6.Size = new Size(419, 205);
            button6.TabIndex = 34;
            button6.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.BackColor = SystemColors.MenuBar;
            button1.Enabled = false;
            button1.Location = new Point(2, 6);
            button1.Name = "button1";
            button1.Size = new Size(419, 43);
            button1.TabIndex = 36;
            button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = SystemColors.MenuBar;
            button2.Enabled = false;
            button2.Location = new Point(427, 6);
            button2.Name = "button2";
            button2.Size = new Size(415, 43);
            button2.TabIndex = 37;
            button2.UseVisualStyleBackColor = false;
            // 
            // label3
            // 
            label3.BackColor = SystemColors.MenuBar;
            label3.Font = new Font("Yu Gothic UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(126, 268);
            label3.Name = "label3";
            label3.Size = new Size(174, 33);
            label3.TabIndex = 38;
            label3.Text = "Global Settings";
            // 
            // button3
            // 
            button3.BackColor = SystemColors.MenuBar;
            button3.Enabled = false;
            button3.Location = new Point(2, 264);
            button3.Name = "button3";
            button3.Size = new Size(419, 43);
            button3.TabIndex = 39;
            button3.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            button4.BackColor = SystemColors.Control;
            button4.Enabled = false;
            button4.Location = new Point(2, 311);
            button4.Name = "button4";
            button4.Size = new Size(419, 124);
            button4.TabIndex = 40;
            button4.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            button5.BackColor = Color.MintCream;
            button5.Font = new Font("Yu Gothic UI Semibold", 18F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            button5.Location = new Point(205, 445);
            button5.Margin = new Padding(3, 4, 3, 4);
            button5.Name = "button5";
            button5.Size = new Size(440, 61);
            button5.TabIndex = 41;
            button5.Text = "Start";
            button5.UseVisualStyleBackColor = false;
            button5.Click += button1_Click;
            // 
            // checkBoxPatriot
            // 
            checkBoxPatriot.BackColor = SystemColors.Control;
            checkBoxPatriot.Font = new Font("Yu Gothic UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            checkBoxPatriot.Location = new Point(148, 68);
            checkBoxPatriot.Margin = new Padding(3, 4, 3, 4);
            checkBoxPatriot.Name = "checkBoxPatriot";
            checkBoxPatriot.Size = new Size(108, 34);
            checkBoxPatriot.TabIndex = 42;
            checkBoxPatriot.Text = "Patriotic";
            checkBoxPatriot.UseVisualStyleBackColor = false;
            // 
            // button8
            // 
            button8.BackColor = SystemColors.MenuBar;
            button8.Enabled = false;
            button8.Location = new Point(134, 63);
            button8.Name = "button8";
            button8.Size = new Size(136, 43);
            button8.TabIndex = 43;
            button8.UseVisualStyleBackColor = false;
            // 
            // label4
            // 
            label4.BackColor = SystemColors.Control;
            label4.Font = new Font("Yu Gothic UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(56, 116);
            label4.Name = "label4";
            label4.Size = new Size(304, 57);
            label4.TabIndex = 44;
            label4.Text = "Input list of key words to exit hidden mode ( divided by space ' '  ):";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            textBox1.Enabled = false;
            textBox1.Location = new Point(56, 176);
            textBox1.Margin = new Padding(3, 4, 3, 4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(304, 67);
            textBox1.TabIndex = 45;
            // 
            // button9
            // 
            button9.BackColor = SystemColors.MenuBar;
            button9.Enabled = false;
            button9.Location = new Point(52, 112);
            button9.Name = "button9";
            button9.Size = new Size(312, 135);
            button9.TabIndex = 46;
            button9.UseVisualStyleBackColor = false;
            // 
            // radioButton2
            // 
            radioButton2.Font = new Font("Yu Gothic UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            radioButton2.Location = new Point(108, 375);
            radioButton2.Margin = new Padding(3, 4, 3, 4);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(210, 24);
            radioButton2.TabIndex = 48;
            radioButton2.Text = "Hidden Mode";
            radioButton2.TextAlign = ContentAlignment.MiddleCenter;
            radioButton2.UseVisualStyleBackColor = true;
            radioButton2.CheckedChanged += radioButton2_CheckedChanged;
            // 
            // radioButton1
            // 
            radioButton1.Checked = true;
            radioButton1.Font = new Font("Yu Gothic UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            radioButton1.Location = new Point(108, 343);
            radioButton1.Margin = new Padding(3, 4, 3, 4);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(210, 24);
            radioButton1.TabIndex = 47;
            radioButton1.TabStop = true;
            radioButton1.Text = "Graphic User Interface";
            radioButton1.TextAlign = ContentAlignment.MiddleCenter;
            radioButton1.UseVisualStyleBackColor = true;
            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // myListView
            // 
            myListView.Font = new Font("Yu Gothic UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            myListView.Location = new Point(447, 97);
            myListView.Name = "myListView";
            myListView.Size = new Size(375, 287);
            myListView.TabIndex = 49;
            myListView.UseCompatibleStateImageBehavior = false;
            myListView.View = View.Details;
            myListView.ItemSelectionChanged += MyListView_ItemSelectionChanged;
            // 
            // button10
            // 
            button10.BackColor = SystemColors.ControlDark;
            button10.Enabled = false;
            button10.Location = new Point(438, 58);
            button10.Name = "button10";
            button10.Size = new Size(393, 365);
            button10.TabIndex = 50;
            button10.UseVisualStyleBackColor = false;
            // 
            // label5
            // 
            label5.BackColor = SystemColors.ControlDark;
            label5.Font = new Font("Yu Gothic UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(479, 68);
            label5.Name = "label5";
            label5.Size = new Size(304, 25);
            label5.TabIndex = 51;
            label5.Text = "blocked processes:";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.BackColor = SystemColors.ControlDark;
            label6.Font = new Font("Yu Gothic UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.Location = new Point(447, 389);
            label6.Name = "label6";
            label6.Size = new Size(52, 23);
            label6.TabIndex = 52;
            label6.Text = "count:";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // blockedProcessesCount
            // 
            blockedProcessesCount.BackColor = SystemColors.ControlDark;
            blockedProcessesCount.Font = new Font("Yu Gothic UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            blockedProcessesCount.Location = new Point(492, 390);
            blockedProcessesCount.Name = "blockedProcessesCount";
            blockedProcessesCount.Size = new Size(134, 23);
            blockedProcessesCount.TabIndex = 53;
            blockedProcessesCount.Text = "0";
            blockedProcessesCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // button11
            // 
            button11.BackColor = Color.MintCream;
            button11.Enabled = false;
            button11.Font = new Font("Yu Gothic UI", 7.8F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            button11.Location = new Point(632, 389);
            button11.Margin = new Padding(3, 4, 3, 4);
            button11.Name = "button11";
            button11.Size = new Size(190, 24);
            button11.TabIndex = 54;
            button11.Text = "UNLOCK SELECTED PROCESS";
            button11.UseVisualStyleBackColor = false;
            button11.Click += button11_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(844, 516);
            Controls.Add(button11);
            Controls.Add(blockedProcessesCount);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(myListView);
            Controls.Add(button10);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(textBox1);
            Controls.Add(label4);
            Controls.Add(checkBoxPatriot);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(label3);
            Controls.Add(button3);
            Controls.Add(button7);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(button2);
            Controls.Add(button8);
            Controls.Add(button9);
            Controls.Add(button6);
            MaximizeBox = false;
            MaximumSize = new Size(862, 563);
            MinimumSize = new Size(862, 563);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Settings";
            Load += Form1_Load_1;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button7;
        private Label label2;
        private Label label1;
        private Button button6;
        private Button button1;
        private Button button2;
        private Label label3;
        private Button button3;
        private Button button4;
        private Button button5;
        private CheckBox checkBoxPatriot;
        private Button button8;
        private Label label4;
        private TextBox textBox1;
        private Button button9;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private ListView myListView;
        private Button button10;
        private Label label5;
        private Label label6;
        private Label blockedProcessesCount;
        private Button button11;
    }
}