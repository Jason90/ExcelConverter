using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConsumptionConverter
{
    public partial class ConverterForm : Form
    {
        public ConverterForm()
        {
            InitializeComponent();

            this.saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1; 
            saveFileDialog.DefaultExt = "xlsx";
        }


        private void btnConvert_Click(object sender, EventArgs e)
        {
            ExcelHelper.Convert(this.txtSource.Text, this.txtTarget.Text);
        }

        private void btnOpenSource_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.txtSource.Text = this.openFileDialog.FileName;
            }
        }

        private void btnOpenTarget_Click(object sender, EventArgs e)
        {
            
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.txtTarget.Text = this.saveFileDialog.FileName;
            }
        }
    }
}
