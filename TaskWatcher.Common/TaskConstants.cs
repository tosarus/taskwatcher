namespace TaskWatcher.Common
{
    public static class TaskPriority
    {
        public const int Top = 0;
        public const int High = 1;
        public const int Normal = 2;
        public const int Low = 3;
        public const int Last = 4;
        public const int Default = Normal;
    }

    public static class TaskTag
    {
        public const string Done = "done";
    }
}
