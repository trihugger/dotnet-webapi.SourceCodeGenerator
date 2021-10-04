using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SourceCodeGenerator
{
    /// <summary>
    /// Interaction logic for SelectField.xaml.
    /// </summary>
    public partial class SelectField : Window
    {
        private string _selectedField = string.Empty;

        public SelectField(List<string> fields, string message, string title)
        {
            InitializeComponent();
            labelMessage.Text = message;
            this.Title = title;

            comboFields.Items.Clear();
            foreach(string field in fields)
            {
                comboFields.Items.Add(field);
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedField = (string)comboFields.SelectedValue;
            this.Close();
        }

        public string SelectedField()
        {
            return _selectedField;
        }
    }
}
