using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Swagger
{
    public class Key
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Enabled { get; set; }
        public List<string> Stages { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine();
            builder.AppendLine($"Name {Name}");
            builder.AppendLine($"Description:  {Description}");
            builder.AppendLine($"CreatedDate:  {CreatedDate}");
            builder.AppendLine($"Enabled:  {Enabled}");
            builder.AppendLine("Stages:");

            foreach (var stage in Stages)
            {
                builder.AppendLine($"\tApiId/Stage: {stage}");
            }

            return builder.ToString();
        }
    }
}