using Humanizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SourceCodeGenerator.CodeGenerator
{
    public class CodeGeneratorEngine : IDisposable
    {
        #region PROPERTIES
        private string _appname = string.Empty; // Name of the Application. e.g. BlazorHero, or whatever name you gave your project when creating it.
        private string _modelnamespace = string.Empty; // Name of your Domain ModelInfo name, this is used to extract your base namespace for customization
        public string[] ExcludeProps = { "CreatedOn", "CreatedBy", "LastModifiedOn", "LastModifiedBy", "TenantKey", "DeletedOn", "DeletedBy" }; // List of Properties to exclude from code generator. i.e. Auditable properties because they are handled in the code already
        public IList<PropertyOption> PropertyOptions = new List<PropertyOption>(); // List of all properties in the model being generated.
        public IList<PropertyOption> AppModelOptions = new List<PropertyOption>(); // List of all App Models in this model. App Models are 1-to-1 models we will be linking
        public IList<PropertyOption> AppModelChildOptions = new List<PropertyOption>(); // List of all App Models that is a child to a 1-to-manu relation in which this model is a child to.
        public IList<PropertyOption> AppModelCollectionOptions = new List<PropertyOption>(); // List of all App ModelInfo Collections in this model. App ModelInfo Collections are 1-to-many models we will be linking
        public Type Model; // This is the ModelInfo type used to exctract information in our code generation
        public ModelInfo ModelInfo; // This is the ModelInfo we created in the Domain level and used in the our code generation
        public bool HasAppModel = false; // Determines if our ModelInfo has any linkage to other app models.
        public bool HasAppModelChild = false; // Determine if any of the app models is a child of a collections aka 1-to-many.
        public bool HasAppModelCollection = false; // Determines if our ModelInfo has any linkage to other app model collections.
        public string ExistsField = string.Empty;
        private bool _getDescriptions;

        public string AppName
        {
            get
            {
                if (string.IsNullOrEmpty(_appname))
                    _appname = EngineFunctions.GetRootApplication();
                return _appname;
            }

            set
            {
                _appname = EngineFunctions.GetRootApplication();
            }
        }

        public string ModelNameSpace
        {
            get
            {
                if (string.IsNullOrEmpty(_modelnamespace))
                    _modelnamespace = EngineFunctions.GetModelNameSpace();
                return _modelnamespace;
            }
            set
            {
                _modelnamespace = EngineFunctions.GetModelNameSpace();
            }
        }
        #endregion

        #region CONSTRUCTOR
        public CodeGeneratorEngine(ModelInfo _modelInfo, bool GetDescriptions = false)
        {
            ModelInfo = _modelInfo;
            MessageBox.Show($"Starting to process Fields for {_modelInfo.Name}");
            Model = EngineFunctions.GetModelType(ModelInfo.Namespace, ModelInfo.Name);
            _getDescriptions = GetDescriptions;
            PropertyOptions = GetPropertyOptions();
            int countModels = PropertyOptions.Where(s => s.IsAppModel == true).Count();
            AppModelOptions = PropertyOptions.Where(s => s.IsAppModel == true).ToList();
            HasAppModel = AppModelOptions.Count() > 0;
            countModels = PropertyOptions.Where(s => s.IsCollection == true).Count();
            AppModelCollectionOptions = PropertyOptions.Where(s => s.IsCollection == true).ToList();
            HasAppModelCollection = AppModelCollectionOptions.Count() > 0;
            AppModelChildOptions = PropertyOptions.Where(s => (s.IsAppModel == true && s.IsCollectionChild == true)).ToList();
            HasAppModelChild = AppModelChildOptions.Count() > 0;

            // Create a SelectField window
            List<string> fields = new List<string>();
            foreach(var item in PropertyOptions)
            {
                if (item.PropertTypeName.Equals("string") && !ExcludeProps.Contains(item.Name)) fields.Add(item.Name);
            }

            if (fields.Count() > 1)
            {
                SelectField selectedField = new SelectField(fields, "Please select the field to be used to find if a record exists in the database.", "Select Record Identifying Field");
                selectedField.ShowDialog();
                ExistsField = selectedField.SelectedField();
            }
            else
            {
                if (fields.Count() == 1) ExistsField = fields[0];
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

        #region CODE GENERATION AID FUNCTIONS
        public string GetProperties(bool ExcludeCollections = false, bool ExcludeAppModelsIds = false, bool ExcludeAppModels = false, bool UseDtosForAppModels = true)
        {
            // Create Properties for the model - used in the cretion of the data object
            // Used in Controller, FeatureXCommand files, Manager, MappedProfile, and Routes.
            var properties = new StringBuilder();

            foreach (PropertyOption property in PropertyOptions)
            {
                var propertyType = property.PropertyType;

                string propertyTypeName = property.PropertTypeName;

                if (!ExcludeProps.Contains(property.Name))
                {
                    if (propertyType.IsGenericType)
                    {
                        if (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) propertyTypeName = $"{propertyTypeName}?";
                    }
                    else if (property.IsAppModel)
                    {
                        if (!ExcludeAppModels)
                        {
                            string appmodelType = property.Name;
                            if (UseDtosForAppModels) { appmodelType += "Dto"; }
                            properties.Append(@$"        public {appmodelType} {property.Name} {{ get; set; }}
");
                        }
                    }
                    else if (property.IsCollection)
                    {
                        if (!ExcludeCollections)
                        {
                            string appModelType = property.PropertyType.Name;
                            bool isAppModelType = EngineFunctions.IsAppModel(appModelType);
                            if (isAppModelType && UseDtosForAppModels) { appModelType += "Dto";  }
                            properties.Append(@$"        public List<{appModelType}> {property.Name} {{ get; set; }}
");
                        }
                    }
                    else if (propertyType.Name.Contains("Guid"))
                    {
                        bool isAppModelId = EngineFunctions.IsAppModelId(property.Name);
                        if(!(ExcludeAppModelsIds && isAppModelId))
                        {
                            StringBuilder guidproperty = new StringBuilder();
                            guidproperty.Append(@$"        public {propertyType.Name} {property.Name} {{ get; set; }}
");
                            if (isAppModelId)
                            {
                                properties.Append(guidproperty);
                            }
                            else
                            {
                                guidproperty.Append(properties);
                                properties = guidproperty;
                            }
                        }
                    }
                    else
                    {
                        properties.Append(@$"        public {propertyTypeName} {property.Name} {{ get; set; }}
");
                    }
                }
            }

            return properties.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        public string GetInLineProperties(bool IncludeType = true, bool ExcludeCollections = false, bool ExcludeAppModelsIds = false, bool ExcludeAppModels = false, bool IsLineARequest = false, bool IsLinePropertyNameLowerCase = false)
        {
            // Create Properties for the commands.
            // Used in Controller, FeatureXCommand files, Manager, MappedProfile, and Routes.
            var properties = new StringBuilder();

            foreach (PropertyOption property in PropertyOptions)
            {
                string typeName = property.PropertTypeName.ToLower();

                Console.WriteLine(typeName);

                // Exclude App Models, Collections, and Properties in the exclusion list
                if (!ExcludeProps.Contains(property.Name) &&
                    !(ExcludeAppModels && property.IsAppModel) &&
                    !(ExcludeCollections && property.IsCollection) &&
                    !(ExcludeAppModelsIds && EngineFunctions.IsAppModelId(property.Name)) && property.CanWrite)
                {
                    string requestInsert = string.Empty;
                    string linetype = string.Empty;
                    string propertyName = property.Name;
                    if (IsLineARequest) requestInsert = "request.";
                    if (IncludeType) linetype = @$"{property.PropertTypeName} ";
                    if (IsLinePropertyNameLowerCase) propertyName = propertyName.ToLower();
                    properties.Append(@$"{linetype}{requestInsert}{propertyName}, ");
                }
            }

            return properties.ToString().TrimEnd().TrimEnd(',');
        }

        public string GetValidationRules()
        {
            StringBuilder iProperties = new StringBuilder();

            foreach (PropertyOption property in PropertyOptions)
            {
                if(!ExcludeProps.Contains(property.Name) && !property.IsAppModel && !property.IsCollection)
                {
                    string iproperty = @$"            RuleFor(p => p.{property.Name})";

                    // Fluent Built-in Validators https://docs.fluentvalidation.net/en/latest/built-in-validators.html

                    // String
                    // MaximumLength is set to 75 as default
                    if (property.PropertyType == typeof(string))
                    {
                        iproperty += ".MaximumLength(75)";
                    }

                    // Int/Double/Float
                    // Greater than or equal to 0
                    if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double) || property.PropertyType == typeof(float) || property.PropertyType == typeof(decimal))
                    {
                        iproperty += ".GreaterThanOrEqualTo(0)";
                    }

                    // DateTime should be greater or equal to today (exception would be for DoB or DoD)
                    if (property.PropertyType == typeof(DateTime))
                    {
                        iproperty += ".GreaterThanOrEqualTo(DateTime.Today)"; // TODO: check if this works??
                    }

                    // Skipping all other types

                    // Id's or All with required should be
                    // .NotNull() or .NotEmpty depending if it is object or native
                    if (property.Name.ToLower().Equals("id") || property.Required)
                    {
                        if (property.PropertyType.Namespace!.StartsWith("System"))
                        {
                            iproperty += ".NotEmpty()";
                        }
                        else
                        {
                            iproperty += ".NotNull()";
                        }
                    }

                    iproperty += @$";
";

                    iProperties.Append(iproperty);
                }
            }

            return iProperties.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        private List<PropertyOption> GetPropertyOptions()
        {
            List<PropertyOption> options = new List<PropertyOption>();

            foreach (var prop in Model.GetProperties())
            {
                PropertyOption propertyOption = new PropertyOption();

                // Determine if this property is a App ModelInfo
                propertyOption.IsAppModel = prop.PropertyType.FullName!.Contains(AppName);
                propertyOption.CanWrite = prop.CanWrite;
                propertyOption.CanRead = prop.CanRead;

                // Determine if this property is a Collection of an App ModelInfo
                if (prop.PropertyType.IsGenericType &&
                    (prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                        prop.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ||
                        prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    // NOTE: This covers ILists and ICollections do we need to cover other types?

                    propertyOption.IsAppModel = false; // NOTE: Check logic but this should probably be true for the purpose of e.g. List<DataModel>
                    propertyOption.IsCollection = true;
                } // Determines if property is a collection.

                // Determine Display in Table and AddEdit modal as well as require validation
                propertyOption.Name = prop.Name;
                switch (prop.Name.ToLower())
                {
                    case "createdby":
                    case "createdon":
                    case "lastmodifiedby":
                    case "lastmodifiedon":
                        propertyOption.TableDisplay = false;
                        propertyOption.AddEditDisplay = false;
                        propertyOption.Required = false;
                        break;
                    default:
                        propertyOption.TableDisplay = true;
                        propertyOption.AddEditDisplay = true;
                        propertyOption.Required = true;
                        break;
                }

                // Determine if field is an ID type (main ID of a model or App ModelInfo's properties) and override options accordantly.
                string last2 = prop.Name.Length > 2 ? prop.Name.ToLower().Substring(prop.Name.Length - 2) : prop.Name.ToLower();
                if (last2.Contains("id"))
                {
                    propertyOption.TableDisplay = false;
                    propertyOption.AddEditDisplay = false;
                    propertyOption.Required = propertyOption.IsAppModel;
                }

                // TODO: make determinations for min, max, and custom format for different property types, i.e. double, decimal, float, etc.
                propertyOption.MaxValue = -1;
                propertyOption.MinValue = -1;
                propertyOption.CustomFormat = string.Empty;

                // ########################################################################################
                // Determinations for PROPERTY TYPE based if its a collection or not
                // ########################################################################################
                if (propertyOption.IsCollection)
                {
                    propertyOption.PropertyType = prop.PropertyType.GenericTypeArguments[0];
                    propertyOption.PropertTypeName = GetTypeName(propertyOption.PropertyType);
                    propertyOption.TableDisplay = false; // override table display
                }
                else
                {
                    propertyOption.PropertyType = prop.PropertyType;
                    propertyOption.PropertTypeName = GetTypeName(prop.PropertyType);
                }

                // ########################################################################################
                // All logic below here must run after Property Type has been determined above.
                // ########################################################################################

                // Make specific determinations if its an App ModelInfo
                if (propertyOption.IsAppModel)
                {
                    ModelInfo appModel = EngineFunctions.GetModel(propertyOption.PropertyType, ModelNameSpace);
                    MessageBox.Show($"Found the property {prop.PropertyType.Name.Humanize(LetterCasing.Title)} in Data Model {ModelInfo.Name} which is a Data Model type. We will process all properties for {prop.PropertyType.Name.Humanize(LetterCasing.Title)} first before continuing with our select model.");
                    CodeGeneratorEngine appModelEngine = new CodeGeneratorEngine(appModel);

                    List<PropertyOption> modelCollections = appModelEngine.PropertyOptions.Where(s => (s.IsCollection == true) && (s.PropertyType.Name == ModelInfo.Name)).ToList();

                    propertyOption.IsCollectionChild = modelCollections.Count > 0;

                    MessageBoxResult result = MessageBox.Show($"The property {prop.PropertyType.Name.Humanize(LetterCasing.Title)} in {ModelInfo.Name}, is the Property a child of {ModelInfo.Name}?\r\nA data model property is a child when its role it to return data to the parrent model.", "Code Generator", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        propertyOption.IsAppModelChild = true;
                    }
                    else
                    {
                        propertyOption.IsAppModelChild = false;
                    }

                    appModelEngine.Dispose();

                    MessageBox.Show($"Finished with {prop.PropertyType.Name.Humanize(LetterCasing.Title)}, we will now continue with the remainding fields for {ModelInfo.Name}.");
                }

                if (propertyOption.PropertTypeName.ToLower().Contains("datetime"))
                {
                    // DateTime will use a variable to manage and does not need to be required as it will have a default value and will always be not null
                    propertyOption.Required = false;
                }

                // Add to the option list
                options.Add(propertyOption);
            }

            return options;
        }

        private string GetTypeName(Type propertyInfo)
        {
            string propertyTypeName = string.Empty;

            switch (propertyInfo.Name)
            {
                case "Guid":
                case "DateTime":
                    propertyTypeName = propertyInfo.Name;
                    break;
                case "Int32":
                    propertyTypeName = "int";
                    break;
                case "Boolean":
                case "boolean":
                    propertyTypeName = "bool";
                    break;
                default:
                    propertyTypeName = propertyInfo.Name.ToLower();
                    break;
            }

            if (propertyInfo.IsGenericType)
            {
                propertyTypeName = propertyInfo.GetGenericArguments()[0].Name;
            }

            return propertyTypeName;
        }
        #endregion
    }

    public class PropertyOption
    {
        // ModelInfo property information
        // e.g.
        // string AccountNumber {get; set;}

        // AppModel - TitleReleaseLoan {ModelName}
        // e.g. TitleReleaseFund {TypeName} Funds {PropertyName}
        // e.g. string {TypeName} CompanyName {PropertyName}
        // string ModelName = _modelInfo.Name; //Parent Name is ModelInfo Name
        // string modelname = ModelName.ToLower();
        // string PropertyName = property.Name; //PropertyName is the property given name in the model
        // string propertyName = PropertyName.ToLower();
        // string TypeName = property.PropertyType.Name; //TypeName is the type of the property e.g. string, double, Brand (i.e. app model)
        // string typeName = TypeName.ToLower();

        public string Name { get; set; } = string.Empty; // Property Name is the name of the Property Type in the example above it would be AccountNumber
        public bool TableDisplay { get; set; } // Determines if this property should be displayed in the over table of models
        public bool AddEditDisplay { get; set; } // Determines if this property should be included in the Add and Edit Modal
        public bool Required { get; set; } // Determines if this property is required for validation purposes
        public int MinValue { get; set; } // Determines if there is a minimal value for this property for validation purposes (Default value for dates is Today-100 years)| TODO: need more logic for amounts
        public int MaxValue { get; set; } // Determines if there is a minimal value for this property for validation purposes (Default value for dates is Today)| TODO: need more logic for amounts
        public string CustomFormat { get; set; } = string.Empty; // Helps determine valudation for other types | TODO: not implemented yet

        public Type PropertyType { get; set; } = typeof(object); // This is the type of the property. In the example above would be the Type String
        public bool CanRead { get; set; } // Determines if property can read, i.e. has a get implementation.
        public bool CanWrite { get; set; } // Determine if property can write, i.e. has a set implementation.
        public string PropertTypeName { get; set; } = string.Empty; // This is a lower case property type name. In the example above it would by string.
        public bool IsAppModel { get; set; } // Identified is this type is of another app model in this application.
        public bool IsAppModelChild { get; set; } // Identify if this app model is a child or a main
        public bool IsCollectionChild { get; set; } // If this is a AppModel then it identifies if the other model is a parent in a 1-to-many relationship or just a 1-to-1 relationship
        public string AppModelDescriptionField { get; set; } = string.Empty; // All AppModel are created with a dropdown in the AddEdit Modal by default, this field will be used to display in the dropdown.
        public bool IsCollection { get; set; } // Determines if this property is a collection of another property, right now only covers collections of other app models.
    }
}
