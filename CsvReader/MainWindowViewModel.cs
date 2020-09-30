using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace CsvReader
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Data> Datas { get; set; }
        private string _connectionString;
        private Data _header;
        private string _filePath;

        private string _dataSupplier;
        public string DataSupplier
        {
            get { return _dataSupplier; }
            set
            {
                _dataSupplier = value;
                OnPropertyChanged("DataSupplier");
            }
        }

        private Data _selectedData;
        public Data SelectedData
        {
            get { return _selectedData; }
            set
            {
                _selectedData = value;
                OnPropertyChanged("SelectedData");
            }
        }

        private RelayCommand _selectFile;
        public RelayCommand SelectFile
        {
            get
            {
                return _selectFile ??
                  (_selectFile = new RelayCommand(obj =>
                  {
                      Datas.Clear();
                      DataSupplier = "Data from CSV file";
                      OpenFileDialog openFileDialog = new OpenFileDialog();

                      openFileDialog.InitialDirectory = "c:\\";
                      openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

                      if (openFileDialog.ShowDialog() == true)
                      {
                          using (StreamReader reader = new StreamReader(openFileDialog.OpenFile()))
                          {
                              _filePath = openFileDialog.FileName;
                              var values = reader.ReadLine().Split(',');
                              _header = new Data { TagName = values[0], Type = values[1], Value = values[2] };
                              while (!reader.EndOfStream)
                              {
                                  values = reader.ReadLine().Split(',');
                                  Datas.Add(new Data { TagName = values[0], Type = values[1], Value = values[2] });
                              }
                          }
                      }
                  }));
            }
        }

        private RelayCommand _writeToDatabase;
        public RelayCommand WriteToDatabase
        {
            get
            {
                return _writeToDatabase ??
                  (_writeToDatabase = new RelayCommand(obj =>
                  {
                      using (SqlConnection connection = new SqlConnection(_connectionString))
                      {
                          var queryString = string.Format("INSERT INTO [Table]({0},{1},{2}) VALUES(@param1,@param2,@param3)", _header.TagName, _header.Type, _header.Value);
                          foreach (var d in Datas)
                          {
                              SqlCommand command = new SqlCommand(queryString, connection);
                              command.Connection.Open();
                              command.Parameters.AddWithValue("@param1", d.TagName);
                              command.Parameters.AddWithValue("@param2", d.Type);
                              command.Parameters.AddWithValue("@param3", d.Value);
                              command.ExecuteNonQuery();
                              command.Connection.Close();
                          }
                      }
                  }));
            }
        }

        private RelayCommand _getDataFromDatabase;
        public RelayCommand GetDataFromDatabase
        {
            get
            {
                return _getDataFromDatabase ??
                  (_getDataFromDatabase = new RelayCommand(obj =>
                  {
                      Datas.Clear();
                      DataSupplier = "Data from database";
                      using (SqlConnection connection = new SqlConnection(_connectionString))
                      {
                          var queryString = "SELECT * FROM [Table]";
                          SqlCommand command = new SqlCommand(queryString, connection);
                          command.Connection.Open();
                          SqlDataReader reader = command.ExecuteReader();
                          if (reader.HasRows)
                          {
                              while (reader.Read())
                              {
                                  Datas.Add(new Data
                                  {
                                      TagName = reader.GetString(1),
                                      Type = reader.GetString(2),
                                      Value = reader.GetString(3)
                                  });
                              }
                          }
                      }
                  }));
            }
        }

        private RelayCommand _writeToCSV;
        public RelayCommand WriteToCSV
        {
            get
            {
                return _writeToCSV ??
                  (_writeToCSV = new RelayCommand(obj =>
                  {
                      var csv = new StringBuilder();
                      var newLine = string.Format("{0},{1},{2}", _header.TagName,_header.Type,_header.Value);
                      Datas = new ObservableCollection<Data>(Datas.OrderBy(i=>i.TagName));
                      csv.AppendLine(newLine);
                      foreach (var d in Datas)
                      {
                          newLine = string.Format("{0},{1},{2}", d.TagName, d.Type, d.Value);
                          csv.AppendLine(newLine);
                      }
                      File.WriteAllText(_filePath, csv.ToString());
                  }));
            }
        }

        private RelayCommand _exit;
        public RelayCommand Exit
        {
            get
            {
                return _exit ??
                  (_exit = new RelayCommand(obj =>
                  {
                      Application.Current.MainWindow.Close();
                  }));
            }
        }

        public MainWindowViewModel()
        {
            Datas = new ObservableCollection<Data>();
            _connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='|DataDirectory|Database.mdf';Integrated Security=True";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
