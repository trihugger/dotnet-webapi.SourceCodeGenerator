using SourceCodeGenerator.CodeGenerator;
using SourceCodeGenerator.CodeTemplates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
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
using System.Windows.Threading;

namespace SourceCodeGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Windows Methods and Properties
        private ModelInfo _selectedModel = new ModelInfo();

        public MainWindow()
        {
            InitializeComponent();

            // Main Screen
            DataModelList.Width = 988;
            namecol.Width = 600;
            categorycol.Width = 275;
            GenerateButton.IsEnabled = false;
            ModelLabel.Visibility = Visibility.Hidden;
            SelectedModelName.Visibility = Visibility.Hidden;

            // Tabs
            WorkTabs.Visibility = Visibility.Hidden;
            WorkTabs.SelectedIndex = 0;

            // Tab 1
            Tab1NextButton.Visibility = Visibility.Hidden;

            // Tab 2
            Tab2.Visibility = Visibility.Hidden;
            IList<ModelInfo> models = EngineFunctions.GetModels();
            DataModelList.ItemsSource = models;
        }

        private void DataModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView selectedItem = (ListView)sender;
            ModelInfo selectedModel = (ModelInfo)selectedItem.SelectedItem;
            this._selectedModel = selectedModel;
            GenerateButton.IsEnabled = true;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExitButton.Content.ToString() !.Equals("Exit"))
            {
                this.Close();
            }
            else
            {
                GenerateButton.IsEnabled = true;
                ExitButton.Content = "Exit";
                ModelLabel.Visibility = Visibility.Hidden;
                SelectedModelName.Visibility = Visibility.Hidden;
                WorkTabs.Visibility = Visibility.Hidden;
                DataModelList.Width = 988;
                namecol.Width = 600;
                categorycol.Width = 275;
            }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateButton.IsEnabled = false;
            ExitButton.Content = "Cancel";

            Tab1Messages.Text = "Loading Data Model...";

            DataModelList.Width = 275;
            namecol.Width = 200;
            categorycol.Width = 75;

            ModelLabel.Visibility = Visibility.Visible;
            SelectedModelName.Text = _selectedModel.Name;
            SelectedModelName.Visibility = Visibility.Visible;

            // Tabs
            WorkTabs.Visibility = Visibility.Visible;
            WorkTabs.SelectedIndex = 0;

            // Step 1 load the model into the engine
            Tab1Messages.Text += "Done\r\nLoading Properties...";
            CodeGeneratorEngine engine = new CodeGeneratorEngine(_selectedModel);

            await Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    // Step 2 add properties to engine
                    Tab1DataModelList.ItemsSource = engine.PropertyOptions;
                    Tab1DataModelList.Items.Refresh();
                    Tab1Messages.Text += "Done\r\n";
                }), DispatcherPriority.Background);

            MessageBoxResult result = MessageBox.Show("Data Model has been loaded, please review and press ok to continue, or cancel to abort.", "Code Generator", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (result == MessageBoxResult.OK)
            {
                await Dispatcher.BeginInvoke(
                   new Action(() =>
                   {
                       CreateCode(engine);
                   }), DispatcherPriority.Background);

                Tab1NextButton.Visibility = Visibility.Visible;
            }
            else
            {
                ExitButton_Click(sender, e);
            }
        }

        private void Tab1NextButton_Click(object sender, RoutedEventArgs e)
        {
            MigrateButton_Click(sender, e);
            ExitButton_Click(sender, e);
        }

        private void MigrateButton_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            processStartInfo.RedirectStandardOutput = true; // Window closes faster than it should so this helps capture the output
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.FileName = "C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe";

            MessageBoxResult result = MessageBox.Show("Would you like to start the migration process?", "Source Code Generator", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // TODO: ask for name of migration
                string migrationName = "NewMigration" + DateTime.Now.ToString("MM-dd-yyyy_hh_mm_ss");
                processStartInfo.Arguments = $"Add-Migration {migrationName} -Context ApplicationDbContext -Output-dir src\\Infrastructure"; // Powershell
                processStartInfo.Arguments = $"dotnet ef migrations add {migrationName} --context ApplicationDbContext --output-dir src\\Infrastructure"; // CLI aka CMD

                // dotnet ef migrations add SCG_MigrationTest --context ApplicationDbContext --project DN.WebApi.Infrastructure --provider SqlServer

                // can add the --project ../Migrators.[MSSQL|MySQL|PostgreSql] -- --provider [SqlServer/MySql/PostgreSQL] to specify projects for migration in CLI
                // -Args "--provider SqlServer" - example for  powershell
                // https://docs.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli - provider's list.

                process.StartInfo = processStartInfo;
                process.Start();
                string s = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                using (StreamWriter outFile = new StreamWriter($"MigrationOutput{DateTime.Now.ToString("MM - dd - yyyy_hh_mm_ss")}.log", true))
                {
                    outFile.Write(s);
                }

                MessageBox.Show("Migration added, please check for errors", "Source Code Generator");
            }

            if (result == MessageBoxResult.Cancel)
            {
                MessageBox.Show("Migration of database was cancelled.", "Source Code Generator");
            }
            else
            {
                MessageBoxResult updateresult = MessageBox.Show("Would you like to start the database update process?", "Source Code Generator", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    processStartInfo.Arguments = "dotnet ef database update"; // cmd line version
                    processStartInfo.Arguments = "Update-Database -Context ApplicationDbContext"; // powershell version
                    process.StartInfo = processStartInfo;
                    process.Start();
                    string s = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    using (StreamWriter outFile = new StreamWriter($"DatabaseUpdate{DateTime.Now.ToString("MM-dd-yyyy_hh_mm_ss")}.log", true))
                    {
                        outFile.Write(s);
                    }

                    MessageBox.Show("Database update complete, please check for errors", "Source Code Generator");
                }
            }
        }
        #endregion

        #region Source Code Generation Methods
        private void CreateCode(CodeGeneratorEngine engine)
        {
            // Generate Code for each of the source code template files
            MessageBoxResult generateFiles = MessageBox.Show("Would you like to generate all source code files?\r\nIf you choose No we will ask for each file before generating.", "Code Generator", MessageBoxButton.YesNo, MessageBoxImage.Question);
            bool generateAllFiles = generateFiles == MessageBoxResult.Yes;

            MessageBoxResult backupFiles = MessageBox.Show("Would you like to backup all files before changing them?\r\nChoose Cancel if you would like to choose for each file.", "Code Generator", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            bool backupAllFiles = backupFiles == MessageBoxResult.Yes;
            bool backupSelect = backupFiles == MessageBoxResult.Cancel;
            bool backupFile;

            // Generate the data model and add the constructors and the update method
            if(GenerateFile(generateAllFiles, "Data Model"))
            {
                Tab1Messages.Text += "Generating Data Model...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Data Model");
                AddDataModelConstructAndUpdate(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate the permissions for Domain\Constances\Permissions file - append
            if (GenerateFile(generateAllFiles, "Permissions"))
            {
                Tab1Messages.Text += "Generating permissions...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Permissions");
                AddPermissions(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate DTO for Shared\Shared.DTOs\{category}\{DataModel}Dto.cs
            if (GenerateFile(generateAllFiles, "Data Transfer Object"))
            {
                Tab1Messages.Text += "Generating Data Transfer Object...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Data Transfer Object");
                DataTransferObject dto = new DataTransferObject(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Details Dto for Shared\Shared.DTOs\{category}\{DataModel}DetailsDto.cs
            if (engine.HasAppModel)
            {
                if (GenerateFile(generateAllFiles, "Details Data Transfer Object"))
                {
                    Tab1Messages.Text += "Generating Details Data Transfer Object...";
                    backupFile = BackupFile(backupAllFiles, backupSelect, "Details Data Transfer Object");
                    DetailsDataTransferObject details = new DetailsDataTransferObject(engine, backupFile);
                    Tab1Messages.Text += "Done\r\n";
                }
            }

            // Generate ListFilter for Shared\Shared.DTOs\{category}\{DataModel}ListFilter.cs
            if (GenerateFile(generateAllFiles, "List Filter"))
            {
                Tab1Messages.Text += "Generating List Filter...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "List Filter");
                ListFilter listFilter = new ListFilter(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Create Request for Shared\Shared.DTOs\{category}\Create{DataModel}Request.cs
            if (GenerateFile(generateAllFiles, "Create Request"))
            {
                Tab1Messages.Text += "Generating Create Request...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Create Request");
                CreateRequest createRequest = new CreateRequest(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Update Request for Shared\Shared.DTOs\{category}\Update{DataModel}Request.cs
            if (GenerateFile(generateAllFiles, "Update Request"))
            {
                Tab1Messages.Text += "Generating Update Request...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Update Request");
                UpdateRequest updateRequest = new UpdateRequest(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Validators for requests for Application\Validators\{category}\Create{DataModel}RequestValidator.cs
            if (GenerateFile(generateAllFiles, "Create Request Validator"))
            {
                Tab1Messages.Text += "Generating Create Request Validator...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Create Request Validator");
                CreateRequestValidator createRequestValidator = new CreateRequestValidator(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Validators for requests for Application\Validators\{category}\Update{DataModel}RequestValidator.cs
            if (GenerateFile(generateAllFiles, "Update Request Validator"))
            {
                Tab1Messages.Text += "Generating Update Request Validator...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Update Request Validator");
                UpdateRequestValidator updateRequestValidator = new UpdateRequestValidator(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate IService for data model for Application\Abstractions\Services\{category}\I{DataModel}Service.cs
            if (GenerateFile(generateAllFiles, "Service Interface"))
            {
                Tab1Messages.Text += "Generating IService...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Service Interface");
                ServiceInterface serviceInterface = new ServiceInterface(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Service for data model for Application\Services\{category}\{DataModel}Service.cs
            if (GenerateFile(generateAllFiles, "Service"))
            {
                Tab1Messages.Text += "Generating Service...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Service");
                Service service = new Service(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Controller for Bootstrapper\Controllers\v1\{Datamodel}Controller.cs
            if (GenerateFile(generateAllFiles, "Controller"))
            {
                Tab1Messages.Text += "Generating Controller...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Controller");
                Controller controller = new Controller(engine, BackupOriginal: backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Generate Localization for Bootstrapper\Localization\en-[IN|US].json - append
            if (GenerateFile(generateAllFiles, "Localization"))
            {
                Tab1Messages.Text += "Generating Localization...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Localization");
                AddLocalization(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }

            // Add Table to context for Infrastructure\Persistence\ApplicationDbContext.cs - append
            if (GenerateFile(generateAllFiles, "Application Database Context"))
            {
                Tab1Messages.Text += "Adding table to Application Database Context...";
                backupFile = BackupFile(backupAllFiles, backupSelect, "Application Database Context");
                AddApplicationDbContext(engine, backupFile);
                Tab1Messages.Text += "Done\r\n";
            }
        }

        private bool GenerateFile(bool generateFile, string filename)
        {
            if (!generateFile)
            {
                MessageBoxResult generateFiles = MessageBox.Show($"Would you like to generate the source code for {filename} file?", "Code Generator", MessageBoxButton.YesNo, MessageBoxImage.Question);
                generateFile = generateFiles == MessageBoxResult.Yes;
            }

            return generateFile;
        }

        private bool BackupFile(bool backupFile, bool backupSelect, string filename)
        {
            if (!backupFile && backupSelect)
            {
                MessageBoxResult result = MessageBox.Show($"Would you like to backup {filename} File?", "Code Generator", MessageBoxButton.YesNo, MessageBoxImage.Question);
                backupFile = result == MessageBoxResult.Yes;
            }

            return backupFile;
        }

        private void AddDataModelConstructAndUpdate(CodeGeneratorEngine engine, bool Backupfile = true)
        {
            string category = string.IsNullOrEmpty(engine.ModelInfo.Category) ? string.Empty : $"{engine.ModelInfo.Category}\\";
            string codefolder = @$"Core\Domain\Entities\{category}";
            string fileName = @$"{engine.ModelInfo.Name}.cs";
            string rootPath = EngineFunctions.GetApplicationPath();
            string codePath = @$"{rootPath}{codefolder}";
            string filePath = codePath + fileName;
            string paramLine = engine.GetInLineProperties(ExcludeCollections: true, ExcludeAppModels: true, IsLinePropertyNameLowerCase: true);
            string properties = GenerateProperties(engine);
            string updateProperties = GenerateUpdateProperties(engine);

            string modelCode = File.ReadAllText(filePath);
            string[] modelCodeLines = modelCode.Split(Environment.NewLine.ToCharArray());

            StringBuilder iConstructor = new StringBuilder();
            iConstructor.Append($@"
        public {engine.Model.Name}({paramLine})
        {{
{properties}
        }}
");

            StringBuilder iUpdate = new StringBuilder();
            iUpdate.Append($@"
        public {engine.Model.Name} Update({paramLine})
        {{
{updateProperties}
            return this;
        }}
");

            StringBuilder iModelCode = new StringBuilder();
            bool foundConstruct = false;
            string linetoFind = $"        public {engine.Model.Name}(";
            foreach(string line in modelCodeLines)
            {
                if(line.Contains(linetoFind)) foundConstruct = true;
                if (!foundConstruct && !string.IsNullOrEmpty(line)) iModelCode.Append(line + Environment.NewLine);
            }

            if (!foundConstruct)
            {
                iModelCode = new StringBuilder();
                iModelCode.Append(iConstructor);
                iModelCode.Append(iUpdate);
                EngineFunctions.AppendCodeToFile(iModelCode.ToString(), 2, filePath, Backupfile);
            }
            else
            {
                iModelCode.Append(iConstructor);
                iModelCode.Append(iUpdate);
                iModelCode.Append($"    }}\r\n}}");

                if (Backupfile) EngineFunctions.BackupFile(filePath);
                File.Delete(filePath);
                File.WriteAllText(filePath, iModelCode.ToString(), Encoding.Unicode);
            }
        }

        private string GenerateProperties(CodeGeneratorEngine engine)
        {
            StringBuilder iProperties = new StringBuilder();
            foreach(PropertyOption property in engine.PropertyOptions)
            {
                string typeName = property.PropertTypeName.ToLower();

                Console.WriteLine(typeName);

                // Exclude App Models, Collections, and Properties in the exclusion list
                if (!engine.ExcludeProps.Contains(property.Name) &&
                    !property.IsAppModel &&
                    !property.IsCollection &&
                    property.CanWrite)
                {
                    iProperties.Append($"            {property.Name} = {property.Name.ToLower()};\r\n");
                }
            }

            return iProperties.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        private string GenerateUpdateProperties(CodeGeneratorEngine engine)
        {
            StringBuilder iProperties = new StringBuilder();
            foreach (PropertyOption property in engine.PropertyOptions)
            {
                string typeName = property.PropertTypeName.ToLower();
                string nameA = property.Name;
                string namea = property.Name.ToLower();

                // Exclude App Models, Collections, and Properties in the exclusion list
                if (!engine.ExcludeProps.Contains(property.Name) &&
                    !property.IsAppModel &&
                    !property.IsCollection &&
                    property.CanWrite)
                {
                    Type propertyType = property.PropertyType;
                    switch (typeName)
                    {
                        case "int":
                        case "int16":
                        case "int32":
                        case "int65":
                        case "double":
                        case "float":
                        case "decimal":
                            iProperties.Append($"            if ({nameA} != {namea}) ");
                            break;
                        default:
                            iProperties.Append($"            if (!{nameA}.Equals({namea})) ");
                            break;
                    }

                    iProperties.Append($"{nameA} = {namea};\r\n");
                }
            }

            return iProperties.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        private void AddPermissions(CodeGeneratorEngine engine, bool BackupFile = true)
        {
            string codefolder = @$"Core\Domain\Constants\";
            string fileName = @$"Permissions.cs";
            string rootPath = EngineFunctions.GetApplicationPath();
            string codePath = @$"{rootPath}{codefolder}";
            string filePath = codePath + fileName;
            StringBuilder iPermissions = new StringBuilder();
            iPermissions.Append($@"
        [DisplayName(""{engine.Model.Name}"")]
        [Description(""{engine.Model.Name}s Permissions"")]
        public static class {engine.Model.Name}s
        {{
            public const string View = ""Permissions.{engine.Model.Name}s.View"";
            public const string ListAll = ""Permissions.{engine.Model.Name}s.ViewAll"";
            public const string Register = ""Permissions.{engine.Model.Name}s.Register"";
            public const string Update = ""Permissions.{engine.Model.Name}s.Update"";
            public const string Remove = ""Permissions.{engine.Model.Name}s.Remove"";
        }}");
            EngineFunctions.AppendCodeToFile(iPermissions.ToString(), 2, filePath, BackupFile);
        }

        private void AddLocalization(CodeGeneratorEngine engine, bool BackupFile = true)
        {
            string codefolder = @$"Bootstrapper\Localization\";
            string fileName = @$"en-US.json";
            string rootPath = EngineFunctions.GetApplicationPath();
            string codePath = @$"{rootPath}{codefolder}";
            string filePath = codePath + fileName;
            LocalizerFile localizer = new LocalizerFile(filePath);
            localizer.AddEntry($@"{engine.Model.Name.ToLower()}.alreadyexists", $@"{engine.Model.Name} {{0}} already Exists.");
            localizer.AddEntry($@"{engine.Model.Name.ToLower()}.notfound", $@"{engine.Model.Name} {{0}} not Found.");
            localizer.SaveLocalizer(BackupFile);
        }

        private void AddApplicationDbContext(CodeGeneratorEngine engine, bool BackupFile = true)
        {
            string codefolder = @$"Infrastructure\Persistence\";
            string fileName = @$"ApplicationDbContext.cs";
            string rootPath = EngineFunctions.GetApplicationPath();
            string codePath = @$"{rootPath}{codefolder}";
            string filePath = codePath + fileName;
            StringBuilder iApplicationDb = new StringBuilder();
            iApplicationDb.Append($@"        public DbSet<{engine.Model.Name}> {engine.Model.Name}s {{ get; set; }}");
            string category = string.IsNullOrEmpty(engine.ModelInfo.Category) ? string.Empty : $@".{engine.ModelInfo.Category}";
            EngineFunctions.AppendCodeToFile(@$"using {engine.AppName}.Domain.Entities{category};", @$"using Microsoft.EntityFrameworkCore;", filePath, BackupFile);
            EngineFunctions.AppendCodeToFile(iApplicationDb.ToString(), "        protected override void OnModelCreating(ModelBuilder modelBuilder)", filePath, backupfile: false);
        }
        #endregion
    }
}
