using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace MdReader;

/// <summary>An offline, isolated browser surface for one Mermaid diagram.</summary>
internal sealed class MermaidDiagram : Grid
{
    private const string AssetHost = "mdreader.invalid";
    private readonly string _source;
    private readonly TextBox _fallback;
    private WebView2? _browser;
    private static Task<CoreWebView2Environment>? _environment;

    public MermaidDiagram(string source)
    {
        _source = source;
        MinHeight = 120;
        Margin = new Thickness(0, 8, 0, 8);
        _fallback = new TextBox
        {
            Text = "Loading Mermaid diagram…\n\n" + source,
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Children.Add(_fallback);
        Loaded += LoadDiagram;
        Unloaded += (_, _) =>
        {
            var browser = _browser;
            _browser = null;
            if (browser != null)
            {
                Children.Remove(browser);
                browser.Dispose();
            }
        };
    }

    private async void LoadDiagram(object sender, RoutedEventArgs e)
    {
        if (_browser != null) return;
        var browser = new WebView2 { Height = 280, Visibility = Visibility.Hidden };
        _browser = browser;
        Children.Add(browser);
        try
        {
            _environment ??= CoreWebView2Environment.CreateAsync(userDataFolder:
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MdReader", "WebView2"));
            var environment = await _environment;
            if (_browser != browser) return;
            await browser.EnsureCoreWebView2Async(environment);
            if (_browser != browser) return;
            var core = browser.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.AreHostObjectsAllowed = false;
            core.Settings.IsStatusBarEnabled = false;
            core.SetVirtualHostNameToFolderMapping(AssetHost,
                Path.Combine(AppContext.BaseDirectory, "Assets", "Mermaid"),
                CoreWebView2HostResourceAccessKind.DenyCors);
            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            core.WebResourceRequested += (_, args) =>
            {
                if (!IsAssetUri(args.Request.Uri))
                    args.Response = environment.CreateWebResourceResponse(null, 403, "Forbidden", "");
            };
            core.NavigationStarting += (_, args) =>
                args.Cancel = args.Uri != $"https://{AssetHost}/diagram.html";
            core.NewWindowRequested += (_, args) => args.Handled = true;
            core.DownloadStarting += (_, args) => args.Cancel = true;
            core.PermissionRequested += (_, args) => args.State = CoreWebView2PermissionState.Deny;
            core.NavigationCompleted += (_, args) =>
            {
                if (_browser != browser) return;
                if (!args.IsSuccess)
                {
                    ShowError("Could not load the bundled Mermaid renderer.");
                    return;
                }
                core.PostWebMessageAsString(_source);
            };
            core.WebMessageReceived += (_, args) =>
            {
                if (_browser != browser || !IsAssetUri(args.Source)) return;
                if (double.TryParse(args.WebMessageAsJson,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var height) && double.IsFinite(height))
                {
                    browser.Height = Math.Clamp(height, 120, 2000);
                    browser.Visibility = Visibility.Visible;
                    _fallback.Visibility = Visibility.Collapsed;
                }
            };
            core.Navigate($"https://{AssetHost}/diagram.html");
        }
        catch (Exception ex)
        {
            if (_browser == browser)
                ShowError("Mermaid rendering requires Microsoft Edge WebView2 Runtime. " + ex.Message);
        }
    }

    private static bool IsAssetUri(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps && uri.Host == AssetHost;

    private void ShowError(string message)
    {
        if (_browser != null) _browser.Visibility = Visibility.Collapsed;
        _fallback.Text = message + "\n\n" + _source;
        _fallback.Visibility = Visibility.Visible;
    }
}
