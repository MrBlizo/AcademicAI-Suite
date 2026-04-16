using System.Windows;
using AcademicAI.Academic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class GrammarCheckViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _resultText = "";
    [ObservableProperty] private string _errorMessage = "";

    public bool HasResult => !string.IsNullOrEmpty(ResultText);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task CheckGrammar()
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
            var processor = App.Services.GetRequiredService<GrammarChecker>();
            ResultText = await processor.CheckAsync(InputText);

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Grammar check failed: {ex.Message}";
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
