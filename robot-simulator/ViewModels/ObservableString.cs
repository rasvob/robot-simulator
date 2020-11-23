using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace robot_simulator.ViewModels
{
    public class ObservableString: BaseViewModel
    {
        private string _value;

        public string Value
        {
            get { return _value; }

            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }
    }
}
