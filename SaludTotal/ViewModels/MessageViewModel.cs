using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaludTotal.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {

        private string _message;
        public string Message{
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMessage));
            }
        }

        private bool HasMessage => !string.IsNullOrEmpty(Message);
    }
}
