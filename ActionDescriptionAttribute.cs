[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ActionDescriptionAttribute : Attribute
{
    public string Description { get; }

    public ActionDescriptionAttribute(string description)
    {
        Description = description;
    }
}