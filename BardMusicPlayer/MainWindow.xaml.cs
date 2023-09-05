﻿using System.Reflection;
using System.Windows;
using System.Windows.Media;
using BardMusicPlayer.Pigeonhole;
using BardMusicPlayer.UI_Classic;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace BardMusicPlayer;

/// <summary>
/// Interaction Logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Title              = "MeowMusic - " + Assembly.GetExecutingAssembly().GetName().Version;
        DataContext        = new ClassicMainView();
        AllowsTransparency = false;
        WindowStyle        = WindowStyle.SingleBorderWindow;
        Height             = 620;
        Width              = 900;
        ResizeMode         = ResizeMode.CanResizeWithGrip;

        if (!BmpPigeonhole.Instance.DarkStyle)
            LightModeStyle();
        else
            DarkModeStyle();
    }

    public static void LightModeStyle()
    {
        const BaseTheme baseTheme = BaseTheme.Light;

        const PrimaryColor primary = PrimaryColor.BlueGrey;
        var primaryColor = SwatchHelper.Lookup[(MaterialDesignColor)primary];

        const SecondaryColor secondary = SecondaryColor.Teal;
        var secondaryColor = SwatchHelper.Lookup[(MaterialDesignColor)secondary];

        var theme = Theme.Create(baseTheme, primaryColor, secondaryColor);
        var paletteHelper = new PaletteHelper();
        theme.Background = Color.FromRgb(250, 250, 250);
        paletteHelper.SetTheme(theme);
    }

    public static void DarkModeStyle()
    {
        const BaseTheme baseTheme = BaseTheme.Dark;

        const PrimaryColor primary = PrimaryColor.Grey;
        var primaryColor = SwatchHelper.Lookup[(MaterialDesignColor)primary];

        const SecondaryColor secondary = SecondaryColor.Teal;
        var secondaryColor = SwatchHelper.Lookup[(MaterialDesignColor)secondary];

        var theme = Theme.Create(baseTheme, primaryColor, secondaryColor);
        var paletteHelper = new PaletteHelper();
        theme.Background = Color.FromRgb(30, 30, 30);
        paletteHelper.SetTheme(theme);
    }
}