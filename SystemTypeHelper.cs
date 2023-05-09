namespace HapGenSlicer;

public static class SystemTypeHelper
{
    private static readonly Dictionary<SystemType, string> _newLineCharacterBySystemType = new()
    {
        { SystemType.Linux, "\n" },
        { SystemType.Mac, "\n" },
        { SystemType.Windows, "\r\n" },
    };

    public static string GetNewLineCharacter(this SystemType systemType) =>
        _newLineCharacterBySystemType[systemType];
}