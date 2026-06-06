namespace ClassLibrary
{
    /// <summary>
    /// Интерфейс, который должны реализовывать все классы файловой системы,
    /// доступные для обнаружения через рефлексию.
    /// </summary>
    public interface IFileSystemItem
    {
        string Name { get; set; }

        long Size { get; }

        string Location { get; }

        string GetInfo();
    }
}
