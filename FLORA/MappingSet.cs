using System.Collections.Generic;
using LiteDB;

namespace FLORA
{
    internal class MappingSet
    {
        public YarnVersion YarnVersion { get; }

        private readonly ILiteCollection<Mapping> _classes;
        private readonly ILiteCollection<Mapping> _fields;
        private readonly ILiteCollection<Mapping> _methods;

        public MappingSet(YarnVersion yarnVersion, ILiteCollection<Mapping> classes, ILiteCollection<Mapping> fields, ILiteCollection<Mapping> methods)
        {
            YarnVersion = yarnVersion;
            _classes = classes;
            _fields = fields;
            _methods = methods;

            _classes.EnsureIndex(mapping => mapping.IntermediaryName);
            _fields.EnsureIndex(mapping => mapping.IntermediaryName);
            _methods.EnsureIndex(mapping => mapping.IntermediaryName);
        }

        public Mapping GetNamedClass(string intName)
        {
            return _classes.FindOne(mapping => mapping.IntermediaryShortName == intName);
        }

        public Mapping GetNamedField(string intName)
        {
            return _fields.FindOne(mapping => mapping.IntermediaryName == intName);
        }

        public Mapping GetNamedMethod(string intName)
        {
            return _methods.FindOne(mapping => mapping.IntermediaryName == intName);
        }

        public void InsertMappings(List<Mapping> classes, List<Mapping> fields, List<Mapping> methods)
        {
            _classes.InsertBulk(classes);
            _fields.InsertBulk(fields);
            _methods.InsertBulk(methods);
        }
    }
}