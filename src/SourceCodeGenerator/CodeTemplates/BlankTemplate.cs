﻿using SourceCodeGenerator.CodeGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceCodeGenerator.CodeTemplates
{
    public class BlankTemplate
    {
        private ModelInfo _modelInfo = new ModelInfo();
        private Type _model;
        private string _codefolder;
        private CodeGeneratorEngine _engine;
        private string _category;
        private string _properties;

        public BlankTemplate(CodeGeneratorEngine engine, bool BackupOriginal = true)
        {
            _engine = engine;
            _modelInfo = _engine.ModelInfo;
            _model = _engine.Model;
            _category = string.IsNullOrEmpty(_modelInfo.Category) ? string.Empty : $@".{_modelInfo.Category}";
            _properties = GenerateProperties(); // CHANGE/DELETE: Delete if you don't need custom Properties, Change to the _engine.GetProperties() if you want the standard get/set properties. NOTE: Don't forget to delete the private statement if you need to delete this.
            _codefolder = @$"\Shared\SharedDTOs\"; // CHANGE: The sub folders to add to your root application lives to identify the folder the generated code will live
            string fileName = @$"Create{_model.Name}Request.cs"; // CHANGE: Name of the file for this source code

            string rootPath = EngineFunctions.GetApplicationPath();
            string codePath = @$"{rootPath}{_codefolder}" + (string.IsNullOrEmpty(_modelInfo.Category) ? @$"\" : @$"\{_modelInfo.Category}\");
            Directory.CreateDirectory(codePath);
            string filePath = codePath + fileName;
            string code = GenerateCode();
            if (BackupOriginal) EngineFunctions.BackupFile(filePath);
            File.WriteAllText(filePath, code);
        }

        private string GenerateCode()
        {
            string iCode = @$"// Autogenerated by SourceCodeGenerator

namespace {_engine.AppName}.Shared.DTOs{_category}
{{
    public class Create{_model.Name}Request : IMustBeValid
    {{
        {_properties}
    }}
}}";
            return iCode;
        }

        private string GenerateProperties()
        {
            StringBuilder iProperties = new StringBuilder();

            foreach(PropertyOption property in _engine.AppModelOptions)
            {
                iProperties.Append($@"      {property.Name} = default;
");
            }

            return iProperties.ToString();
        }
    }
}