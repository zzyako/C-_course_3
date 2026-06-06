using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ReflectionApp.Models;

namespace ReflectionApp.Views
{
    public partial class MainWindow : Window
    {
        private const string TargetInterfaceName = "IFileSystemItem";

        private static readonly string[] SystemMethodNames =
        {
            "GetType", "ToString", "Equals", "GetHashCode", "MemberwiseClone", "Finalize"
        };

        private readonly List<ClassInfoModel> _classes = new List<ClassInfoModel>();
        private readonly List<MethodInfoModel> _methods = new List<MethodInfoModel>();
        private ClassInfoModel _selectedClass;
        private MethodInfoModel _selectedMethod;

        public MainWindow()
        {
            InitializeComponent();
        }

        // ── Обработчики UI ───────────────────────────────────────────────────

        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearAll();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Библиотека классов (*.dll)|*.dll",
                Title = "Выберите библиотеку классов"
            };

            if (dialog.ShowDialog() == true)
            {
                PathTextBox.Text = dialog.FileName;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            string path = PathTextBox.Text.Trim();

            if (string.IsNullOrEmpty(path))
            {
                StatusText.Text = "Укажите путь к библиотеке.";
                return;
            }

            ClearAll();

            try
            {
                Assembly assembly = Assembly.LoadFrom(path);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    foreach (Type iface in type.GetInterfaces())
                    {
                        if (iface.Name == TargetInterfaceName)
                        {
                            _classes.Add(new ClassInfoModel(type));
                            break;
                        }
                    }
                }

                ClassListBox.ItemsSource = null;
                ClassListBox.ItemsSource = _classes;

                StatusText.Text = _classes.Count == 0
                    ? $"Классы с интерфейсом {TargetInterfaceName} не найдены."
                    : $"Загружено классов: {_classes.Count}. Выберите класс из списка.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }

        private void ClassListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedClass = ClassListBox.SelectedItem as ClassInfoModel;
            LoadMethods();
        }

