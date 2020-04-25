using System.Collections.Generic;

namespace FLORA.Mapping
{
    internal interface IMappingSource
    {
        Mapping GetClassByInt(string intName);
        Mapping GetFieldByInt(string intName);
        Mapping GetMethodByInt(string intName);
        Mapping GetClassByObf(string obfName);
        Mapping GetFieldByObf(string obfName);
        Mapping GetMethodByObf(string obfName);
        Mapping[] Search(string name);
        Mapping[] GetChildren(string parent);
        void InsertMappings(List<Mapping> classes, List<Mapping> fields, List<Mapping> methods);
    }
}