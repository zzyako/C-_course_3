using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace ReflectionApp.Models
{
    /// <summary>
    /// Модель описания метода класса, полученного через рефлексию.
    /// </summary>
    public class MethodInfoModel
    {
        public MethodInfoModel(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            Parameters = new ObservableCollection<MethodParameterModel>();

            foreach (ParameterInfo param in methodInfo.GetParameters())
            {
                Parameters.Add(new MethodParameterModel(param.Name, param.ParameterType.Name));
            }
        }

        public MethodInfo MethodInfo { get; }

        public string MethodName => MethodInfo.Name;

        public string ReturnTypeName => MethodInfo.ReturnType.Name;

        public ObservableCollection<MethodParameterModel> Parameters { get; }

        public string DisplaySignature
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ReturnTypeName);
                sb.Append(" ");
                sb.Append(MethodName);
                sb.Append("(");

                for (int i = 0; i < Parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(Parameters[i].TypeName);
                    sb.Append(" ");
                    sb.Append(Parameters[i].ParameterName);
                }

                sb.Append(")");
                return sb.ToString();
            }
        }
    }
}
