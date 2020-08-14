using OptimizationLogic;
using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace robot_simulator.ViewModels
{


    public class WarehouseItemViewModel : BaseViewModel
    {
        private int row;
        private int col;
        private string stateStr;
        private string posiionCode;

        public int Row
        {
            get { return row; }

            set
            {
                if (row != value)
                {
                    row = value;
                    OnPropertyChanged(nameof(Row));
                }
            }
        }
        public int Col
        {
            get => col; set
            {
                if (col != value)
                {
                    col = value;
                    OnPropertyChanged(nameof(Col));
                }
            }
        }
        public string StateStr
        {
            get => stateStr; set
            {
                if (stateStr != value)
                {
                    stateStr = value;
                    OnPropertyChanged(nameof(StateStr));
                }
            }
        }
        public string PositionCode { get => posiionCode; set
            {
                if (posiionCode != value)
                {
                    posiionCode = value;
                    OnPropertyChanged(nameof(PositionCode));
                }
            }
        }
    }

}
