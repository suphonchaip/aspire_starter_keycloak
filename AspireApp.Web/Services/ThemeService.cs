using MudBlazor;
using Microsoft.JSInterop;

namespace AspireApp.Web.Services
{
    public interface IThemeService
    {
        event Action<bool>? ThemeChanged;
        bool IsDarkMode { get; }
        MudTheme CurrentTheme { get; }
        Task ToggleThemeAsync();
        Task LoadThemeAsync();
    }

    public class ThemeService : IThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isDarkMode = false;

        public event Action<bool>? ThemeChanged;
        public bool IsDarkMode => _isDarkMode;
        public MudTheme CurrentTheme => _isDarkMode ? CreateDarkTheme() : CreateLightTheme();

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task ToggleThemeAsync()
        {
            _isDarkMode = !_isDarkMode;
            await SaveThemeAsync();
            ThemeChanged?.Invoke(_isDarkMode);
        }

        public async Task LoadThemeAsync()
        {
            try
            {
                var savedTheme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme");
                _isDarkMode = savedTheme == "dark";
                ThemeChanged?.Invoke(_isDarkMode);
            }
            catch
            {
                // Default to light mode if localStorage is not available
                _isDarkMode = false;
            }
        }

        private async Task SaveThemeAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", _isDarkMode ? "dark" : "light");
            }
            catch
            {
                // Ignore localStorage errors
            }
        }

        public static MudTheme CreateLightTheme()
        {
            var theme = new MudTheme()
            {
                PaletteLight = new PaletteLight()
                {
                    Primary = "#1976d2",
                    Secondary = "#424242",
                    AppbarBackground = "#1976d2",
                    Background = "#f5f5f5",
                    BackgroundGray = "#fafafa",
                    Surface = "#ffffff",
                    DrawerBackground = "#ffffff",
                    DrawerText = "rgba(0,0,0, 0.87)",
                    DrawerIcon = "rgba(0,0,0, 0.54)",
                    AppbarText = "#ffffff",
                    TextPrimary = "rgba(0,0,0, 0.87)",
                    TextSecondary = "rgba(0,0,0, 0.6)",
                    ActionDefault = "rgba(0,0,0, 0.54)",
                    ActionDisabled = "rgba(0,0,0, 0.26)",
                    ActionDisabledBackground = "rgba(0,0,0, 0.12)",
                    Divider = "rgba(0,0,0, 0.12)",
                    DividerLight = "rgba(0,0,0, 0.06)",
                    TableLines = "rgba(0,0,0, 0.12)",
                    LinesDefault = "rgba(0,0,0, 0.12)",
                    LinesInputs = "rgba(0,0,0, 0.42)",
                    TextDisabled = "rgba(0,0,0, 0.38)",
                    Info = "#2196f3",
                    Success = "#4caf50",
                    Warning = "#ff9800",
                    Error = "#f44336",
                    Dark = "#424242"
                }
            };

            // Set the default font family for all typography - Srinakharinwirot first
            var fontFamily = new[] { "srinakharinwirot", "Sarabun", "Noto Sans Thai", "Angsana New", "AngsanaUPC", "Cordia New", "CordiaUPC", "TH SarabunPSK", "Arial Unicode MS", "sans-serif" };

            theme.Typography.Default.FontFamily = fontFamily;
            theme.Typography.H1.FontFamily = fontFamily;
            theme.Typography.H2.FontFamily = fontFamily;
            theme.Typography.H3.FontFamily = fontFamily;
            theme.Typography.H4.FontFamily = fontFamily;
            theme.Typography.H5.FontFamily = fontFamily;
            theme.Typography.H6.FontFamily = fontFamily;
            theme.Typography.Body1.FontFamily = fontFamily;
            theme.Typography.Body2.FontFamily = fontFamily;
            theme.Typography.Button.FontFamily = fontFamily;
            theme.Typography.Caption.FontFamily = fontFamily;
            theme.Typography.Subtitle1.FontFamily = fontFamily;
            theme.Typography.Subtitle2.FontFamily = fontFamily;
            theme.Typography.Overline.FontFamily = fontFamily;

            return theme;
        }

        public static MudTheme CreateDarkTheme()
        {
            var theme = new MudTheme()
            {
                PaletteDark = new PaletteDark()
                {
                    Primary = "#90caf9",
                    Secondary = "#f48fb1",
                    AppbarBackground = "#1e1e1e",
                    Background = "#121212",
                    BackgroundGray = "#1e1e1e",
                    Surface = "#1e1e1e",
                    DrawerBackground = "#1e1e1e",
                    DrawerText = "#ffffff",
                    DrawerIcon = "#ffffff",
                    AppbarText = "#ffffff",
                    TextPrimary = "#ffffff",
                    TextSecondary = "rgba(255,255,255, 0.7)",
                    ActionDefault = "rgba(255,255,255, 0.54)",
                    ActionDisabled = "rgba(255,255,255, 0.26)",
                    ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                    Divider = "rgba(255,255,255, 0.12)",
                    DividerLight = "rgba(255,255,255, 0.06)",
                    TableLines = "rgba(255,255,255, 0.12)",
                    LinesDefault = "rgba(255,255,255, 0.12)",
                    LinesInputs = "rgba(255,255,255, 0.42)",
                    TextDisabled = "rgba(255,255,255, 0.38)",
                    Info = "#64b5f6",
                    Success = "#81c784",
                    Warning = "#ffb74d",
                    Error = "#e57373",
                    Dark = "#f5f5f5"
                }
            };

            // Set the default font family for all typography - Srinakharinwirot first
            var fontFamily = new[] { "srinakharinwirot", "Sarabun", "Noto Sans Thai", "Angsana New", "AngsanaUPC", "Cordia New", "CordiaUPC", "TH SarabunPSK", "Arial Unicode MS", "sans-serif" };

            theme.Typography.Default.FontFamily = fontFamily;
            theme.Typography.H1.FontFamily = fontFamily;
            theme.Typography.H2.FontFamily = fontFamily;
            theme.Typography.H3.FontFamily = fontFamily;
            theme.Typography.H4.FontFamily = fontFamily;
            theme.Typography.H5.FontFamily = fontFamily;
            theme.Typography.H6.FontFamily = fontFamily;
            theme.Typography.Body1.FontFamily = fontFamily;
            theme.Typography.Body2.FontFamily = fontFamily;
            theme.Typography.Button.FontFamily = fontFamily;
            theme.Typography.Caption.FontFamily = fontFamily;
            theme.Typography.Subtitle1.FontFamily = fontFamily;
            theme.Typography.Subtitle2.FontFamily = fontFamily;
            theme.Typography.Overline.FontFamily = fontFamily;

            return theme;
        }

        // สำหรับ backward compatibility
        public static MudTheme CreateThaiTheme()
        {
            return CreateLightTheme();
        }
    }
}