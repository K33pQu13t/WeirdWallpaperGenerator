namespace WeirdWallpaperGenerator.Config
{
    public class ContextConfig
    {
        private static ContextConfig instance;
        private ContextConfig() {}

        public static ContextConfig GetInstance()
        {
            if (instance == null)
                instance = new ContextConfig();
            return instance;
        }

        public About About { get; set; }
        public ColorsSets ColorsSets { get; set; }
    }
}
