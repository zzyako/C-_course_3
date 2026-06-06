using System.Collections.Generic;
using System.Text;

namespace ClassLibrary
{
    /// <summary>
    /// Представляет папку в файловой системе.
    /// </summary>
    public class FolderItem : FileSystemItem
    {
        private readonly List<FileSystemItem> _items;

        public FolderItem(string name, string parentPath)
            : base(name, parentPath)
        {
            _items = new List<FileSystemItem>();
        }

        public IReadOnlyList<FileSystemItem> Items => _items.AsReadOnly();

        public override FileSystemItemType ItemType => FileSystemItemType.Folder;

        public override long Size
        {
            get
            {
                long total = 0;

                foreach (FileSystemItem item in _items)
                {
                    total += item.Size;
                }

                return total;
            }
        }

        public int GetItemCount()
        {
            return _items.Count;
        }

        public int GetFileCount()
        {
            int count = 0;

            foreach (FileSystemItem item in _items)
            {
                if (item is FileItem)
                {
                    count++;
                }
                else if (item is FolderItem subfolder)
                {
                    count += subfolder.GetFileCount();
                }
            }

            return count;
        }

        public bool ContainsItem(string itemName)
        {
            foreach (FileSystemItem item in _items)
            {
                if (item.Name == itemName)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddItem(FileSystemItem item)
        {
            if (item == null || _items.Contains(item))
            {
                return;
            }

            _items.Add(item);
            item.SetParentPath(Location);
        }

        public bool RemoveItem(string itemName)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Name == itemName)
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Папка: {Name}");
            sb.AppendLine($"Расположение: {Location}");
            sb.AppendLine($"Элементов: {GetItemCount()}");
            sb.AppendLine($"Файлов (всего): {GetFileCount()}");
            sb.Append($"Общий размер: {Size} байт");
            return sb.ToString();
        }
    }
}
