namespace PockeTwit
{
    partial class AdvancedSearchSmartphoneForm
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
            this.panelWords = new System.Windows.Forms.Panel();
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
            this.panelMore = new System.Windows.Forms.Panel();
            this.dateTimePickerUntil = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerSince = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtReferencing = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblMore = new System.Windows.Forms.LinkLabel();
            this.lblWords = new System.Windows.Forms.LinkLabel();
            this.panelWords.SuspendLayout();
            this.panelMore.SuspendLayout();
            this.panelBottom.SuspendLayout();
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
            // panelWords
            // 
            this.panelWords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelWords.BackColor = System.Drawing.Color.Black;
            this.panelWords.Controls.Add(this.txtHashtag);
            this.panelWords.Controls.Add(this.label10);
            this.panelWords.Controls.Add(this.txtPhrase);
            this.panelWords.Controls.Add(this.label9);
            this.panelWords.Controls.Add(this.txtNone);
            this.panelWords.Controls.Add(this.label3);
            this.panelWords.Controls.Add(this.txtAny);
            this.panelWords.Controls.Add(this.label2);
            this.panelWords.Controls.Add(this.txtAll);
            this.panelWords.Controls.Add(this.label1);
            this.panelWords.Location = new System.Drawing.Point(5, 3);
            this.panelWords.Name = "panelWords";
            this.panelWords.Size = new System.Drawing.Size(234, 236);
            // 
            // txtHashtag
            // 
            this.txtHashtag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHashtag.Location = new System.Drawing.Point(55, 111);
            this.txtHashtag.Name = "txtHashtag";
            this.txtHashtag.Size = new System.Drawing.Size(176, 21);
            this.txtHashtag.TabIndex = 34;
            // 
            // label10
            // 
            this.label10.ForeColor = System.Drawing.Color.LightGray;
            this.label10.Location = new System.Drawing.Point(2, 111);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(44, 20);
            this.label10.Text = "Hash:";
            // 
            // txtPhrase
            // 
            this.txtPhrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPhrase.Location = new System.Drawing.Point(55, 30);
            this.txtPhrase.Name = "txtPhrase";
            this.txtPhrase.Size = new System.Drawing.Size(176, 21);
            this.txtPhrase.TabIndex = 31;
            // 
            // label9
            // 
            this.label9.ForeColor = System.Drawing.Color.LightGray;
            this.label9.Location = new System.Drawing.Point(2, 30);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 20);
            this.label9.Text = "Phrase:";
            // 
            // txtNone
            // 
            this.txtNone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNone.Location = new System.Drawing.Point(55, 84);
            this.txtNone.Name = "txtNone";
            this.txtNone.Size = new System.Drawing.Size(176, 21);
            this.txtNone.TabIndex = 33;
            // 
            // label3
            // 
            this.label3.ForeColor = System.Drawing.Color.LightGray;
            this.label3.Location = new System.Drawing.Point(2, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 20);
            this.label3.Text = "None:";
            // 
            // txtAny
            // 
            this.txtAny.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAny.Location = new System.Drawing.Point(55, 57);
            this.txtAny.Name = "txtAny";
            this.txtAny.Size = new System.Drawing.Size(176, 21);
            this.txtAny.TabIndex = 32;
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.Color.LightGray;
            this.label2.Location = new System.Drawing.Point(2, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 20);
            this.label2.Text = "Any:";
            // 
            // txtAll
            // 
            this.txtAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAll.Location = new System.Drawing.Point(55, 3);
            this.txtAll.Name = "txtAll";
            this.txtAll.Size = new System.Drawing.Size(176, 21);
            this.txtAll.TabIndex = 30;
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(1, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 20);
            this.label1.Text = "All:";
            // 
            // panelMore
            // 
            this.panelMore.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMore.BackColor = System.Drawing.Color.Black;
            this.panelMore.Controls.Add(this.dateTimePickerUntil);
            this.panelMore.Controls.Add(this.dateTimePickerSince);
            this.panelMore.Controls.Add(this.label8);
            this.panelMore.Controls.Add(this.label7);
            this.panelMore.Controls.Add(this.txtReferencing);
            this.panelMore.Controls.Add(this.label4);
            this.panelMore.Controls.Add(this.txtTo);
            this.panelMore.Controls.Add(this.label5);
            this.panelMore.Controls.Add(this.txtFrom);
            this.panelMore.Controls.Add(this.label6);
            this.panelMore.Location = new System.Drawing.Point(5, 3);
            this.panelMore.Name = "panelMore";
            this.panelMore.Size = new System.Drawing.Size(234, 236);
            // 
            // dateTimePickerUntil
            // 
            this.dateTimePickerUntil.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerUntil.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerUntil.Location = new System.Drawing.Point(53, 112);
            this.dateTimePickerUntil.Name = "dateTimePickerUntil";
            this.dateTimePickerUntil.Size = new System.Drawing.Size(179, 22);
            this.dateTimePickerUntil.TabIndex = 15;
            // 
            // dateTimePickerSince
            // 
            this.dateTimePickerSince.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerSince.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerSince.Location = new System.Drawing.Point(53, 84);
            this.dateTimePickerSince.Name = "dateTimePickerSince";
            this.dateTimePickerSince.Size = new System.Drawing.Size(179, 22);
            this.dateTimePickerSince.TabIndex = 14;
            this.dateTimePickerSince.Value = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            // 
            // label8
            // 
            this.label8.ForeColor = System.Drawing.Color.LightGray;
            this.label8.Location = new System.Drawing.Point(3, 112);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 20);
            this.label8.Text = "Until:";
            // 
            // label7
            // 
            this.label7.ForeColor = System.Drawing.Color.LightGray;
            this.label7.Location = new System.Drawing.Point(3, 84);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 20);
            this.label7.Text = "Since:";
            // 
            // txtReferencing
            // 
            this.txtReferencing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtReferencing.Location = new System.Drawing.Point(55, 57);
            this.txtReferencing.Name = "txtReferencing";
            this.txtReferencing.Size = new System.Drawing.Size(176, 21);
            this.txtReferencing.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.ForeColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(2, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 20);
            this.label4.Text = "Ref:";
            // 
            // txtTo
            // 
            this.txtTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTo.Location = new System.Drawing.Point(55, 30);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(176, 21);
            this.txtTo.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.ForeColor = System.Drawing.Color.LightGray;
            this.label5.Location = new System.Drawing.Point(2, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 20);
            this.label5.Text = "To:";
            // 
            // txtFrom
            // 
            this.txtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFrom.Location = new System.Drawing.Point(55, 3);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(176, 21);
            this.txtFrom.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.ForeColor = System.Drawing.Color.LightGray;
            this.label6.Location = new System.Drawing.Point(2, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 20);
            this.label6.Text = "From:";
            // 
            // panelBottom
            // 
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBottom.BackColor = System.Drawing.Color.Black;
            this.panelBottom.Controls.Add(this.lblMore);
            this.panelBottom.Controls.Add(this.lblWords);
            this.panelBottom.Location = new System.Drawing.Point(5, 241);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(234, 26);
            // 
            // lblMore
            // 
            this.lblMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMore.Location = new System.Drawing.Point(167, 6);
            this.lblMore.Name = "lblMore";
            this.lblMore.Size = new System.Drawing.Size(64, 19);
            this.lblMore.TabIndex = 2;
            this.lblMore.Text = "More";
            this.lblMore.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblMore.Click += new System.EventHandler(this.lblMore_Click);
            // 
            // lblWords
            // 
            this.lblWords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWords.Location = new System.Drawing.Point(3, 6);
            this.lblWords.Name = "lblWords";
            this.lblWords.Size = new System.Drawing.Size(64, 19);
            this.lblWords.TabIndex = 0;
            this.lblWords.Text = "Words";
            this.lblWords.Click += new System.EventHandler(this.lblWords_Click);
            // 
            // AdvancedSearchSmartphoneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.panelMore);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelWords);
            this.Menu = this.mainMenu1;
            this.Name = "AdvancedSearchSmartphoneForm";
            this.Text = "Advanced Search";
            this.panelWords.ResumeLayout(false);
            this.panelMore.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelWords;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.LinkLabel lblWords;
        private System.Windows.Forms.LinkLabel lblMore;
        private System.Windows.Forms.TextBox txtHashtag;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtPhrase;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtNone;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAny;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelMore;
        private System.Windows.Forms.TextBox txtReferencing;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFrom;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dateTimePickerUntil;
        private System.Windows.Forms.DateTimePicker dateTimePickerSince;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.MenuItem menuOK;
        private System.Windows.Forms.MenuItem menuCancel;
    }
}