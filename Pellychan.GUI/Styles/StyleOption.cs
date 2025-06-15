namespace Pellychan.GUI.Styles;

public interface IStyleOption
{
    public enum OptionType
    {
        Button,
        TitleBar
    }
    
    public Style.StateFlag State { get; set; }
}

public struct StyleOptionButton : IStyleOption
{
    public string Text { get; set; } = string.Empty;
    public Style.StateFlag State { get; set; }
    
    public StyleOptionButton()
    {
    }
}