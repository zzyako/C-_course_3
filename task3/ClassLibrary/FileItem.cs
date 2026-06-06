namespace ClassLibrary
{
    /// <summary>
    /// Представляет файл в файловой системе.
    /// </summary>
    public class FileItem : FileSystemItem
    {
        private long _sizeValue;

        public FileItem(string name, string parentPath, long size)
            : base(name, parentPath)
        {
            _sizeValue = size;
        }

        public long SizeValue
        {
            get => _sizeValue;
            set => _sizeValue = value;
        }

        public override FileSystemItemType ItemType => FileSystemItemType.File;

        public override long Size => _sizeValue;

        public string GetExtension()
        {
            int dotIndex = Name.LastIndexOf('.');

            if (dotIndex < 0)
            {
                return string.Empty;
            }

            return Name.Substring(dotIndex + 1).ToUpperInvariant();
        }

        public bool IsLargerThan(long thresholdBytes)
        {
            return _sizeValue > thresholdBytes;
        }

        public string FormatSize()
        {
            if (_sizeValue >= 1_073_741_824)
            {
                return $"{_sizeValue / 1_073_741_824.0:F2} GB";
            }

            if (_sizeValue >= 1_048_576)
            {
                return $"{_sizeValue / 1_048_576.0:F2} MB";
            }

            if (_sizeValue >= 1024)
            {
                return $"{_sizeValue / 1024.0:F2} KB";
            }

            return $"{_sizeValue} B";
        }

        public override string GetInfo()
        {
            return $"Файл: {Name}\nРасположение: {Location}\nРазмер: {FormatSize()}\nРасширение: {GetExtension()}";
        }
    }
}
