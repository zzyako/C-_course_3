using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using ReflectionApp.Commands;
using ReflectionApp.Models;

namespace ReflectionApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const string TargetInterfaceName = "IFileSystemItem";

        private static readonly string[] SystemMethodNames =
        {
            "GetType", "ToString", "Equals", "GetHashCode", "MemberwiseClone", "Finalize"
        };

        private string _libraryPath;
        private ClassInfoModel _selectedClass;
        private MethodInfoModel _selectedMethod;
        private string _statusMessage;
        private string _executionResult;

        public MainViewModel()
        {
            _libraryPath = string.Empty;
            _statusMessage = "Укажите путь к библиотеке классов и нажмите «Загрузить».";
            _executionResult = string.Empty;

            DiscoveredClasses = new ObservableCollection<ClassInfoModel>();
            AvailableMethods = new ObservableCollection<MethodInfoModel>();

            LoadLibraryCommand = new RelayCommand(
                execute: _ => LoadLibrary(),
                canExecute: _ => !string.IsNullOrWhiteSpace(LibraryPath));

            ExecuteMethodCommand = new RelayCommand(
                execute: _ => ExecuteSelectedMethod(),
                canExecute: _ => SelectedClass != null && SelectedMethod != null);

            BrowseFileCommand = new RelayCommand(
                execute: _ => BrowseForLibrary());
        }

        public string LibraryPath
        {
            get => _libraryPath;
            set
            {
                if (SetProperty(ref _libraryPath, value, nameof(LibraryPath)))
                {
                    DiscoveredClasses.Clear();
                    AvailableMethods.Clear();
                    SelectedClass = null;
                    SelectedMethod = null;
                }
            }
        }

        public ObservableCollection<ClassInfoModel> DiscoveredClasses { get; }

        public ObservableCollection<MethodInfoModel> AvailableMethods { get; }

        public ClassInfoModel SelectedClass
        {
            get => _selectedClass;
            set
            {
                _selectedClass = value;
                OnPropertyChanged(nameof(SelectedClass));
                OnSelectedClassChanged();
            }
        }

        public MethodInfoModel SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                _selectedMethod = value;
                OnPropertyChanged(nameof(SelectedMethod));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
        }

        public string ExecutionResult
        {
            get => _executionResult;
            set => SetProperty(ref _executionResult, value, nameof(ExecutionResult));
        }

        public ICommand LoadLibraryCommand { get; }

        public ICommand ExecuteMethodCommand { get; }

        public ICommand BrowseFileCommand { get; }

        private void LoadLibrary()
        {
            DiscoveredClasses.Clear();
            AvailableMethods.Clear();
            SelectedClass = null;
            SelectedMethod = null;
            ExecutionResult = string.Empty;

            try
            {
                Assembly assembly = Assembly.LoadFrom(LibraryPath);
                Type[] allTypes = assembly.GetTypes();

                foreach (Type type in allTypes)
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    bool implementsInterface = false;

                    foreach (Type iface in type.GetInterfaces())
                    {
                        if (iface.Name == TargetInterfaceName)
                        {
                            implementsInterface = true;
                            break;
                        }
                    }

                    if (implementsInterface)
                    {
                        DiscoveredClasses.Add(new ClassInfoModel(type));
                    }
                }

                if (DiscoveredClasses.Count == 0)
                {
                    StatusMessage = $"Классы, реализующие интерфейс {TargetInterfaceName}, не найдены.";
                }
                else
                {
                    StatusMessage = $"Загружено классов: {DiscoveredClasses.Count}. Выберите класс из списка.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки библиотеки: {ex.Message}";
            }
        }

        private void OnSelectedClassChanged()
        {
            AvailableMethods.Clear();
            SelectedMethod = null;
            ExecutionResult = string.Empty;

            if (_selectedClass == null)
            {
                StatusMessage = "Выберите класс из списка.";
                return;
            }

            MethodInfo[] methods = _selectedClass.ClassType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (MethodInfo method in methods)
            {
                if (method.IsSpecialName)
                {
                    continue;
                }

                bool isSystem = false;
                foreach (string sysName in SystemMethodNames)
                {
                    if (method.Name == sysName)
                    {
                        isSystem = true;
                        break;
                    }
                }

                if (isSystem)
                {
                    continue;
                }

                bool allParamsSupported = true;

                foreach (ParameterInfo param in method.GetParameters())
                {
                    if (!IsSupportedParameterType(param.ParameterType))
                    {
                        allParamsSupported = false;
                        break;
                    }
                }

                if (allParamsSupported)
                {
                    AvailableMethods.Add(new MethodInfoModel(method));
                }
            }

            StatusMessage = $"Класс: {_selectedClass.ClassName} | Базовый тип: {_selectedClass.BaseTypeName} | Методов: {AvailableMethods.Count}";
        }

        private void ExecuteSelectedMethod()
        {
            ExecutionResult = string.Empty;

            if (_selectedClass == null || _selectedMethod == null)
            {
                return;
            }

            try
            {
                object instance = CreateInstance(_selectedClass.ClassType);

                if (instance == null)
                {
                    ExecutionResult = "Не удалось создать экземпляр класса: нет подходящего конструктора.";
                    return;
                }

                object[] arguments = ConvertParameters(_selectedMethod);
                object returnValue = _selectedMethod.MethodInfo.Invoke(instance, arguments);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Класс: {_selectedClass.ClassName}");
                sb.AppendLine($"Метод: {_selectedMethod.DisplaySignature}");
                sb.AppendLine();

                if (_selectedMethod.MethodInfo.ReturnType == typeof(void))
                {
                    sb.AppendLine("Метод выполнен успешно (void).");
                    sb.AppendLine();
                    sb.AppendLine("Состояние объекта:");
                    sb.Append(GetObjectState(instance));
                }
                else
                {
                    sb.AppendLine($"Результат: {returnValue ?? "null"}");
                }

                ExecutionResult = sb.ToString();
                StatusMessage = "Метод выполнен успешно.";
            }
            catch (TargetInvocationException tie)
            {
                ExecutionResult = $"Ошибка при выполнении:\n{tie.InnerException?.Message ?? tie.Message}";
                StatusMessage = "Ошибка выполнения метода.";
            }
            catch (Exception ex)
            {
                ExecutionResult = $"Ошибка: {ex.Message}";
                StatusMessage = "Ошибка.";
            }
        }

        private void BrowseForLibrary()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Библиотека классов (*.dll)|*.dll",
                Title = "Выберите библиотеку классов"
            };

            if (dialog.ShowDialog() == true)
            {
                LibraryPath = dialog.FileName;
            }
        }

        private object CreateInstance(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();

            foreach (ConstructorInfo ctor in constructors)
            {
                ParameterInfo[] ctorParams = ctor.GetParameters();

                if (ctorParams.Length == 0)
                {
                    return Activator.CreateInstance(type);
                }

                bool canCreate = true;
                object[] args = new object[ctorParams.Length];

                for (int i = 0; i < ctorParams.Length; i++)
                {
                    Type paramType = ctorParams[i].ParameterType;

                    if (paramType == typeof(string))
                    {
                        args[i] = "sample";
                    }
                    else if (paramType == typeof(int))
                    {
                        args[i] = 0;
                    }
                    else if (paramType == typeof(long))
                    {
                        args[i] = 0L;
                    }
                    else if (paramType == typeof(bool))
                    {
                        args[i] = false;
                    }
                    else if (!paramType.IsValueType)
                    {
                        args[i] = null;
                    }
                    else
                    {
                        canCreate = false;
                        break;
                    }
                }

                if (canCreate)
                {
                    return ctor.Invoke(args);
                }
            }

            return null;
        }

        private object[] ConvertParameters(MethodInfoModel methodModel)
        {
            ParameterInfo[] paramInfos = methodModel.MethodInfo.GetParameters();
            object[] args = new object[paramInfos.Length];

            for (int i = 0; i < paramInfos.Length; i++)
            {
                string rawValue = methodModel.Parameters[i].InputValue;
                args[i] = Convert.ChangeType(rawValue, paramInfos[i].ParameterType);
            }

            return args;
        }

        private bool IsSupportedParameterType(Type type)
        {
            return type == typeof(string)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(double)
                || type == typeof(float)
                || type == typeof(bool)
                || type == typeof(decimal);
        }

        private string GetObjectState(object instance)
        {
            StringBuilder sb = new StringBuilder();
            Type type = instance.GetType();

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanRead)
                {
                    continue;
                }

                try
                {
                    object val = prop.GetValue(instance);
                    sb.AppendLine($"  {prop.Name}: {val}");
                }
                catch
                {
                    sb.AppendLine($"  {prop.Name}: <ошибка чтения>");
                }
            }

            return sb.ToString();
        }
    }
}
