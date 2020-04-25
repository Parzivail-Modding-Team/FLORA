using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FLORA.Mapping
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

            var parentMapping = _classes.FirstOrDefault(mapping => mapping.IntermediaryName == parent || mapping.MappedName == parent || mapping.OfficialName == parent);

            if (parentMapping == null)
                return Array.Empty<Mapping>();

            var childClassRegex = new Regex("^" + parentMapping.MappedName + "(?:\\$[^$]+)+$", RegexOptions.Compiled);

            mappings.AddRange(_classes.Where(mapping => childClassRegex.IsMatch(mapping.MappedName)));
            mappings.AddRange(_fields.Where(mapping => mapping.ParentOfficialName == parentMapping.OfficialName));
            mappings.AddRange(_methods.Where(mapping => mapping.ParentOfficialName == parentMapping.OfficialName));

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