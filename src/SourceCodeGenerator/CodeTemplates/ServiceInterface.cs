﻿using SourceCodeGenerator.CodeGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceCodeGenerator.CodeTemplates
{
    public class ServiceInterface
    {
        private ModelInfo _modelInfo = new ModelInfo();
        private Type _model;
        private string _codefolder;
        private CodeGeneratorEngine _engine;
        private string _category;
        private string _methods;

        public ServiceInterface(CodeGeneratorEngine engine, bool BackupOriginal = true)
        {
            _engine = engine;
            _modelInfo = _engine.ModelInfo;
            _model = _engine.Model;
            _codefolder = @$"Core\Application\Abstractions\Services\";
            string fileName = @$"I{_model.Name}Service.cs";
            _category = string.IsNullOrEmpty(_modelInfo.Category) ? string.Empty : $@".{_modelInfo.Category}";

            string rootPath = EngineFunctions.GetApplicationPath();
            string codePath = @$"{rootPath}{_codefolder}" + (string.IsNullOrEmpty(_modelInfo.Category) ? @$"\" : @$"\{_modelInfo.Category}\");
            Directory.CreateDirectory(codePath);
            string filePath = codePath + fileName;
            _methods = GenerateMethods();
            string code = GenerateCode();
            if (BackupOriginal) EngineFunctions.BackupFile(filePath);
            File.WriteAllText(filePath, code);
        }

        private string GenerateCode()
        {
            string iCode = @$"// Autogenerated by SourceCodeGenerator

using {_engine.AppName}.Shared.DTOs{_category};
using {_engine.AppName}.Application.Abstractions.Services;
using {_engine.AppName}.Application.Wrapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace {_engine.AppName}.Application.Abstractions.Services{_category}
{{
    public interface I{_model.Name}Service : ITransientService
    {{
{_methods}
    }}
}}";
            return iCode;
        }

        private string GenerateMethods()
        {
            StringBuilder methods = new StringBuilder();

            if (_engine.HasAppModel)
            {
                methods.Append($@"        Task<Result<{_model.Name}DetailsDto>> Get{_model.Name}DetailsAsync(Guid id);
");
            }

            methods.Append($@"        Task<PaginatedResult<{_model.Name}Dto>> Get{_model.Name}sAsync({_model.Name}ListFilter filter);
        Task<Result<{_model.Name}Dto>> GetByIdUsingDapperAsync(Guid id);
        Task<Result<Guid>> Create{_model.Name}Async(Create{_model.Name}Request request);
        Task<Result<Guid>> Update{_model.Name}Async(Update{_model.Name}Request request, Guid id);
        Task<Result<Guid>> Delete{_model.Name}Async(Guid id);
");
            return methods.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}
