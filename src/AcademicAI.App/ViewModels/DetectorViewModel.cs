using System.Windows;
using AcademicAI.Detection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class DetectorViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private string _pdfText = "";
    [ObservableProperty] private bool _isDetecting;
    [ObservableProperty] private double _aiProbability;
    [ObservableProperty] private string _detectionReasoning = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isPdfMode;

    public bool HasResult => !string.IsNullOrEmpty(DetectionReasoning) || AiProbability > 0;

    public string DetectionLevel => AiProbability switch
    {
        > 90 => "Very Likely AI-Generated",
        > 70 => "Likely AI-Generated",
        > 40 => "Possibly AI-Generated",
        > 20 => "Likely Human-Written",
        _ => "Very Likely Human-Written"
    };

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task Detect()
    {
        var text = IsPdfMode ? PdfText : InputText;
        if (string.IsNullOrWhiteSpace(text) || IsDetecting) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsDetecting = true;
        ErrorMessage = "";
        AiProbability = 0;
        DetectionReasoning = "";
        try
        {
            var detector = App.Services.GetRequiredService<AiDetector>();
            var result = await detector.DetectAsync(text.Trim());

            token.ThrowIfCancellationRequested();

            AiProbability = result.Probability * 100;
            DetectionReasoning = result.Reasoning;
            OnPropertyChanged(nameof(HasResult));
            OnPropertyChanged(nameof(DetectionLevel));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Detection failed: {ex.Message}";
        }
        finally
        {
            IsDetecting = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }
}
