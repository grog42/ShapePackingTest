namespace WebApps
{
    /// <summary>
    /// Collection of ids to access DOM elements and keys to access local webstorage
    /// </summary>
    public static class IdList
    {
        //Local stroage keys
        public const string WriterStorageKey = "WriterStorage";
        public const string ReaderStorageKey = "ReaderStorage";
        public const string ExampleStorageKey = "ExampleStorage";

        //html ids
        public const string MainCanvasId = "main-canvas";
        public const string WidthInputId = "width-input";
        public const string HeightInputId = "height-input";
        public const string FileInputId = "file-input";

        public static string[] ToArray()
        {
            string[] ids = new string[7];

            ids[0] = WriterStorageKey;
            ids[1] = ReaderStorageKey;
            ids[2] = ExampleStorageKey;

            ids[3] = MainCanvasId;
            ids[4] = WidthInputId;
            ids[5] = HeightInputId;
            ids[6] = FileInputId;

            return ids;
        }
    }
}
