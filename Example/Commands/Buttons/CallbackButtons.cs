namespace Example.Commands.Buttons;

public static class CallbackButtons
{
    public enum Id
    {
        FirstTest,
        SecondTest
    }
    
    public static string ToButtonIdString(this Id id) => id.ToButtonIdInt().ToString();
    public static int ToButtonIdInt(this Id id) => (int)id;
    public static int ToInt(this string id) => int.Parse(id);
    public static Id ToButtonIdEnum(this int id) => (Id)id;
    public static Id ToButtonIdEnum(this string id) => (Id)id.ToInt();
}