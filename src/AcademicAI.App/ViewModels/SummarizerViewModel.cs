using System.Windows;
using AcademicAI.Academic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class SummarizerViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private string _pdfText = "";
    [ObservableProperty] private bool _isSummarizing;
    [ObservableProperty] private string _summaryText = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isPdfMode;
    [ObservableProperty] private string _summaryLength = "Medium";

    public List<string> AvailableLengths { get; } = ["Brief", "Medium", "Detailed"];

    public bool HasResult => !string.IsNullOrEmpty(SummaryText);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task Summarize()
    {
        var text = IsPdfMode ? PdfText : InputText;
        if (string.IsNullOrWhiteSpace(text) || IsSummarizing) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsSummarizing = true;
        ErrorMessage = "";
        SummaryText = "";
        try
        {
            var processor = App.Services.GetRequiredService<AcademicTextProcessor>();
            SummaryText = await processor.SummarizeAsync(text.Trim());

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Summarization failed: {ex.Message}";
        }
        finally
        {
            IsSummarizing = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void CopyResult()
    {
        if (!string.IsNullOrEmpty(SummaryText))
            Clipboard.SetText(SummaryText);
    }
}
