using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace APIGateway.Swagger
{
    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class SwaggerDocument
    {
        public readonly string Swagger = "2.0";

        public Info Info;
        public string Host;
        public string BasePath;
        public IList<string> Schemes;
        public IList<string> Consumes;
        public IList<string> Produces;
        public IDictionary<string, PathItem> Paths;
        public IDictionary<string, Schema> Definitions;
        public IDictionary<string, Parameter> Parameters;
        public IDictionary<string, Response> Responses;
        public IDictionary<string, SecurityScheme> SecurityDefinitions;
        public IList<IDictionary<string, IEnumerable<string>>> Security;
        public IList<Tag> Tags;
        public ExternalDocs ExternalDocs;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }

    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class Info
    {
        public string Version;
        public string Title;
        public string Description;
        public string TermsOfService;
        public Contact Contact;
        public License License;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }

    public class Contact
    {
        public string Name;
        public string Url;
        public string Email;
    }

    public class License
    {
        public string Name;
        public string Url;
    }

    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class PathItem
    {
        [JsonProperty("$Ref")] public string Ref;
        public Operation Get;
        public Operation Put;
        public Operation Post;
        public Operation Delete;
        public Operation Options;
        public Operation Head;
        public Operation Patch;
        public IList<Parameter> Parameters;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }

    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class Operation
    {
        public IList<string> Tags;
        public string Summary;
        public string Description;
        public ExternalDocs ExternalDocs;
        public string OperationId;
        public IList<string> Consumes;
        public IList<string> Produces;
        public IList<Parameter> Parameters;
        public IDictionary<string, Response> Responses;
        public IList<string> Schemes;
        public bool Deprecated;
        public IList<IDictionary<string, IEnumerable<string>>> Security;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }

    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class Tag
    {
        public string Name;
        public string Description;
        public ExternalDocs ExternalDocs;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }

    public class ExternalDocs
    {
        public string Description;
        public string Url;
    }

    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class Parameter : PartialSchema
    {
        [JsonProperty("$Ref")] public string Ref;
        public string Name;
        public string In;
        public string Description;
        public bool? Required;
        public Schema Schema;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }

    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class Schema
    {
        [JsonProperty("$ref")] public string Ref;
        public string Format;
        public string Title;
        public string Description;
        public object @Default;
        public int? MultipleOf;
        public int? Maximum;
        public bool? ExclusiveMaximum;
        public int? Minimum;
        public bool? ExclusiveMinimum;
        public int? MaxLength;
        public int? MinLength;
        public string Pattern;
        public int? MaxItems;
        public int? MinItems;
        public bool? UniqueItems;
        public int? MaxProperties;
        public int? MinProperties;
        public IList<string> Required;
        public IList<object> Enum;
        public string Type;
        public Schema Items;
        public IList<Schema> AllOf;
        public IDictionary<string, Schema> Properties;
        public Schema AdditionalProperties;
        public string Ddiscriminator;
        [ApiGatewayJsonIgnore]
        public bool? ReadOnly;
        public Xml Xml;
        public ExternalDocs ExternalDocs;
        public object Example;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }

    public class PartialSchema
    {
        public string Type;
        public string Format;
        public PartialSchema Items;
        public string CollectionFormat;
        public object Default;
        public int? Maximum;
        public bool? ExclusiveMaximum;
        public int? Minimum;
        public bool? ExclusiveMinimum;
        public int? MaxLength;
        public int? MinLength;
        public string Pattern;
        public int? MaxItems;
        public int? MinItems;
        public bool? UniqueItems;
        public IList<object> Enum;
        public int? MultipleOf;
    }

    [JsonConverter(typeof(ResponseConverter))]
    public class Response
    {
        public string Description;
        public Schema Schema;
        public IDictionary<string, Header> Headers;
        public object Examples;
    }

    public class Header : PartialSchema
    {
        public string Description;
        public bool? Required;
    }

    public class Xml
    {
        public string Name;
        public string Namespace;
        public string Prefix;
        public bool? Attribute;
        public bool? Wrapped;
    }

    [JsonConverter(typeof (VendorExtensionsConverter))]
    public class SecurityScheme
    {
        public string Type;
        public string Description;
        public string Name;
        public string In;
        public string Flow;
        public string AuthorizationUrl;
        public string TokenUrl;
        public IDictionary<string, string> Scopes;
        public Dictionary<string, object> VendorExtensions = new Dictionary<string, object>();
    }
}
