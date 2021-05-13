using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Marten.Storage;
using Weasel.Postgresql;

namespace Marten.PLv8.Transforms
{
    public interface ITransforms
    {
        void LoadFile(string file, string name = null);

        void LoadDirectory(string directory);

        void LoadJavascript(string name, string script);

        void Load(TransformFunction function);

        TransformFunction For(string name);

        IEnumerable<TransformFunction> AllFunctions();
    }

    public class Transforms: ITransforms, IFeatureSchema
    {
        private readonly StoreOptions _options;

        private readonly IDictionary<string, TransformFunction> _functions
            = new Dictionary<string, TransformFunction>();

        public Transforms(StoreOptions options)
        {
            _options = options;
        }

        private void AddFunction(TransformFunction function)
        {
            if (!_functions.ContainsKey(function.Name))
            {
                _functions.Add(function.Name, function);
            }
        }

        public void LoadFile(string file, string name = null)
        {

            if (!Path.IsPathRooted(file))
            {
                file = AppContext.BaseDirectory.AppendPath(file);
            }

            var function = TransformFunction.ForFile(_options, file, name);
            AddFunction(function);
        }

        public void LoadDirectory(string directory)
        {

            if (!Path.IsPathRooted(directory))
            {
                directory = AppContext.BaseDirectory.AppendPath(directory);
            }

            new FileSystem().FindFiles(directory, FileSet.Deep("*.js")).Each(file =>
            {
                LoadFile(file);
            });
        }

        public void LoadJavascript(string name, string script)
        {
            var func = new TransformFunction(_options, name, script);
            AddFunction(func);
        }

        public void Load(TransformFunction function)
        {
            AddFunction(function);
        }

        public TransformFunction For(string name)
        {
            if (!_functions.TryGetValue(name, out var function))
            {
                throw new ArgumentOutOfRangeException(nameof(name), "Unknown Transform Name");
            }

            return function;
        }

        public IEnumerable<TransformFunction> AllFunctions()
        {
            return _functions.Values;
        }

        public IEnumerable<Type> DependentTypes()
        {
            yield break;
        }

        public bool IsActive(StoreOptions options)
        {
            return _functions.Any();
        }

        public ISchemaObject[] Objects
        {
            get
            {
                return _functions.Values.OfType<ISchemaObject>().ToArray();
            }
        }

        public Type StorageType { get; } = typeof(Transforms);
        public string Identifier { get; } = "transforms";

        public void WritePermissions(DdlRules rules, TextWriter writer)
        {
            // Nothing
        }
    }
}
