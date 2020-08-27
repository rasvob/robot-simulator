using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace robot_simulator.Dialogs
{
    public class OpenFileDialogService
    {
        public string OpenFile()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt, *.csv) | *.txt; *.csv;"
            };

            var result = fileDialog.ShowDialog();
            if (result == true)
            {
                string file = fileDialog.FileName;
                fileDialog.Reset();
                return file;
            }
            return null;
        }
    }
}
