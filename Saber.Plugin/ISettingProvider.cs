using System.Windows.Controls;

namespace Saber.Plugin
{
    public interface ISettingProvider
    {
        Control CreateSettingPanel();
    }
}
