using System;
using System.Collections.Generic;
using System.Linq;

namespace FLORA
{
    internal class LocalMappingSource : IMappingSource
    {
        private List<Mapping> _classes;
        private List<Mapping> _fields;
        private List<Mapping> _methods;

        /// <inheritdoc />
        public Mapping GetClassByInt(string intName)
        {
            return _classes.FirstOrDefault(mapping => mapping.IntermediaryShortName == intName);
        }

        /// <inheritdoc />
        public Mapping GetFieldByInt(string intName)
        {
            return _fields.FirstOrDefault(mapping => mapping.IntermediaryShortName == intName);
        }

        /// <inheritdoc />
        public Mapping GetMethodByInt(string intName)
        {
            return _methods.FirstOrDefault(mapping => mapping.IntermediaryShortName == intName);
        }

        /// <inheritdoc />
        public Mapping GetClassByObf(string obfName)
        {
            return _classes.FirstOrDefault(mapping => mapping.OfficialName == obfName);
        }

        /// <inheritdoc />
        public Mapping GetFieldByObf(string obfName)
        {
            return _fields.FirstOrDefault(mapping => mapping.OfficialName == obfName);
        }

        /// <inheritdoc />
        public Mapping GetMethodByObf(string obfName)
        {
            return _methods.FirstOrDefault(mapping => mapping.OfficialName == obfName);
        }

        /// <inheritdoc />
        public Mapping[] Search(string name)
        {
            var mappings = new List<Mapping>();

            mappings.AddRange(_classes.Where(mapping => mapping.IntermediaryName == name || mapping.MappedName == name || mapping.OfficialName == name));
            mappings.AddRange(_fields.Where(mapping => mapping.IntermediaryName == name || mapping.MappedName == name));
            mappings.AddRange(_methods.Where(mapping => mapping.IntermediaryName == name || mapping.MappedName == name));

            return mappings.ToArray();
        }

        /// <inheritdoc />
        public Mapping[] GetChildren(string parent)
        {
            var mappings = new List<Mapping>();

            var parentOfficialName = _classes.FirstOrDefault(mapping => mapping.IntermediaryName == parent || mapping.MappedName == parent || mapping.OfficialName == parent)?.OfficialName;

            if (parentOfficialName == null)
                return Array.Empty<Mapping>();

            mappings.AddRange(_classes.Where(mapping => mapping.ParentOfficialName == parentOfficialName));
            mappings.AddRange(_fields.Where(mapping => mapping.ParentOfficialName == parentOfficialName));
            mappings.AddRange(_methods.Where(mapping => mapping.ParentOfficialName == parentOfficialName));

            return mappings.ToArray();
        }

        /// <inheritdoc />
        public void InsertMappings(List<Mapping> classes, List<Mapping> fields, List<Mapping> methods)
        {
            _classes = classes;
            _fields = fields;
            _methods = methods;
        }
    }
}