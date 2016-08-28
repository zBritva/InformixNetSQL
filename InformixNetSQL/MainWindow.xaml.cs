using InformixNetSQL.DbProviderInterface;
using InformixNetSQL.DbProviders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InformixNetSQL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionManager manager = null;
        CollectionViewSource tablesCollection = null;
        bool isExecuting = false;

        public MainWindow()
        {
            InitializeComponent();

            manager = new ConnectionManager();
            var connections = manager.GetConnectionList();

            this.cb_dbserver.ItemsSource = connections;

            this.bt_connect.Foreground = DisconnectionStateStyle();
            this.bt_disconnect.Foreground = ConnectionStateStyle();
        }

        private Brush ConnectionStateStyle()
        {
            return Brushes.Green;
        }

        private Brush DisconnectionStateStyle()
        {
            return Brushes.Red;
        }

        private void bt_connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentConnectionString = this.cb_dbserver.SelectedItem as ConnectionStringSettings;

                if(manager.connection == null)
                {
                    this.manager.InitConnection(currentConnectionString);
                }

                if (manager.currentDatabse != currentConnectionString.Name)
                {
                    this.manager.connection.Disconnect();
                    this.manager.InitConnection(currentConnectionString);
                }

                this.manager.connection.Connect();

                var dblist = (this.manager.connection as IDbInfo).GetDataBases();

                this.cb_dbname.ItemsSource = dblist;
                this.bt_connect.Foreground = ConnectionStateStyle();
                this.bt_disconnect.Foreground = DisconnectionStateStyle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void bt_disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(this.manager.connection != null)
                    this.manager.connection.Disconnect();

                this.bt_connect.Foreground = DisconnectionStateStyle();
                this.bt_disconnect.Foreground = ConnectionStateStyle();
                this.cb_dbname.ItemsSource = new List<String>();
                this.lv_dbTables.ItemsSource = new List<String>();
                this.tablesCollection = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cb_dbname_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.manager.connection != null)
                {
                    var currentDataBase = this.cb_dbname.SelectedItem as String;
                    this.manager.connection.SetDatabase(currentDataBase);

                    var tables = (this.manager.connection as IDbInfo).GetTables();

                    tablesCollection = new CollectionViewSource();
                    tablesCollection.Source = tables;
                    tablesCollection.Filter += tableListFilter;
                    lv_dbTables.ItemsSource = tablesCollection.View;                    
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void tableListFilter(object sender, FilterEventArgs e)
        {
            try
            {
                var filterList = tb_filter.Text.Split(new string[]{ ";"}, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var filter in filterList)
                {
                    e.Accepted = ((string)e.Item).IndexOf(filter) >= 0;
                    if(e.Accepted)
                        return;
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void TableName_Click(object sender, EventArgs e)
        {

        }

        private void tb_filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                tablesCollection.View.Refresh();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void bt_execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isExecuting)
                {
                    MessageBox.Show("Please wait execution of previous operation");
                    return;
                }

                var sql = new TextRange(rtb_sqlSource.Document.ContentStart, rtb_sqlSource.Document.ContentEnd).Text;

                if (sql.Length == 0)
                {
                    MessageBox.Show("sql string is empty");
                    return;
                }

                if (!isExecuting)
                {
                    var result = await Task.Run<IExecutionResult<DataTable>>(()=> RunExecutionTask(sql));

                    ExecutionFinished(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isExecuting = false;
            }
        }

        private async Task<IExecutionResult<DataTable>> RunExecutionTask(string sql)
        {
            return this.ExecutionTask(sql);
        }

        private IExecutionResult<DataTable> ExecutionTask(string sql)
        {
            try
            {
                return this.manager.connection.Execution(sql);
            }
            catch(Exception ex)
            {
                return new SqlResult<DataTable>()
                {
                    resultCode = -1,
                    resultMessage = ex.Message,
                    resultData = new DataTable()
                };
            }
        }

        private void ExecutionFinished(IExecutionResult<DataTable> result)
        {
            try
            {
                if (result.resultCode >= 0)
                {
                    this.dg_sqlResult.ItemsSource = result.resultData.DefaultView;
                    this.dg_sqlResult.AutoGenerateColumns = true;
                    this.dg_sqlResult.CanUserAddRows = false;
                }
                else
                {
                    MessageBox.Show(result.resultMessage);
                }
                isExecuting = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isExecuting = false;
            }
        }

        private void bt_abort_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void bt_beginWork_Click(object sender, RoutedEventArgs e)
        {
            this.manager.connection.BeginWork();
        }

        private void bt_commitWork_Click(object sender, RoutedEventArgs e)
        {
            this.manager.connection.CommitWork();
        }

        private void bt_rollbackWork_Click(object sender, RoutedEventArgs e)
        {
            this.manager.connection.RollbackWork();
        }
    }
}
