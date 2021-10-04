using DN.WebApi.Domain.Entities.Multitenancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SourceCodeGenerator.CodeGenerator
{
    public static class EngineFunctions
    {
        #region PROPERTIES AND CODE GENERATOR SETTINGS
        // Use a class that will always exist on your project and at the highest namespace hierarchy that all your other classes would live.
        // NOTE: Best is to select a project/layer and create a folder for all your data models.
        private static readonly Type ProjectClass = typeof(Tenant); // Define This based on your project
        private static readonly Assembly Assembly = ProjectClass.GetTypeInfo().Assembly;

        // This is the base name space for all your other namespaces. This is used in creating the source code for your project
        private static readonly string ModelNamespace = ProjectClass.GetTypeInfo().Namespace!.Replace(".Multitenancy", string.Empty); // Define This based on your project

        // Use project Assembly to determine the name of the application.
        private static readonly string RootApplication = Assembly.ManifestModule.Name!.Replace(".Domain.dll", string.Empty); // Define This based on your project

        // Determine the base directory of this project and create a relative path to the folder there all your projects will live under.
        private static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string RelativePath = Path.Combine(AppPath, @"..\..\..\..\src\"); // Define This based on your project
        private static readonly string[] ExcludeModels = { "Tenant", "Brand", "Product" };

        public static string GetModelNameSpace()
        {
            return ModelNamespace;
        }

        public static string GetRootApplication()
        {
            return RootApplication;
        }

        public static string GetApplicationPath()
        {
            return Path.GetFullPath(RelativePath);
        }
        #endregion

        #region FUNCTIONS TO GATHER INFORMATION FROM PROJECT AND CODE
        public static IList<ModelInfo> GetModels()
        {
            IList<ModelInfo> models = new List<ModelInfo>();
            IList<Type> types = GetApplications();
            foreach (Type type in types)
            {
                if (!ExcludeModels.Contains(type.Name))
                {
                    ModelInfo newModel = GetModel(type, ModelNamespace);
                    if (newModel != null)
                    {
                        models.Add(newModel);
                    }
                }
            }

            return models;
        }

        private static IList<Type> GetApplications()
        {
            IList<Type> classes = Assembly.GetTypes().Where(t => t.IsPublic).ToArray<Type>().Where(t => t.Namespace is not null && t.Namespace.Contains(ModelNamespace)).ToList();
            return classes;
        }

        public static ModelInfo GetModel(Type type, string baseNamespace)
        {
            if(string.IsNullOrWhiteSpace(baseNamespace)) baseNamespace = ModelNamespace;
            ModelInfo modelInfo = new ModelInfo();
            modelInfo.Name = type.Name;
            modelInfo.Namespace = type.Namespace!;
            modelInfo.FullPath = type.FullName!;
            modelInfo.Category = modelInfo.Namespace == null ? string.Empty : modelInfo.Namespace.Replace(baseNamespace + ".", string.Empty).Replace(baseNamespace, string.Empty);
            return modelInfo;
        }

        public static Type GetModelType(string modelNamespace, string modelName)
        {
            IList<Type> classes = Assembly.GetTypes().Where(t => t.Namespace == modelNamespace && t.IsPublic && t.Name == modelName).ToList();
            if (classes.Count > 1)
            {
                MessageBoxResult result = MessageBox.Show(modelName + " has more than one classe in Documents folder, please investigate. Using " + classes[0].Name + " by default.", "Class Generation.", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return classes[0];
        }

        public static bool IsAppModelId(string id)
        {
            string modelName = id.Replace("Id", string.Empty).Replace("id", string.Empty);
            IList<Type> models = GetApplications();
            return string.IsNullOrEmpty(modelName) ? false : models.Select(x => x.Name.Contains(modelName)).Count() > 0;
        }

        public static bool IsAppModel(string name)
        {
            IList<Type> models = GetApplications();
            return models.Select(x => x.Name.Contains(name)).Count() > 0;

        }
        #endregion

        #region FILE APPEND OPERATIONS AID FUNCTIONS
        public static void AppendCodeToFile(string codeToAdd, string codeToFind, string filePath, bool backupfile = true)
        {
            // General Function to append to file given code to add, code to find (uses existing code as a marker), complete file path, and if we should backup the original file
            var fileCode = File.ReadAllLines(filePath).ToList();
            int index = 0;
            string[] addLines = codeToAdd.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (backupfile) BackupFile(filePath);
            if (string.IsNullOrEmpty(addLines[0]))
            {
                index = fileCode.IndexOf(addLines[1]);
            } // If first line is blank use second line will throw error addLines is blank or second line is blank as it should
            else
            {
                index = fileCode.IndexOf(addLines[0]);
            }

            if (index < 0)
            {
                string[] codeLines = codeToFind.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                index = fileCode.IndexOf(codeLines[0]);
                if (index > 0)
                {
                    fileCode.Insert(index, codeToAdd);
                    File.WriteAllLines(filePath, fileCode);
                }
            }
        }

        public static void AppendCodeToFile(string codeToAdd, int offset, string filePath, bool backupfile = true)
        {
            // General Function to append to file given code to add, and offset to the line from the bottom of the file where to add, complete file path, and if we should backup the original file
            var fileCode = File.ReadAllLines(filePath).ToList();
            bool codeExists = FindCode(codeToAdd, filePath);
            if (!codeExists)
            {
                int index = FindLine(offset, filePath);
                if (backupfile) BackupFile(filePath);
                if (index > 0)
                {
                    fileCode.Insert(index, codeToAdd);
                    File.WriteAllLines(filePath, fileCode);
                }
            }
        }

        private static bool FindCode(string lineToFind, string filePath)
        {
            string[] linesofCode = lineToFind.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var fileCode = File.ReadAllLines(filePath).ToList();
            int index = 0;
            int idx = 0;
            while(index == 0)
            {
                if (!string.IsNullOrEmpty(linesofCode[idx])) index = fileCode.IndexOf(linesofCode[idx]); // Ignore blank lines as sometimes are necessary.
                idx += 1;
            }

            return index > 0;
        }

        private static int FindLine(int offset, string filePath)
        {
            var fileCode = File.ReadAllLines(filePath).ToList();

            return fileCode.Count - offset;
        }

        public static void BackupFile(string filePath)
        {
            FileInfo codefile = new FileInfo(filePath);
            string fileName = codefile.Name;
            string[] files = Directory.GetFiles(codefile.Directory!.FullName, fileName.Replace(codefile.Extension, "*"));
            int totalFiles = files.Count();
            string newFilePath = filePath.Replace(codefile.Extension, ".bak" + totalFiles.ToString());
            if (totalFiles > 0)
            {
                while (File.Exists(newFilePath))
                {
                    totalFiles += 1;
                    newFilePath = filePath.Replace(codefile.Extension, ".bak" + totalFiles.ToString());
                }

                if (File.Exists(filePath))
                {
                    File.Move(filePath, newFilePath);
                }
            }
        }
        #endregion
    }
}