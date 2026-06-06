using System.ComponentModel;

namespace ReflectionApp.Models
{
    /// <summary>
    /// Модель одного параметра метода, введённого пользователем.
    /// </summary>
    public class MethodParameterModel : INotifyPropertyChanged
    {
        private string _inputValue;

        public MethodParameterModel(string parameterName, string typeName)
        {
            ParameterName = parameterName;
            TypeName = typeName;
            _inputValue = string.Empty;
        }

        public string ParameterName { get; }

        public string TypeName { get; }

        public string DisplayLabel => $"{ParameterName} ({TypeName}):";

        public string InputValue
        {
            get => _inputValue;
            set
            {
                if (_inputValue != value)
                {
                    _inputValue = value;
                    OnPropertyChanged(nameof(InputValue));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
