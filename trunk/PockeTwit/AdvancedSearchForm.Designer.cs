using System;
namespace PockeTwit
{
    partial class AdvancedSearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

        /// <summary>
        /// Clean up any resources being used.
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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuOK = new System.Windows.Forms.MenuItem();
            this.menuCancel = new System.Windows.Forms.MenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageWords = new System.Windows.Forms.TabPage();
            this.txtHashtag = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtPhrase = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtNone = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAny = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAll = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPagePeople = new System.Windows.Forms.TabPage();
            this.txtReferencing = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPageDates = new System.Windows.Forms.TabPage();
            this.dateTimePickerUntil = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerSince = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.inputPanel1 = new Microsoft.WindowsCE.Forms.InputPanel();
            this.tabControl1.SuspendLayout();
            this.tabPageWords.SuspendLayout();
            this.tabPagePeople.SuspendLayout();
            this.tabPageDates.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuOK);
            this.mainMenu1.MenuItems.Add(this.menuCancel);
            // 
            // menuOK
            // 
            this.menuOK.Text = "OK";
            this.menuOK.Click += new System.EventHandler(this.menuOK_Click);
            // 
            // menuCancel
            // 
            this.menuCancel.Text = "Cancel";
            this.menuCancel.Click += new System.EventHandler(this.menuCancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageWords);
            this.tabControl1.Controls.Add(this.tabPagePeople);
            this.tabControl1.Controls.Add(this.tabPageDates);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(240, 268);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageWords
            // 
            this.tabPageWords.BackColor = System.Drawing.Color.Black;
            this.tabPageWords.Controls.Add(this.txtHashtag);
            this.tabPageWords.Controls.Add(this.label10);
            this.tabPageWords.Controls.Add(this.txtPhrase);
            this.tabPageWords.Controls.Add(this.label9);
            this.tabPageWords.Controls.Add(this.txtNone);
            this.tabPageWords.Controls.Add(this.label3);
            this.tabPageWords.Controls.Add(this.txtAny);
            this.tabPageWords.Controls.Add(this.label2);
            this.tabPageWords.Controls.Add(this.txtAll);
            this.tabPageWords.Controls.Add(this.label1);
            this.tabPageWords.Location = new System.Drawing.Point(0, 0);
            this.tabPageWords.Name = "tabPageWords";
            this.tabPageWords.Size = new System.Drawing.Size(240, 245);
            this.tabPageWords.Text = "Words";
            // 
            // txtHashtag
            // 
            this.txtHashtag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHashtag.Location = new System.Drawing.Point(57, 115);
            this.txtHashtag.Name = "txtHashtag";
            this.txtHashtag.Size = new System.Drawing.Size(180, 21);
            this.txtHashtag.TabIndex = 4;
            // 
            // label10
            // 
            this.label10.ForeColor = System.Drawing.Color.LightGray;
            this.label10.Location = new System.Drawing.Point(4, 115);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 20);
            this.label10.Text = "Hash:";
            // 
            // txtPhrase
            // 
            this.txtPhrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPhrase.Location = new System.Drawing.Point(57, 34);
            this.txtPhrase.Name = "txtPhrase";
            this.txtPhrase.Size = new System.Drawing.Size(180, 21);
            this.txtPhrase.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.ForeColor = System.Drawing.Color.LightGray;
            this.label9.Location = new System.Drawing.Point(4, 34);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 20);
            this.label9.Text = "Phrase:";
            // 
            // txtNone
            // 
            this.txtNone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNone.Location = new System.Drawing.Point(57, 88);
            this.txtNone.Name = "txtNone";
            this.txtNone.Size = new System.Drawing.Size(180, 21);
            this.txtNone.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.ForeColor = System.Drawing.Color.LightGray;
            this.label3.Location = new System.Drawing.Point(4, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 20);
            this.label3.Text = "None:";
            // 
            // txtAny
            // 
            this.txtAny.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAny.Location = new System.Drawing.Point(57, 61);
            this.txtAny.Name = "txtAny";
            this.txtAny.Size = new System.Drawing.Size(180, 21);
            this.txtAny.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.Color.LightGray;
            this.label2.Location = new System.Drawing.Point(4, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 20);
            this.label2.Text = "Any:";
            // 
            // txtAll
            // 
            this.txtAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAll.Location = new System.Drawing.Point(57, 7);
            this.txtAll.Name = "txtAll";
            this.txtAll.Size = new System.Drawing.Size(180, 21);
            this.txtAll.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 20);
            this.label1.Text = "All:";
            // 
            // tabPagePeople
            // 
            this.tabPagePeople.BackColor = System.Drawing.Color.Black;
            this.tabPagePeople.Controls.Add(this.txtReferencing);
            this.tabPagePeople.Controls.Add(this.label4);
            this.tabPagePeople.Controls.Add(this.txtTo);
            this.tabPagePeople.Controls.Add(this.label5);
            this.tabPagePeople.Controls.Add(this.txtFrom);
            this.tabPagePeople.Controls.Add(this.label6);
            this.tabPagePeople.Location = new System.Drawing.Point(0, 0);
            this.tabPagePeople.Name = "tabPagePeople";
            this.tabPagePeople.Size = new System.Drawing.Size(240, 245);
            this.tabPagePeople.Text = "People";
            // 
            // txtReferencing
            // 
            this.txtReferencing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtReferencing.Location = new System.Drawing.Point(57, 61);
            this.txtReferencing.Name = "txtReferencing";
            this.txtReferencing.Size = new System.Drawing.Size(180, 21);
            this.txtReferencing.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.ForeColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(4, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 20);
            this.label4.Text = "Ref:";
            // 
            // txtTo
            // 
            this.txtTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTo.Location = new System.Drawing.Point(57, 34);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(180, 21);
            this.txtTo.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.ForeColor = System.Drawing.Color.LightGray;
            this.label5.Location = new System.Drawing.Point(4, 34);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 20);
            this.label5.Text = "To:";
            // 
            // txtFrom
            // 
            this.txtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFrom.Location = new System.Drawing.Point(57, 7);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(180, 21);
            this.txtFrom.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.ForeColor = System.Drawing.Color.LightGray;
            this.label6.Location = new System.Drawing.Point(4, 7);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 20);
            this.label6.Text = "From:";
            // 
            // tabPageDates
            // 
            this.tabPageDates.BackColor = System.Drawing.Color.Black;
            this.tabPageDates.Controls.Add(this.dateTimePickerUntil);
            this.tabPageDates.Controls.Add(this.dateTimePickerSince);
            this.tabPageDates.Controls.Add(this.label8);
            this.tabPageDates.Controls.Add(this.label7);
            this.tabPageDates.Location = new System.Drawing.Point(0, 0);
            this.tabPageDates.Name = "tabPageDates";
            this.tabPageDates.Size = new System.Drawing.Size(240, 245);
            this.tabPageDates.Text = "Dates";
            // 
            // dateTimePickerUntil
            // 
            this.dateTimePickerUntil.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerUntil.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerUntil.Location = new System.Drawing.Point(57, 35);
            this.dateTimePickerUntil.Name = "dateTimePickerUntil";
            this.dateTimePickerUntil.Size = new System.Drawing.Size(179, 22);
            this.dateTimePickerUntil.TabIndex = 1;
            this.dateTimePickerUntil.ValueChanged += new System.EventHandler(this.dateTimePickerUntil_ValueChanged);
            // 
            // dateTimePickerSince
            // 
            this.dateTimePickerSince.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerSince.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerSince.Location = new System.Drawing.Point(57, 7);
            this.dateTimePickerSince.Name = "dateTimePickerSince";
            this.dateTimePickerSince.Size = new System.Drawing.Size(179, 22);
            this.dateTimePickerSince.TabIndex = 0;
            this.dateTimePickerSince.Value = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerSince.ValueChanged += new System.EventHandler(this.dateTimePickerSince_ValueChanged);
            // 
            // label8
            // 
            this.label8.ForeColor = System.Drawing.Color.LightGray;
            this.label8.Location = new System.Drawing.Point(7, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 20);
            this.label8.Text = "Until:";
            // 
            // label7
            // 
            this.label7.ForeColor = System.Drawing.Color.LightGray;
            this.label7.Location = new System.Drawing.Point(4, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 20);
            this.label7.Text = "Since:";
            // 
            // AdvancedSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.tabControl1);
            this.Menu = this.mainMenu1;
            this.Name = "AdvancedSearchForm";
            this.Text = "Advanced Search";
            this.Activated += new System.EventHandler(this.AdvancedSearchForm_Activated);
            this.tabControl1.ResumeLayout(false);
            this.tabPageWords.ResumeLayout(false);
            this.tabPagePeople.ResumeLayout(false);
            this.tabPageDates.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuOK;
        private System.Windows.Forms.MenuItem menuCancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageWords;
        private System.Windows.Forms.TextBox txtNone;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAny;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPagePeople;
        private System.Windows.Forms.TextBox txtReferencing;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFrom;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabPageDates;
        private System.Windows.Forms.DateTimePicker dateTimePickerUntil;
        private System.Windows.Forms.DateTimePicker dateTimePickerSince;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private Microsoft.WindowsCE.Forms.InputPanel inputPanel1;
        private System.Windows.Forms.TextBox txtHashtag;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtPhrase;
        private System.Windows.Forms.Label label9;
    }
}