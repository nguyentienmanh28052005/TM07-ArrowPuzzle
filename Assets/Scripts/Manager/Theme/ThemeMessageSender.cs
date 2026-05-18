using UnityEngine;
using UnityEngine.EventSystems;

public class ThemeMessageSender : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameThemeMode theme = GameThemeMode.Light;
    [SerializeField] private bool toggleFromCurrentTheme;
    [SerializeField] private bool saveSelection = true;
    [SerializeField] private bool sendOnPointerClick = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sendOnPointerClick) SendConfiguredTheme();
    }

    public void SendConfiguredTheme()
    {
        SendTheme(toggleFromCurrentTheme ? GetOppositeTheme() : theme);
    }

    public void SendDarkTheme()
    {
        SendTheme(GameThemeMode.Dark);
    }

    public void SendLightTheme()
    {
        SendTheme(GameThemeMode.Light);
    }

    public void ToggleTheme()
    {
        SendTheme(GetOppositeTheme());
    }

    public void SendTheme(GameThemeMode targetTheme)
    {
        ThemeChangeMessage.Broadcast(targetTheme, saveSelection);
    }

    private GameThemeMode GetOppositeTheme()
    {
        return ThemeChangeMessage.CurrentTheme == GameThemeMode.Dark ? GameThemeMode.Light : GameThemeMode.Dark;
    }
}
