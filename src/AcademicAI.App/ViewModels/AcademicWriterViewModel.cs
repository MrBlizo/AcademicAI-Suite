using System.IO;
using System.Windows;
using AcademicAI.Academic;
using AcademicAI.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace AcademicAI.App.ViewModels;

public partial class AcademicWriterViewModel : ObservableObject
{
    [ObservableProperty] private string _prompt = "";
    [ObservableProperty] private string _selectedType = "Essay";
    [ObservableProperty] private string _selectedTone = "Formal";
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private string _generatedText = "";
    [ObservableProperty] private string _errorMessage = "";

    public List<string> AvailableTypes { get; } = ["Essay", "Research Paper", "Literature Review", "Thesis"];
    public List<string> AvailableTones { get; } = ["Formal", "Semi-Formal", "Informal"];

    public bool HasResult => !string.IsNullOrEmpty(GeneratedText);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task Generate()
    {
        if (string.IsNullOrWhiteSpace(Prompt) || IsGenerating) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsGenerating = true;
        ErrorMessage = "";
        GeneratedText = "";
        try
        {
            var processor = App.Services.GetRequiredService<AcademicTextProcessor>();
            GeneratedText = await processor.GenerateWritingAsync(Prompt.Trim(), SelectedType, SelectedTone);

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Generation failed: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void CopyResult()
    {
        if (!string.IsNullOrEmpty(GeneratedText))
            Clipboard.SetText(GeneratedText);
    }

    [RelayCommand]
    private void SaveResult()
    {
        if (string.IsNullOrEmpty(GeneratedText)) return;

        var dialog = new SaveFileDialog
        {
            Filter = "Text Files|*.txt|Markdown Files|*.md|All Files|*.*",
            DefaultExt = ".txt",
            FileName = $"{SelectedType}_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(dialog.FileName, GeneratedText);
            }
            catch (Exception ex)
            {
                var notificationService = App.Services.GetRequiredService<INotificationService>();
                notificationService.Show("Error", $"Failed to save file: {ex.Message}");
            }
        }
    }
}
