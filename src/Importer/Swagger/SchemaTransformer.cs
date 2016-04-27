using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace Importer.Swagger
{
    public class SchemaTransformer
    {
        readonly ILog log = LogManager.GetLogger(typeof (SchemaTransformer));

        public string Flatten(string model, string models)
        {
            return GetFlattened(Deserialize(model), Deserialize(models));
        }

        private void BuildSchemaReferenceMap(JObject model, JObject models, IDictionary<string, string> modelMap)
        {
            IDictionary<JToken, JToken> refs = new Dictionary<JToken, JToken>();
            FindReferences(model, refs);

            foreach (JToken @ref in refs.Keys)
            {
                var canonicalRef = @ref.ToString();

                var resource = GetDataType(@ref.ToString());
                var parentResource = GetParentDataType(@ref.Path);

                if (resource.Equals(parentResource, StringComparison.OrdinalIgnoreCase))
                    return;

                try
                {
                    var schemaName = GetSchemaName(canonicalRef);
                    var subSchema = GetSchema(schemaName, models);

                    // replace reference values with inline definitions
                    ReplaceRef((JObject) refs[@ref], schemaName);

                    BuildSchemaReferenceMap(subSchema, models, modelMap);
                    modelMap[schemaName] = SerializeExisting(subSchema);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        private JObject GetSchema(string schemaName, JObject models)
        {
            var token = models.GetValue(schemaName, StringComparison.CurrentCultureIgnoreCase);

            return (JObject) token;
        }

        private string GetFlattened(JObject model, JObject models)
        {
            IDictionary<string, string> schemaMap = new Dictionary<string, string>();

            BuildSchemaReferenceMap(model, models, schemaMap);

            ReplaceRefs(model, schemaMap);

            if (log.IsDebugEnabled)
            {
                try
                {
                    log.DebugFormat("Flattened schema to: {0}", model.ToString(Formatting.Indented));
                }
                catch (JsonException ignored)
                {

                }
            }

            var flattened = SerializeExisting(model);
            Validate(model);

            return flattened;
        }

        private void Validate(JObject rootNode)
        {
            var generator = new JsonSchemaGenerator() {ContractResolver = new CamelCasePropertyNamesContractResolver()};

            try
            {
                var schema = generator.Generate(typeof (SwaggerDocument));
                rootNode.IsValid(schema);

            }
            catch (JsonSerializationException e)
            {
                throw new ArgumentException("Invalid schema json was generated", e);
            }
            catch (Exception e)
            {
                return; // this should only happen from test code. JsonSchemaFactory not easily mocked
            }
        }

        /*
     * Add schema references as inline definitions to the root schema
     */

        private void ReplaceRefs(JToken root, IDictionary<string, string> schemaMap)
        {
            var definitionsNode = new JObject();

            foreach (var entry in schemaMap)
            {
                var schemaNode = Deserialize(entry.Value);
                definitionsNode[entry.Key] = schemaNode;
            }

            root["definitions"] = definitionsNode;
        }

        /*
         * Replace a reference node with an inline reference
         */

        private void ReplaceRef(JObject parent, string schemaName)
        {
            parent["$ref"] = new JValue("#/definitions/" + schemaName);
        }

        /*
         * Find all reference node in the schema tree. Build a map of the reference node to its parent
         */

        private void FindReferences(JToken node, IDictionary<JToken, JToken> refNodes)
        {
            try
            {
                var refNode = node["$ref"];
                if (refNode != null)
                {
                    refNodes[refNode] = node;
                }
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException;
            }
           
            foreach (JToken child in node.Children())
            {
                FindReferences(child, refNodes);
            }
        }

        /*
        * Attempt to serialize an existing schema
        * If this fails something is seriously wrong, because this schema has already been saved by the control plane
        */

        JObject Deserialize(string schemaText)
        {
            try
            {
                return JObject.Parse(schemaText);
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format(
                    "Invalid schema found. Could not deserialize schema: {0}, {1}", schemaText, e));
            }
        }

        /*
         * Attempt to serialize an existing schema
         * If this fails something is seriously wrong, because this schema has already been saved by the control plane
         */

        private string SerializeExisting(JObject root)
        {
            try
            {
                return root.ToString(Formatting.None);
            }
            catch (Exception e)
            {
                throw new JsonException("Could not serialize generated schema json", e);
            }
        }

        public static string GetSchemaName(string refVal)
        {
            string schemaName;

            try
            {
                schemaName = refVal.Substring(refVal.LastIndexOf("/") + 1);
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("Invalid reference found: {0}, {1}", refVal, e));
            }

            return schemaName;
        }

        public static string GetRestApiId(string refVal)
        {
            string apiId;

            try
            {
                apiId = refVal.Substring(refVal.IndexOf("restapis/"), refVal.Length).Split('/')[1];
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("Invalid reference found: {0}, {1}", refVal, e));
            }

            return apiId;
        }

        private string GetDataType(string defintionPath)
        {
            return defintionPath.Substring(defintionPath.LastIndexOf("/", StringComparison.Ordinal) + 1);
        }

        private string GetParentDataType(string refPath)
        {
            return refPath.Split('.')[0];
        }
    }
}
