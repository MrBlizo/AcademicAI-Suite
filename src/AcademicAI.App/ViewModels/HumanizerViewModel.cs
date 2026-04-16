using System.Windows;
using AcademicAI.Humanizer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class HumanizerViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private string _selectedTone = "Academic";
    [ObservableProperty] private string _rewriteStrength = "Moderate";
    [ObservableProperty] private bool _isHumanizing;
    [ObservableProperty] private string _humanizedText = "";
    [ObservableProperty] private string _errorMessage = "";

    public List<string> AvailableTones { get; } = ["Academic", "Formal", "Semi-Formal", "Semi-Casual", "Casual"];
    public List<string> AvailableStrengths { get; } = ["Subtle", "Moderate", "Aggressive"];

    public bool HasResult => !string.IsNullOrEmpty(HumanizedText);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task Humanize()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsHumanizing) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsHumanizing = true;
        ErrorMessage = "";
        HumanizedText = "";
        try
        {
            var humanizer = App.Services.GetRequiredService<TextHumanizer>();
            HumanizedText = await humanizer.HumanizeAsync(InputText.Trim(), SelectedTone);

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Humanization failed: {ex.Message}";
        }
        finally
        {
            IsHumanizing = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void CopyResult()
    {
        if (!string.IsNullOrEmpty(HumanizedText))
            Clipboard.SetText(HumanizedText);
    }
}
