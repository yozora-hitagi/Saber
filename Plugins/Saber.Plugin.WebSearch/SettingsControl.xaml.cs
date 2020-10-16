using System.Windows;
using System.Windows.Controls;
using Saber.Core.Plugin;

namespace Saber.Plugin.WebSearch
{
    /// <summary>
    /// Interaction logic for WebSearchesSetting.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private readonly Settings _settings;
        private readonly PluginInitContext _context;

        public SettingsControl(PluginInitContext context, SettingsViewModel viewModel)
        {
            InitializeComponent();
            _context = context;
            _settings = viewModel.Settings;
            DataContext = viewModel;
        }

        private void OnAddSearchSearchClick(object sender, RoutedEventArgs e)
        {
            var setting = new SearchSourceSettingWindow(_settings.SearchSources, _context);
            setting.ShowDialog();
        }

        private void OnDeleteSearchSearchClick(object sender, RoutedEventArgs e)
        {
            var selected = _settings.SelectedSearchSource;
            var warning = "你确定要删除 {0} 吗？";
            var formated = string.Format(warning, selected.Title);

            var result = MessageBox.Show(formated, string.Empty, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var id = _context.CurrentPluginMetadata.ID;
                PluginManager.RemoveActionKeyword(id, selected.ActionKeyword);
                _settings.SearchSources.Remove(selected);
            }
        }

        private void OnEditSearchSourceClick(object sender, RoutedEventArgs e)
        {
            var selected = _settings.SelectedSearchSource;
            var webSearch = new SearchSourceSettingWindow
                (
                _settings.SearchSources, _context, selected
                );
            webSearch.ShowDialog();
        }
    }
}