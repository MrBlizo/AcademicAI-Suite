using System.Collections.ObjectModel;
using System.Windows;
using AcademicAI.Academic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class CitationsViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private string _selectedStyle = "APA";
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private string _generatedCitation = "";
    [ObservableProperty] private string _errorMessage = "";

    public ObservableCollection<string> CitationHistory { get; } = [];

    public List<string> AvailableStyles { get; } = ["APA", "MLA", "Chicago", "Harvard", "Vancouver"];

    public bool HasResult => !string.IsNullOrEmpty(GeneratedCitation);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task GenerateCitation()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsGenerating) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsGenerating = true;
        ErrorMessage = "";
        GeneratedCitation = "";
        try
        {
            var processor = App.Services.GetRequiredService<AcademicTextProcessor>();
            GeneratedCitation = await processor.GenerateCitationAsync(InputText.Trim(), SelectedStyle);

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
            if (!string.IsNullOrEmpty(GeneratedCitation))
                CitationHistory.Insert(0, $"[{SelectedStyle}] {GeneratedCitation}");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Citation generation failed: {ex.Message}";
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
        if (!string.IsNullOrEmpty(GeneratedCitation))
            Clipboard.SetText(GeneratedCitation);
    }
}
