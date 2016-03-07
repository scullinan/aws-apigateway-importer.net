using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace ApiGatewayImporter
{
    public class PatchOperationBuilder
    {
        private readonly List<PatchOperation> operations = new List<PatchOperation>();

        public static PatchOperationBuilder With()
        {
            return new PatchOperationBuilder();
        }

        public PatchOperationBuilder Operation(string op, string path, string value, string from = null)
        {
            operations.Add(new PatchOperation() {
                Value = value,
                Op = op,
                Path = path,
                From = from
            });

            return this;
        }

        public List<PatchOperation> ToList()
        {
            return operations;
        }
    }
}