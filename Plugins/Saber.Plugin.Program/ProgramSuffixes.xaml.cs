using System.Windows;

namespace Saber.Plugin.Program
{
    /// <summary>
    /// ProgramSuffixes.xaml 的交互逻辑
    /// </summary>
    public partial class ProgramSuffixes
    {
        private PluginInitContext context;
        private Settings _settings;

        public ProgramSuffixes(PluginInitContext context, Settings settings)
        {
            this.context = context;
            InitializeComponent();
            _settings = settings;
            tbSuffixes.Text = string.Join(Settings.SuffixSeperator.ToString(), _settings.ProgramSuffixes);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbSuffixes.Text))
            {
                MessageBox.Show("文件后缀不能为空");
                return;
            }

            _settings.ProgramSuffixes = tbSuffixes.Text.Split(Settings.SuffixSeperator);
            MessageBox.Show("成功更新索引文件后缀");
        }
    }
}
