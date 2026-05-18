using UnityEngine;

public enum GameThemeMode
{
    Dark,
    Light
}

public struct ThemeChangeMessage
{
    public const string PlayerPrefsKey = "SETTING_THEME_MODE";

    public GameThemeMode Theme;

    public ThemeChangeMessage(GameThemeMode theme)
    {
        Theme = theme;
    }

    public static GameThemeMode CurrentTheme
    {
        get { return (GameThemeMode)PlayerPrefs.GetInt(PlayerPrefsKey, (int)GameThemeMode.Dark); }
        set
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static bool TryRead(object data, out GameThemeMode theme)
    {
        if (data is ThemeChangeMessage)
        {
            ThemeChangeMessage message = (ThemeChangeMessage)data;
            theme = message.Theme;
            return true;
        }

        if (data is GameThemeMode)
        {
            theme = (GameThemeMode)data;
            return true;
        }

        if (data is int)
        {
            int themeIndex = (int)data;
            if (System.Enum.IsDefined(typeof(GameThemeMode), themeIndex))
            {
                theme = (GameThemeMode)themeIndex;
                return true;
            }
        }

        string themeName = data as string;
        if (!string.IsNullOrEmpty(themeName))
        {
            GameThemeMode parsedTheme;
            if (System.Enum.TryParse(themeName, true, out parsedTheme))
            {
                theme = parsedTheme;
                return true;
            }
        }

        theme = CurrentTheme;
        return data == null;
    }

    public static void Broadcast(GameThemeMode theme, bool save = true)
    {
        if (save) CurrentTheme = theme;

        MessageManager manager = Object.FindObjectOfType<MessageManager>();
        if (manager != null)
        {
            manager.SendMessage(ManhMessageType.OnThemeChanged, new ThemeChangeMessage(theme));
        }

        ApplyDirect(theme);
    }

    private static void ApplyDirect(GameThemeMode theme)
    {
        ThemeVisualApplier[] appliers = Object.FindObjectsOfType<ThemeVisualApplier>(true);
        for (int i = 0; i < appliers.Length; i++)
        {
            if (appliers[i] != null) appliers[i].ApplyTheme(theme);
        }
    }
}
