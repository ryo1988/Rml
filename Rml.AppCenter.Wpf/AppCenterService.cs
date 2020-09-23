namespace Rml.AppCenter.Wpf
{
    public class AppCenterService : IAppCenterService
    {
        public void Start(string appSecret)
        {
            Microsoft.AppCenter.AppCenter.Start(appSecret);
        }
    }
}