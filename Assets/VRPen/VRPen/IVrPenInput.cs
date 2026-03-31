namespace VRPenNamespace
{
    public partial interface IVrPenInput
    {
        bool MenuToggle { get; }
        bool Up         { get; }
        bool Down       { get; }
        bool Enter      { get; }
    }
}