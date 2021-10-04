using System.ComponentModel;

namespace DN.WebApi.Domain.Constants
{
    public class Permissions
    {
        [DisplayName("Identity")]
        [Description("Identity Permissions")]
        public static class Identity
        {
            public const string Register = "Permissions.Identity.Register";
        }

        [DisplayName("Roles")]
        [Description("Roles Permissions")]
        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string ListAll = "Permissions.Roles.ViewAll";
            public const string Register = "Permissions.Roles.Register";
            public const string Update = "Permissions.Roles.Update";
            public const string Remove = "Permissions.Roles.Remove";
        }

        [DisplayName("Products")]
        [Description("Products Permissions")]
        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string ListAll = "Permissions.Products.ViewAll";
            public const string Register = "Permissions.Products.Register";
            public const string Update = "Permissions.Products.Update";
            public const string Remove = "Permissions.Products.Remove";
        }

        [DisplayName("Brands")]
        [Description("Brands Permissions")]
        public static class Brands
        {
            public const string View = "Permissions.Brands.View";
            public const string ListAll = "Permissions.Brands.ViewAll";
            public const string Register = "Permissions.Brands.Register";
            public const string Update = "Permissions.Brands.Update";
            public const string Remove = "Permissions.Brands.Remove";
        }

        [DisplayName("Company")]
        [Description("Companys Permissions")]
        public static class Companys
        {
            public const string View = "Permissions.Companys.View";
            public const string ListAll = "Permissions.Companys.ViewAll";
            public const string Register = "Permissions.Companys.Register";
            public const string Update = "Permissions.Companys.Update";
            public const string Remove = "Permissions.Companys.Remove";
        }

        [DisplayName("Note")]
        [Description("Notes Permissions")]
        public static class Notes
        {
            public const string View = "Permissions.Notes.View";
            public const string ListAll = "Permissions.Notes.ViewAll";
            public const string Register = "Permissions.Notes.Register";
            public const string Update = "Permissions.Notes.Update";
            public const string Remove = "Permissions.Notes.Remove";
        }

        [DisplayName("Person")]
        [Description("Persons Permissions")]
        public static class Persons
        {
            public const string View = "Permissions.Persons.View";
            public const string ListAll = "Permissions.Persons.ViewAll";
            public const string Register = "Permissions.Persons.Register";
            public const string Update = "Permissions.Persons.Update";
            public const string Remove = "Permissions.Persons.Remove";
        }
    }
}
