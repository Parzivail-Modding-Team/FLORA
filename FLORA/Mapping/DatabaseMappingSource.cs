using System;
using System.Collections.Generic;
using System.Net.Mail;
using LiteDB;

namespace FLORA
{
    internal class DatabaseMappingSource : IMappingSource
    {
        public YarnVersion YarnVersion { get; }

        private readonly ILiteCollection<Mapping> _classes;
        private readonly ILiteCollection<Mapping> _fields;
        private readonly ILiteCollection<Mapping> _methods;

        public DatabaseMappingSource(YarnVersion yarnVersion, ILiteCollection<Mapping> classes, ILiteCollection<Mapping> fields, ILiteCollection<Mapping> methods)
        {
            YarnVersion = yarnVersion;
            _classes = classes;
            _fields = fields;
            _methods = methods;

            _classes.EnsureIndex(mapping => mapping.IntermediaryName);
            _fields.EnsureIndex(mapping => mapping.IntermediaryName);
            _methods.EnsureIndex(mapping => mapping.IntermediaryName);
        }

        public Mapping GetClassByInt(string intName)
        {
            return _classes.FindOne(mapping => mapping.IntermediaryShortName == intName);
        }

        public Mapping GetFieldByInt(string intName)
        {
            return _fields.FindOne(mapping => mapping.IntermediaryName == intName);
        }

        public Mapping GetMethodByInt(string intName)
        {
            return _methods.FindOne(mapping => mapping.IntermediaryName == intName);
        }

        /// <inheritdoc />
        public Mapping GetClassByObf(string obfName)
        {
            return _classes.FindOne(mapping => mapping.OfficialName == obfName);
        }

        /// <inheritdoc />
        public Mapping GetFieldByObf(string obfName)
        {
            return _fields.FindOne(mapping => mapping.OfficialName == obfName);
        }

        /// <inheritdoc />
        public Mapping GetMethodByObf(string obfName)
        {
            return _methods.FindOne(mapping => mapping.OfficialName == obfName);
        }

        /// <inheritdoc />
        public Mapping[] Search(string name)
        {
            var mappings = new List<Mapping>();

            mappings.AddRange(_classes.Find(mapping => mapping.IntermediaryName == name || mapping.MappedName == name || mapping.OfficialName == name));
            mappings.AddRange(_fields.Find(mapping => mapping.IntermediaryName == name || mapping.MappedName == name));
            mappings.AddRange(_methods.Find(mapping => mapping.IntermediaryName == name || mapping.MappedName == name));

            return mappings.ToArray();
        }

        /// <inheritdoc />
        public Mapping[] GetChildren(string parent)
        {
            var mappings = new List<Mapping>();

            var parentOfficialName = _classes.FindOne(mapping => mapping.IntermediaryName == parent || mapping.MappedName == parent || mapping.OfficialName == parent)?.OfficialName;

            if (parentOfficialName == null)
                return Array.Empty<Mapping>();

            mappings.AddRange(_classes.Find(mapping => mapping.ParentOfficialName == parentOfficialName));
            mappings.AddRange(_fields.Find(mapping => mapping.ParentOfficialName == parentOfficialName));
            mappings.AddRange(_methods.Find(mapping => mapping.ParentOfficialName == parentOfficialName));

            return mappings.ToArray();
        }

        public void InsertMappings(List<Mapping> classes, List<Mapping> fields, List<Mapping> methods)
        {
            _classes.InsertBulk(classes);
            _fields.InsertBulk(fields);
            _methods.InsertBulk(methods);
        }
    }
}