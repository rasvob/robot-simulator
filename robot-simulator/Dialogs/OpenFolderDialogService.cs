using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace robot_simulator.Dialogs
{
    public class OpenFolderDialogService: IOpenFileService
    {
        public string Open()
        {
            var fileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
            };

            var result = fileDialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                string file = fileDialog.FileName;
                fileDialog.ResetUserSelections();
                return file;
            }
            return null;
        }
    }
}
