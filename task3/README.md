# Задача 3 — Инспектор классов через рефлексию

## Структура решения

```
Task3.sln
├── ClassLibrary/           ← Отдельная библиотека классов
│   ├── IFileSystemItem.cs  ← Интерфейс (маркер для рефлексии)
│   ├── FileSystemItemType.cs
│   ├── FileSystemItem.cs   ← Абстрактный базовый класс
│   ├── FileItem.cs         ← Конкретный класс «Файл»
│   └── FolderItem.cs       ← Конкретный класс «Папка»
│
└── ReflectionApp/          ← WPF-приложение (MVVM)
    ├── App.xaml / App.xaml.cs
    ├── Commands/
    │   └── RelayCommand.cs
    ├── Models/
    │   ├── ClassInfoModel.cs
    │   ├── MethodInfoModel.cs
    │   └── MethodParameterModel.cs
    ├── ViewModels/
    │   ├── ViewModelBase.cs
    │   └── MainViewModel.cs
    └── Views/
        ├── MainWindow.xaml
        └── MainWindow.xaml.cs
```

## Как собрать и запустить

### Требования
- Visual Studio 2022 (или .NET SDK 4.8 + MSBuild)
- .NET Framework 4.8

### Шаги

1. **Открыть решение** `Task3.sln` в Visual Studio.

2. **Собрать ClassLibrary** (Правая кнопка → Собрать).  
   DLL появится в `ClassLibrary\bin\Debug\ClassLibrary.dll`.

3. **Запустить ReflectionApp** (установить как стартовый проект → F5).

4. **В интерфейсе:**
   - Нажать **Обзор…** и выбрать `ClassLibrary.dll`
   - Нажать **Загрузить** — список классов заполнится
   - Выбрать класс (`FileItem` или `FolderItem`) — список методов заполнится динамически
   - Выбрать метод — при наличии параметров появятся поля ввода
   - Заполнить параметры и нажать **▶ Выполнить**
   - Результат и состояние объекта отображаются справа

## Реализованные требования

| Требование | Реализация |
|---|---|
| Классы в отдельном модуле | Проект `ClassLibrary` |
| Ввод пути к библиотеке | TextBox + кнопка «Обзор…» |
| Поиск классов по интерфейсу | `IFileSystemItem` через рефлексию |
| Список классов | ListBox слева |
| Динамическая загрузка методов | `GetMethods()` при выборе класса |
| Ввод параметров | ItemsControl с TextBox на каждый параметр |
| Кнопка «Выполнить» | Создаёт объект и вызывает метод |
| Соглашения об именовании | camelCase для полей, PascalCase для методов/свойств |
| Без инициализации данных в конструкторе | ✓ |
| Оконное приложение | WPF |
| MVVM | ViewModelBase, MainViewModel, RelayCommand |

## Методы, доступные для вызова

### FileItem
- `GetExtension()` — расширение файла
- `IsLargerThan(long thresholdBytes)` — сравнение размера
- `FormatSize()` — форматированный размер (B/KB/MB/GB)
- `GetInfo()` — полное описание файла

### FolderItem
- `GetItemCount()` — количество элементов
- `GetFileCount()` — количество файлов рекурсивно
- `ContainsItem(string itemName)` — поиск по имени
- `GetInfo()` — полное описание папки
