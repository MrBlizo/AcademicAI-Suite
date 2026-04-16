using Wpf.Ui.Controls;

string[] names =
[
    "Home24", "HatGraduation24", "CalendarMonth24", "Search24",
    "DocumentBulletList24", "DocumentEdit24", "Translate24", "People24",
    "DocumentText24", "Bot24", "Settings24", "CardUi24", "School24",
    "Robot24", "Warning24", "Alert24", "DocumentAdd24", "Checkmark24",
    "Copy24", "Save24"
];

foreach (var name in names)
{
    bool ok = Enum.TryParse<SymbolRegular>(name, out var result);
    Console.WriteLine($"{name,-28} {(ok ? "VALID" : "INVALID")}");
}
