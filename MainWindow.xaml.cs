using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DiscordTokenChecker.Models;
using DiscordTokenChecker.Services;

namespace DiscordTokenChecker;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<TokenResult> _allResults = new();
    private readonly ObservableCollection<TokenResult> _validResults = new();
    private readonly ObservableCollection<TokenResult> _invalidResults = new();
    private readonly ObservableCollection<TokenResult> _errorResults = new();
    private readonly CheckerEngine _engine = new();
    private readonly NotificationService _notifier = new();
    private List<string> _tokens = new();
    private List<string> _proxies = new();
    private string _activeTab = "All";

    public MainWindow()
    {
        InitializeComponent();
        dgResults.ItemsSource = _allResults;

        sliderThreads.ValueChanged += (s, e) =>
        {
            txtThreads.Text = ((int)sliderThreads.Value).ToString();
        };

        _engine.OnResult += result =>
        {
            Dispatcher.Invoke(() =>
            {
                _allResults.Add(result);
                switch (result.Status)
                {
                    case CheckStatus.Valid:
                        _validResults.Add(result);
                        break;
                    case CheckStatus.Invalid:
                        _invalidResults.Add(result);
                        break;
                    default:
                        _errorResults.Add(result);
                        break;
                }
            });

            // Send notifications for valid hits
            if (result.Status == CheckStatus.Valid)
                _ = _notifier.SendValidHitAsync(result);
        };

        _engine.OnStatsUpdate += (valid, invalid, errors, chk, cpm) =>
        {
            Dispatcher.Invoke(() =>
            {
                txtValid.Text = valid.ToString();
                txtInvalid.Text = invalid.ToString();
                txtErrors.Text = errors.ToString();
                txtCPM.Text = ((int)cpm).ToString();
                txtProgress.Text = $"{chk} / {_tokens.Count}";

                if (_tokens.Count > 0)
                    progressBar.Value = (double)chk / _tokens.Count * 100;
            });
        };

        _engine.OnComplete += () =>
        {
            Dispatcher.Invoke(() =>
            {
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                btnLoadTokens.IsEnabled = true;
                btnLoadProxies.IsEnabled = true;
                MessageBox.Show($"Checking complete!\n\nValid: {txtValid.Text}\nInvalid: {txtInvalid.Text}\nErrors: {txtErrors.Text}",
                    "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        };
    }

    // ── Tab switching ──
    private void SetActiveTab(string tab, Button activeBtn)
    {
        _activeTab = tab;

        // Reset all tab styles
        tabAll.Style = (Style)FindResource("TabButton");
        tabValid.Style = (Style)FindResource("TabButton");
        tabInvalid.Style = (Style)FindResource("TabButton");
        tabErrors.Style = (Style)FindResource("TabButton");

        // Set active
        activeBtn.Style = (Style)FindResource("TabButtonActive");

        // Switch DataGrid source
        dgResults.ItemsSource = tab switch
        {
            "Valid" => _validResults,
            "Invalid" => _invalidResults,
            "Errors" => _errorResults,
            _ => _allResults
        };
    }

    private void TabAll_Click(object sender, RoutedEventArgs e) => SetActiveTab("All", tabAll);
    private void TabValid_Click(object sender, RoutedEventArgs e) => SetActiveTab("Valid", tabValid);
    private void TabInvalid_Click(object sender, RoutedEventArgs e) => SetActiveTab("Invalid", tabInvalid);
    private void TabErrors_Click(object sender, RoutedEventArgs e) => SetActiveTab("Errors", tabErrors);

    // ── Load tokens ──
    private void BtnLoadTokens_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Load Token List"
        };

        if (dlg.ShowDialog() == true)
        {
            _tokens = File.ReadAllLines(dlg.FileName)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .ToList();

            txtTokenCount.Text = _tokens.Count.ToString();
            progressBar.Value = 0;
            txtProgress.Text = $"0 / {_tokens.Count}";
        }
    }

    // ── Load proxies ──
    private void BtnLoadProxies_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Load Proxy List (IP:PORT or IP:PORT:USER:PASS)"
        };

        if (dlg.ShowDialog() == true)
        {
            _proxies = File.ReadAllLines(dlg.FileName)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .Where(l => l.Contains(':'))
                .ToList();

            txtProxyCount.Text = _proxies.Count.ToString();
            txtProxyMode.Text = _proxies.Count > 0 ? "Rotating" : "Proxyless";
        }
    }

    // ── Start ──
    private async void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        if (_tokens.Count == 0)
        {
            MessageBox.Show("Load tokens first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _allResults.Clear();
        _validResults.Clear();
        _invalidResults.Clear();
        _errorResults.Clear();

        btnStart.IsEnabled = false;
        btnStop.IsEnabled = true;
        btnLoadTokens.IsEnabled = false;
        btnLoadProxies.IsEnabled = false;
        progressBar.Value = 0;

        // Configure notifications
        _notifier.DiscordWebhookUrl = txtWebhook.Text.Trim();
        _notifier.TelegramBotToken = txtTgToken.Text.Trim();
        _notifier.TelegramChatId = txtTgChatId.Text.Trim();

        _engine.SetProxies(_proxies);

        var threads = (int)sliderThreads.Value;
        await _engine.StartAsync(_tokens, threads);
    }

    // ── Stop ──
    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        _engine.Stop();
        btnStop.IsEnabled = false;
    }

    // ── Export ──
    private void BtnExportHits_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt",
            FileName = $"hits_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
            Title = "Export Valid Tokens"
        };

        if (dlg.ShowDialog() == true)
        {
            ResultExporter.ExportHits(dlg.FileName, _allResults);
            MessageBox.Show($"Exported {_allResults.Count(r => r.Status == CheckStatus.Valid)} hits!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnExportAll_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt",
            FileName = $"results_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
            Title = "Export All Results"
        };

        if (dlg.ShowDialog() == true)
        {
            ResultExporter.ExportAll(dlg.FileName, _allResults);
            MessageBox.Show($"Exported {_allResults.Count} results!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
