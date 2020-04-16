using LiteDB;

namespace FLORA
{
    internal class MappingSet
    {
        public ILiteCollection<Mapping> Classes { get; set; }
        public ILiteCollection<Mapping> Fields { get; set; }
        public ILiteCollection<Mapping> Methods { get; set; }
    }
}