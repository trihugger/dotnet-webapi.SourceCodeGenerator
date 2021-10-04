Step 1 - Load the Data Model

Step 2 - Create a source code template class

Step 3 - Use the properties and methods from the engine to create the logic and customization you need for your template

Step 4 - Run and test and confirm its correct


PROPERTIES FROM ENGINE
    private string _appname = string.Empty; // Name of the Application. e.g. BlazorHero, or whatever name you gave your project when creating it.
    private string _modelnamespace = string.Empty; // Name of your Domain ModelInfo name, this is used to extract your base namespace for customization
    public string[] ExcludeProps = { "CreatedOn", "CreatedBy", "LastModifiedOn", "LastModifiedBy", "TenantKey" }; // List of Properties to exclude from code generator. i.e. Auditable properties because they are handled in the code already
    public IList<PropertyOption> PropertyOptions = new List<PropertyOption>(); // List of all properties in the model being generated.
    public IList<PropertyOption> AppModelOptions = new List<PropertyOption>(); // List of all App Models in this model. App Models are 1-to-1 models we will be linking
    public IList<PropertyOption> AppModelChildOptions = new List<PropertyOption>(); // List of all App Models that is a child to a 1-to-manu relation in which this model is a child to.
    public IList<PropertyOption> AppModelCollectionOptions = new List<PropertyOption>(); // List of all App ModelInfo Collections in this model. App ModelInfo Collections are 1-to-many models we will be linking
    public Type Model; // This is the ModelInfo type used to exctract information in our code generation
    public ModelInfo ModelInfo; // This is the ModelInfo we created in the Domain level and used in the our code generation
    public bool HasAppModel = false; // Determines if our ModelInfo has any linkage to other app models.
    public bool HasAppModelChild = false; // Determine if any of the app models is a child of a collections aka 1-to-many.
    public bool HasAppModelCollection = false; // Determines if our ModelInfo has any linkage to other app model collections.
    public string AppName;
    public string ModelNameSpace;

METHODS FROM ENGINE
    public CodeGeneratorEngine(ModelInfo _modelInfo, bool GetDescriptions = false) // This is the construct and does most of the work for you but all the public methods can be accessed invidually.
    public string GetProperties(bool ExcludeCollections = false, bool ExcludeAppModelsIds = false, bool ExcludeAppModels = false, bool UseDtosForAppModels = true) // This is used to generate properties as you would see it in a data model, DTO, etc.
    public string GetInLineProperties() // This property is used to create a line of properties from a datamodel in a row like "Guid Id, string Name, string Description" that you can use inline
    private List<PropertyOption> GetPropertyOptions() // Used in the constructor only and is the method that gathers all the information on the data model and helps populate most of those properties.
    private string GetTypeName(Type propertyInfo) // Used in the GetPropertyOptions method to help streamline the name of the type we are looking at in a property.

BLANKTEMPLATE.CS - Bare bones code for a source code template

DATA MODEL CONVENTIONS
When adding a property for another Data Model always use the [DataModel]Id convention for the Id and the property for the option should be the [DataModel] for both class and property name.
Use List, IList, or ICollection for your 1-to-many relationships

TODO:
Chain Generate to back fill dependencies
Make adjustments for Many-to-Many relationships
Generate the Constructor and Update method for Data Model