        private void MethodListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMethod = MethodListBox.SelectedItem as MethodInfoModel;
            LoadParameters();
            ExecuteButton.IsEnabled = _selectedMethod != null;
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteMethod();
        }

        // ── Логика ───────────────────────────────────────────────────────────

        private void ClearAll()
        {
            _classes.Clear();
            _methods.Clear();
            _selectedClass = null;
            _selectedMethod = null;
            ClassListBox.ItemsSource = null;
            MethodListBox.ItemsSource = null;
            ParametersPanel.ItemsSource = null;
            ResultText.Text = string.Empty;
            ExecuteButton.IsEnabled = false;
            FooterText.Text = "Выбранный класс: —  |  Базовый тип: —";
        }

        private void LoadMethods()
        {
            _methods.Clear();
            _selectedMethod = null;
            MethodListBox.ItemsSource = null;
            ParametersPanel.ItemsSource = null;
            ResultText.Text = string.Empty;
            ExecuteButton.IsEnabled = false;

            if (_selectedClass == null)
            {
                return;
            }

            MethodInfo[] allMethods = _selectedClass.ClassType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (MethodInfo method in allMethods)
            {
                if (method.IsSpecialName)
                {
                    continue;
                }

                bool isSystem = Array.IndexOf(SystemMethodNames, method.Name) >= 0;

                if (isSystem)
                {
                    continue;
                }

                bool supported = true;

                foreach (ParameterInfo param in method.GetParameters())
                {
                    if (!IsSupportedType(param.ParameterType))
                    {
                        supported = false;
                        break;
                    }
                }

                if (supported)
                {
                    _methods.Add(new MethodInfoModel(method));
                }
            }

            MethodListBox.ItemsSource = _methods;

            FooterText.Text = $"Выбранный класс: {_selectedClass.ClassName}  |  " +
                              $"Базовый тип: {_selectedClass.BaseTypeName}  |  " +
                              $"Методов: {_methods.Count}";

            StatusText.Text = $"Класс {_selectedClass.ClassName} загружен. Выберите метод.";
        }

        private void LoadParameters()
        {
            ParametersPanel.ItemsSource = null;

            if (_selectedMethod == null)
            {
                return;
            }

            List<FrameworkElement> controls = new List<FrameworkElement>();

            foreach (MethodParameterModel param in _selectedMethod.Parameters)
            {
                StackPanel row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 3) };

                TextBlock label = new TextBlock
                {
                    Text = $"{param.ParameterName} ({param.TypeName}):",
                    Width = 160,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12
                };

                TextBox input = new TextBox
                {
                    Width = 200,
                    Padding = new Thickness(5, 3, 5, 3),
                    FontSize = 12,
                    Tag = param
                };

                input.TextChanged += (s, e) =>
                {
                    TextBox tb = (TextBox)s;
                    MethodParameterModel p = (MethodParameterModel)tb.Tag;
                    p.InputValue = tb.Text;
                };

                row.Children.Add(label);
                row.Children.Add(input);
                controls.Add(row);
            }

            if (controls.Count == 0)
            {
                controls.Add(new TextBlock
                {
                    Text = "(параметров нет)",
                    FontSize = 12,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            ParametersPanel.ItemsSource = controls;
        }

        private void ExecuteMethod()
        {
            if (_selectedClass == null || _selectedMethod == null)
            {
                return;
            }

            try
            {
                object instance = CreateInstance(_selectedClass.ClassType);

                if (instance == null)
                {
                    ResultText.Text = "Не удалось создать экземпляр класса.";
                    return;
                }

                object[] args = BuildArguments(_selectedMethod);
                object returnValue = _selectedMethod.MethodInfo.Invoke(instance, args);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Класс:  {_selectedClass.ClassName}");
                sb.AppendLine($"Метод:  {_selectedMethod.DisplaySignature}");
                sb.AppendLine();

                if (_selectedMethod.MethodInfo.ReturnType == typeof(void))
                {
                    sb.AppendLine("Метод выполнен (void).");
                    sb.AppendLine();
                    sb.AppendLine("Состояние объекта:");
                    sb.Append(GetObjectState(instance));
                }
                else
                {
                    sb.AppendLine($"Результат:  {returnValue ?? "null"}");
                }

                ResultText.Text = sb.ToString();
                StatusText.Text = "Метод выполнен успешно.";
            }
            catch (TargetInvocationException tie)
            {
                ResultText.Text = $"Ошибка выполнения:\n{tie.InnerException?.Message ?? tie.Message}";
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Ошибка: {ex.Message}";
            }
        }

        // ── Вспомогательные методы ───────────────────────────────────────────

        private object CreateInstance(Type type)
        {
            foreach (ConstructorInfo ctor in type.GetConstructors())
            {
                ParameterInfo[] ctorParams = ctor.GetParameters();

                if (ctorParams.Length == 0)
                {
                    return Activator.CreateInstance(type);
                }

                object[] args = new object[ctorParams.Length];
                bool canCreate = true;

                for (int i = 0; i < ctorParams.Length; i++)
                {
                    Type t = ctorParams[i].ParameterType;

                    if (t == typeof(string))       args[i] = "sample";
                    else if (t == typeof(int))     args[i] = 0;
                    else if (t == typeof(long))    args[i] = 0L;
                    else if (t == typeof(bool))    args[i] = false;
                    else if (!t.IsValueType)       args[i] = null;
                    else { canCreate = false; break; }
                }

                if (canCreate)
                {
                    return ctor.Invoke(args);
                }
            }

            return null;
        }

        private object[] BuildArguments(MethodInfoModel methodModel)
        {
            ParameterInfo[] paramInfos = methodModel.MethodInfo.GetParameters();
            object[] args = new object[paramInfos.Length];

            for (int i = 0; i < paramInfos.Length; i++)
            {
                string raw = methodModel.Parameters[i].InputValue ?? string.Empty;
                args[i] = Convert.ChangeType(raw, paramInfos[i].ParameterType);
            }

            return args;
        }

        private bool IsSupportedType(Type type)
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

            foreach (PropertyInfo prop in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead) continue;

                try
                {
                    object val = prop.GetValue(instance);
                    sb.AppendLine($"  {prop.Name}: {val}");
                }
                catch
                {
                    sb.AppendLine($"  {prop.Name}: <ошибка>");
                }
            }

            return sb.ToString();
        }
    }
}
