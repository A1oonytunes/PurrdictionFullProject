namespace com.onlineobject.objectnet
{
    public enum PositionReference : byte
    {
        UseGlobal = 1, // Will use global position ( normally used when object haven't any parent )
        UseLocal = 2 // Will use a local position ( normally used when object is child of another object )
    }
}