namespace Example.Commands.CallbackButtons;

internal enum ButtonId
{
    FirstTest,
    SecondTest
}

internal static class ButtonIdExtensions
{
    public static string ToButtonIdString(this ButtonId id) => id.ToButtonIdInt().ToString();
    public static int ToButtonIdInt(this ButtonId id) => (int)id;
    public static int ToInt(this string id) => int.Parse(id);
    public static ButtonId ToButtonIdEnum(this int id) => (ButtonId)id;
    public static ButtonId ToButtonIdEnum(this string id) => (ButtonId)id.ToInt();
}