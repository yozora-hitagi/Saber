using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wox.Plugin.Program.Programs;

namespace Wox.Plugin.Program
{
    /// <summary>
    /// Interaction logic for ProgramSetting.xaml
    /// </summary>
    public partial class ProgramSetting : UserControl
    {
        private PluginInitContext context;
        private Settings _settings;
        private Indexing _indexing;

        public ProgramSetting(PluginInitContext context, Settings settings,Indexing indexing)
        {
            this.context = context;
            InitializeComponent();
            Loaded += Setting_Loaded;
            _settings = settings;

            _indexing = indexing;

            progressBarIndexing.SetBinding(TextBlock.VisibilityProperty, new System.Windows.Data.Binding("Visibility") { Source = indexing, Mode = System.Windows.Data.BindingMode.OneWay });
            messageIndexing.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Text") { Source = indexing, Mode = System.Windows.Data.BindingMode.OneWay });

       
        }

        private void Setting_Loaded(object sender, RoutedEventArgs e)
        {
            programSourceView.ItemsSource = _settings.ProgramSources;
            StartMenuEnabled.IsChecked = _settings.EnableStartMenuSource;
            RegistryEnabled.IsChecked = _settings.EnableRegistrySource;
        }

        //private void ReIndexing()
        //{
        //    //programSourceView.Items.Refresh();
        //    Task.Run(() =>
        //    {
        //        Dispatcher.Invoke(() => { indexingPanel.Visibility = Visibility.Visible; });
        //        Main.IndexPrograms();
        //        Dispatcher.Invoke(() => { indexingPanel.Visibility = Visibility.Hidden; });
        //    });
        //}

        private void btnAddProgramSource_OnClick(object sender, RoutedEventArgs e)
        {
            //var add = new AddProgramSource(_settings);
            //if(add.ShowDialog() ?? false)
            //{
            //    ReIndexing();
            //}


            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var source = new Settings.ProgramSource
                {
                    Location = dialog.SelectedPath,
                };
                _settings.ProgramSources.Add(source);
                programSourceView.Items.Refresh();
            }
        }

        private void btnDeleteProgramSource_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedProgramSource = programSourceView.SelectedItem as Settings.ProgramSource;
            if (selectedProgramSource != null)
            {
                string msg = string.Format(context.API.GetTranslation("wox_plugin_program_delete_program_source"), selectedProgramSource.Location);

                if (MessageBox.Show(msg, string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _settings.ProgramSources.Remove(selectedProgramSource);
                    //ReIndexing();
                    programSourceView.Items.Refresh();
                }
            }
            else
            {
                string msg = context.API.GetTranslation("wox_plugin_program_pls_select_program_source");
                MessageBox.Show(msg);
            }
        }

        private void btnEditProgramSource_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedProgramSource = programSourceView.SelectedItem as Settings.ProgramSource;
            if (selectedProgramSource != null)
            {
                //var add = new AddProgramSource(selectedProgramSource, _settings);
                //if (add.ShowDialog() ?? false)
                //{
                //    ReIndexing();
                //}

                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.SelectedPath = selectedProgramSource.Location;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    selectedProgramSource.Location = dialog.SelectedPath;
                    programSourceView.Items.Refresh();
                }
            }
            else
            {
                string msg = context.API.GetTranslation("wox_plugin_program_pls_select_program_source");
                MessageBox.Show(msg);
            }
        }

        private void btnReindex_Click(object sender, RoutedEventArgs e)
        {
            //Task.Run(() =>
            //{
            //    Dispatcher.Invoke(() => { indexingPanel.Visibility = Visibility.Visible; });
            //    Main.IndexPrograms();
            //    Dispatcher.Invoke(() => { indexingPanel.Visibility = Visibility.Hidden; });
            //});
            _indexing.start();
        }

        private void BtnProgramSuffixes_OnClick(object sender, RoutedEventArgs e)
        {
            ProgramSuffixes p = new ProgramSuffixes(context, _settings);
            p.ShowDialog();
        }

        private void programSourceView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void programSourceView_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                foreach (string s in files)
                {
                    if (Directory.Exists(s))
                    {
                        _settings.ProgramSources.Add(new Settings.ProgramSource
                        {
                            Location = s
                        });

                        programSourceView.Items.Refresh();
                    }
                }
            }
        }

        private void StartMenuEnabled_Click(object sender, RoutedEventArgs e)
        {
            _settings.EnableStartMenuSource = StartMenuEnabled.IsChecked ?? false;
            //ReIndexing();
        }

        private void RegistryEnabled_Click(object sender, RoutedEventArgs e)
        {
            _settings.EnableRegistrySource = RegistryEnabled.IsChecked ?? false;
            //ReIndexing();
        }
    }
}