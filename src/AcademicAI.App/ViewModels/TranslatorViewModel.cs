using System.Collections.ObjectModel;
using System.Windows;
using AcademicAI.Academic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public class TranslationRecord
{
    public string SourceLanguage { get; set; } = "";
    public string TargetLanguage { get; set; } = "";
    public string SourceText { get; set; } = "";
    public string TranslatedText { get; set; } = "";
}

public partial class TranslatorViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private string _sourceLanguage = "English";
    [ObservableProperty] private string _targetLanguage = "Arabic";
    [ObservableProperty] private bool _isTranslating;
    [ObservableProperty] private string _translatedText = "";
    [ObservableProperty] private string _errorMessage = "";

    public ObservableCollection<TranslationRecord> TranslationHistory { get; } = [];

    public List<string> Languages { get; } = ["English", "Arabic", "French", "Spanish", "German", "Chinese", "Japanese", "Korean"];

    public bool HasResult => !string.IsNullOrEmpty(TranslatedText);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task Translate()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsTranslating) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsTranslating = true;
        ErrorMessage = "";
        TranslatedText = "";
        try
        {
            var processor = App.Services.GetRequiredService<AcademicTextProcessor>();
            TranslatedText = await processor.TranslateAsync(InputText.Trim(), SourceLanguage, TargetLanguage);

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
            if (!string.IsNullOrEmpty(TranslatedText))
            {
                TranslationHistory.Insert(0, new TranslationRecord
                {
                    SourceLanguage = SourceLanguage,
                    TargetLanguage = TargetLanguage,
                    SourceText = InputText.Trim(),
                    TranslatedText = TranslatedText
                });
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Translation failed: {ex.Message}";
        }
        finally
        {
            IsTranslating = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void SwapLanguages()
    {
        var temp = SourceLanguage;
        SourceLanguage = TargetLanguage;
        TargetLanguage = temp;
    }

    [RelayCommand]
    private void CopyResult()
    {
        if (!string.IsNullOrEmpty(TranslatedText))
            Clipboard.SetText(TranslatedText);
    }
}
