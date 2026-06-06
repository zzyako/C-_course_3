using System;

namespace ReflectionApp.Models
{
    /// <summary>
    /// Модель описания класса, реализующего интерфейс IFileSystemItem.
    /// </summary>
    public class ClassInfoModel
    {
        public ClassInfoModel(Type classType)
        {
            ClassType = classType;
        }

        public Type ClassType { get; }

        public string ClassName => ClassType.Name;

        public string FullName => ClassType.FullName;

        public string BaseTypeName => ClassType.BaseType?.Name ?? "—";

        public bool IsAbstract => ClassType.IsAbstract;

        public override string ToString()
        {
            return ClassName;
        }
    }
}
