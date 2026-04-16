using System;
using System.IO;
using System.Linq;
using System.Reflection;

var asmPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    @".nuget\packages\wpf-ui\3.0.5\lib\net8.0-windows7.0\Wpf.Ui.dll");

var asm = Assembly.LoadFrom(asmPath);

foreach (var type in asm.GetExportedTypes().Where(t => t.Name == "FluentWindow" || t.Name == "TitleBar" || t.Name == "PasswordBox"))
{
    Console.WriteLine($"\n=== {type.FullName} ===");
    foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        Console.WriteLine($"  Property: {prop.Name} ({prop.PropertyType.Name})");
}
