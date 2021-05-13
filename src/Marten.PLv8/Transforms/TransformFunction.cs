using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Weasel.Postgresql;
using Weasel.Postgresql.Functions;

namespace Marten.PLv8.Transforms
{
    public class TransformFunction: Function
    {
        public static readonly string Prefix = "mt_transform_";

        public readonly IList<string> OtherArgs = new List<string>();

        public TransformFunction(StoreOptions options, string name, string body)
            : base(new DbObjectName(options.DatabaseSchemaName, "mt_transform_" + name.Replace(".", "_")))
        {
            Name = name;
            Body = body;
        }

        public string Name { get; set; }
        public string Body { get; set; }

        private IEnumerable<string> allArgs()
        {
            return new[] { "doc" }.Concat(OtherArgs);
        }

        public override void WriteCreateStatement(DdlRules rules, TextWriter writer)
        {
            writer.WriteLine(GenerateFunction());
            writer.WriteLine();
        }

        public string ToDropSignature()
        {
            var signature = allArgs().Select(_ => $"JSONB").Join(", ");
            return $"DROP FUNCTION IF EXISTS {Identifier}({signature});";
        }

        public string GenerateFunction()
        {
            var defaultExport = "{export: {}}";

            var signature = allArgs().Select(x => $"{x} JSONB").Join(", ");
            var args = allArgs().Join(", ");

            return
                $@"
CREATE OR REPLACE FUNCTION {Identifier}({signature}) RETURNS JSONB AS $$

  var module = {defaultExport};

  {Body}

  var func = module.exports;

  return func({args});

$$ LANGUAGE plv8 IMMUTABLE STRICT;
";
        }

        public static TransformFunction ForFile(StoreOptions options, string file, string name = null)
        {
            var body = new FileSystem().ReadStringFromFile(file);
            name = name ?? Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

            return new TransformFunction(options, name, body);
        }

        public override string ToString()
        {
            return $"Transform Function '{Name}'";
        }
    }
}
