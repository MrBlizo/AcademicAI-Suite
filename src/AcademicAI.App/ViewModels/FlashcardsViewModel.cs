using System.Windows;
using AcademicAI.Academic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class FlashcardsViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _resultText = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private int _cardCount = 5;
    [ObservableProperty] private string _difficulty = "Medium";

    public List<string> AvailableDifficulties { get; } = ["Easy", "Medium", "Hard"];

    public bool HasResult => !string.IsNullOrEmpty(ResultText);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task GenerateCards()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsProcessing) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsProcessing = true;
        ErrorMessage = "";
        ResultText = "";
        try
        {
            var processor = App.Services.GetRequiredService<FlashcardGenerator>();
            ResultText = await processor.GenerateAsync(InputText, CardCount, Difficulty);

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Generating flashcards failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void CopyResult()
    {
        if (!string.IsNullOrEmpty(ResultText))
            Clipboard.SetText(ResultText);
    }
}
