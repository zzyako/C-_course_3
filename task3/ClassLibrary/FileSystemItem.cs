namespace ClassLibrary
{
    /// <summary>
    /// Абстрактный базовый класс элемента файловой системы.
    /// </summary>
    public abstract class FileSystemItem : IFileSystemItem
    {
        protected FileSystemItem(string name, string parentPath)
        {
            Name = name;
            ParentPath = parentPath;
        }

        public string Name { get; set; }

        public string ParentPath { get; private set; }

        public string Location
        {
            get
            {
                if (string.IsNullOrEmpty(ParentPath))
                {
                    return Name;
                }

                return ParentPath + "\\" + Name;
            }
        }

        public abstract FileSystemItemType ItemType { get; }

        public abstract long Size { get; }

        public abstract string GetInfo();

        public void SetParentPath(string newParentPath)
        {
            ParentPath = newParentPath;
        }

        public static FileSystemItem CopyItem(FileSystemItem source, string destinationPath)
        {
            if (source == null)
            {
                return null;
            }

            if (source is FileItem fileItem)
            {
                return new FileItem(fileItem.Name, destinationPath, fileItem.Size);
            }

            if (source is FolderItem folderItem)
            {
                FolderItem copy = new FolderItem(folderItem.Name, destinationPath);

                foreach (FileSystemItem child in folderItem.Items)
                {
                    FileSystemItem childCopy = CopyItem(child, copy.Location);
                    copy.AddItem(childCopy);
                }

                return copy;
            }

            return null;
        }

        public static void MoveItem(FileSystemItem item, string destinationPath)
        {
            if (item == null)
            {
                return;
            }

            item.SetParentPath(destinationPath);
        }
    }
}
