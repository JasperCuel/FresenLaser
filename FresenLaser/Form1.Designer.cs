namespace FresenLaser
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            bt_selectFile = new Button();
            llb_filePath = new LinkLabel();
            gb_settings = new GroupBox();
            nud_cutSpeed = new NumericUpDown();
            label2 = new Label();
            label1 = new Label();
            nud_moveSpeed = new NumericUpDown();
            bt_start = new Button();
            gb_settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_cutSpeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud_moveSpeed).BeginInit();
            SuspendLayout();
            // 
            // bt_selectFile
            // 
            bt_selectFile.Location = new Point(8, 7);
            bt_selectFile.Margin = new Padding(2, 2, 2, 2);
            bt_selectFile.Name = "bt_selectFile";
            bt_selectFile.Size = new Size(97, 24);
            bt_selectFile.TabIndex = 0;
            bt_selectFile.Text = "Selecteer bestand";
            bt_selectFile.UseVisualStyleBackColor = true;
            bt_selectFile.Click += bt_selectFile_Click;
            // 
            // llb_filePath
            // 
            llb_filePath.AutoSize = true;
            llb_filePath.Location = new Point(117, 11);
            llb_filePath.Margin = new Padding(2, 0, 2, 0);
            llb_filePath.Name = "llb_filePath";
            llb_filePath.Size = new Size(60, 15);
            llb_filePath.TabIndex = 1;
            llb_filePath.TabStop = true;
            llb_filePath.Text = "linkLabel1";
            // 
            // gb_settings
            // 
            gb_settings.Controls.Add(nud_cutSpeed);
            gb_settings.Controls.Add(label2);
            gb_settings.Controls.Add(label1);
            gb_settings.Controls.Add(nud_moveSpeed);
            gb_settings.Location = new Point(8, 35);
            gb_settings.Margin = new Padding(2, 2, 2, 2);
            gb_settings.Name = "gb_settings";
            gb_settings.Padding = new Padding(2, 2, 2, 2);
            gb_settings.Size = new Size(198, 55);
            gb_settings.TabIndex = 2;
            gb_settings.TabStop = false;
            gb_settings.Text = "Instellingen";
            // 
            // nud_cutSpeed
            // 
            nud_cutSpeed.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            nud_cutSpeed.Location = new Point(136, 35);
            nud_cutSpeed.Margin = new Padding(2, 2, 2, 2);
            nud_cutSpeed.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nud_cutSpeed.Name = "nud_cutSpeed";
            nud_cutSpeed.Size = new Size(52, 23);
            nud_cutSpeed.TabIndex = 5;
            nud_cutSpeed.Value = new decimal(new int[] { 700, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 35);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 2;
            label2.Text = "Snijsnelheid";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 17);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(120, 15);
            label1.TabIndex = 1;
            label1.Text = "Verplaatsingssnelheid";
            // 
            // nud_moveSpeed
            // 
            nud_moveSpeed.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            nud_moveSpeed.Location = new Point(136, 16);
            nud_moveSpeed.Margin = new Padding(2, 2, 2, 2);
            nud_moveSpeed.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nud_moveSpeed.Name = "nud_moveSpeed";
            nud_moveSpeed.Size = new Size(52, 23);
            nud_moveSpeed.TabIndex = 0;
            nud_moveSpeed.Value = new decimal(new int[] { 2000, 0, 0, 0 });
            // 
            // bt_start
            // 
            bt_start.Location = new Point(434, 70);
            bt_start.Margin = new Padding(2, 2, 2, 2);
            bt_start.Name = "bt_start";
            bt_start.Size = new Size(52, 22);
            bt_start.TabIndex = 3;
            bt_start.Text = "Start";
            bt_start.UseVisualStyleBackColor = true;
            bt_start.Click += bt_start_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(495, 97);
            Controls.Add(bt_start);
            Controls.Add(gb_settings);
            Controls.Add(llb_filePath);
            Controls.Add(bt_selectFile);
            Name = "Form1";
            Text = "CNC Naar Laser";
            gb_settings.ResumeLayout(false);
            gb_settings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_cutSpeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud_moveSpeed).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button bt_selectFile;
        private LinkLabel llb_filePath;
        private GroupBox gb_settings;
        private NumericUpDown nud_cutSpeed;
        private Label label2;
        private Label label1;
        private NumericUpDown nud_moveSpeed;
        private Button bt_start;
    }
}