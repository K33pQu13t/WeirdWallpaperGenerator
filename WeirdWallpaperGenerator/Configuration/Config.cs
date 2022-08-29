namespace WeirdWallpaperGenerator.Configuration
{
    public class Config
    {
        public About About { get; set; } = new About();
        public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();
        public UpdaterSettings UpdaterSettings { get; set; } = new UpdaterSettings();
        public ColorsSets ColorsSets { get; set; } = new ColorsSets();
    }
}
