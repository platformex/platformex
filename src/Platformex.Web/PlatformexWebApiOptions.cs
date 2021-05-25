namespace Platformex.Web
{
    public sealed class PlatformexWebApiOptions
    {
        public PlatformexWebApiOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; }
    }
}