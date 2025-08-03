using MudBlazor;

namespace AspireApp.Web.Services
{
    public static class ThemeService
    {
        public static MudTheme CreateThaiTheme()
        {
            var theme = new MudTheme();
            
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
            
            // Set color palette
            theme.PaletteLight.Primary = "#1976d2";
            theme.PaletteLight.Secondary = "#dc004e";
            theme.PaletteLight.AppbarBackground = "#1976d2";
            
            return theme;
        }
    }
}