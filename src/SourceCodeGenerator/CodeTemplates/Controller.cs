﻿using SourceCodeGenerator.CodeGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceCodeGenerator.CodeTemplates
{
    public class Controller
    {
        private ModelInfo _modelInfo = new ModelInfo();
        private Type _model;
        private string _codefolder;
        private CodeGeneratorEngine _engine;
        private string _category;
        private string _detaislmethod;

        public Controller(CodeGeneratorEngine engine, string version = "v1", bool BackupOriginal = true)
        {
            _engine = engine;
            _modelInfo = _engine.ModelInfo;
            _model = _engine.Model;
            _codefolder = @$"Bootstrapper\Controllers\" + version; // TODO: Build a version selection for the API
            string fileName = @$"{_model.Name}sController.cs";
            _category = string.IsNullOrEmpty(_modelInfo.Category) ? string.Empty : $@".{_modelInfo.Category}";
            _detaislmethod = GenerateDetailsMethod();

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

using {_engine.AppName}.Application.Abstractions.Services{_category};
using {_engine.AppName}.Domain.Constants;
using {_engine.AppName}.Infrastructure.Identity.Permissions;
using {_engine.AppName}.Shared.DTOs{_category};
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace {_engine.AppName}.Bootstrapper.Controllers.v1
{{
    public class {_model.Name}sController : BaseController
    {{
        private readonly I{_model.Name}Service _service;

        public {_model.Name}sController(I{_model.Name}Service service)
        {{
            _service = service;
        }}
{_detaislmethod}
        [HttpGet]
        [MustHavePermission(Permissions.{_model.Name}s.ListAll)]
        public async Task<IActionResult> GetListAsync({_model.Name}ListFilter filter)
        {{
            var {_model.Name.ToLower()}s = await _service.Get{_model.Name}sAsync(filter);
            return Ok({_model.Name.ToLower()}s);
        }}

        [HttpGet(""dapper"")]
        public async Task<IActionResult> GetDapperAsync(Guid id)
        {{
            var {_model.Name.ToLower()}s = await _service.GetByIdUsingDapperAsync(id);
            return Ok({_model.Name.ToLower()}s);
        }}

        [HttpPost]
        public async Task<IActionResult> CreateAsync(Create{_model.Name}Request request)
        {{
            return Ok(await _service.Create{_model.Name}Async(request));
        }}

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(Update{_model.Name}Request request, Guid id)
        {{
            return Ok(await _service.Update{_model.Name}Async(request, id));
        }}

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {{
            var {_model.Name.ToLower()}Id = await _service.Delete{_model.Name}Async(id);
            return Ok({_model.Name.ToLower()}Id);
        }}
    }}
}}";
            return iCode;
        }

        private string GenerateDetailsMethod()
        {
            StringBuilder detailsMethod = new StringBuilder();

            if (_engine.HasAppModel)
            {
                detailsMethod.Append($@"
        [HttpGet(""{{id}}"")]
        [MustHavePermission(Permissions.{_model.Name}s.View)]
        public async Task<IActionResult> GetAsync(Guid id)
        {{
            var {_model.Name.ToLower()} = await _service.Get{_model.Name}DetailsAsync(id);
            return Ok({_model.Name.ToLower()});
        }}
");
            }

            return detailsMethod.ToString();
        }
    }
}