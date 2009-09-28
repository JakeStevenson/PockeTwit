using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;

namespace PockeTwit
{
    public partial class AdvancedSearchSmartphoneForm : Form
    {
        private string _query;
        public string Query { get { return _query; } }
        public AdvancedSearchSmartphoneForm()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            } 
        }

        private void lblWords_Click(object sender, EventArgs e)
        {
            panelWords.Visible = true;
            panelMore.Visible = false;
        }
        
        private void lblMore_Click(object sender, EventArgs e)
        {
            panelWords.Visible = false;
            panelMore.Visible = true;
        }

        bool _hasValidSinceDate;
        private void dateTimePickerSince_ValueChanged(object sender, EventArgs e)
        {
            _hasValidSinceDate = true;
        }

        private bool _hasValidUntilDate;
        private void dateTimePickerUntil_ValueChanged(object sender, EventArgs e)
        {
            _hasValidUntilDate = true;
        }
        private void ProcessTextBox(string format, string txt)
        {
            Debug.Assert(_queryText != null);
            _queryText.AppendFormat(CultureInfo.InvariantCulture, format, System.Web.HttpUtility.UrlEncode(txt));
        }

        private void ProcessDate(string format, DateTime date)
        {
            Debug.Assert(_queryText != null);

            _queryText.AppendFormat(CultureInfo.InvariantCulture, format, date.ToString("yyyy-MM-dd"));
        }

        private StringBuilder _queryText;
        
        private void menuOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            // parse controls to combine query
            _queryText = new StringBuilder();
            _queryText.Append("q=");

            if (!String.IsNullOrEmpty(txtAll.Text))
            {
                ProcessTextBox("&ands={0}", txtAll.Text);
            }
            if (!String.IsNullOrEmpty(txtAny.Text))
            {
                ProcessTextBox("&ors={0}", txtAny.Text);
            }
            if (!String.IsNullOrEmpty(txtNone.Text))
            {
                ProcessTextBox("&nots={0}", txtNone.Text);
            }
            if (!String.IsNullOrEmpty(txtFrom.Text))
            {
                ProcessTextBox("&from={0}", txtFrom.Text);
            }
            if (!String.IsNullOrEmpty(txtTo.Text))
            {
                ProcessTextBox("&to={0}", txtTo.Text);
            }
            if (!String.IsNullOrEmpty(txtReferencing.Text))
            {
                ProcessTextBox("&ref={0}", txtReferencing.Text);
            }
            if (!String.IsNullOrEmpty(txtPhrase.Text))
            {
                ProcessTextBox("&phrase={0}", txtPhrase.Text);
            }
            if (!String.IsNullOrEmpty(txtHashtag.Text))
            {
                ProcessTextBox("&tag={0}", txtHashtag.Text);
            }
            if (_hasValidSinceDate)
            {
                ProcessDate("&since={0}", dateTimePickerSince.Value);
            }
            if (_hasValidUntilDate)
            {
                ProcessDate("&until={0}", dateTimePickerUntil.Value);
            }
            _query = _queryText.ToString();

            // no input
            if (_query.Length == 3)
            {
                DialogResult = DialogResult.Cancel;
            }
            
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

 
    }
}