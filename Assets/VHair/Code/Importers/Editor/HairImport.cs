using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VHair.Editor
{
    /// <summary>
    /// Static helper class for dealing with hair import.
    /// </summary>
    public static class HairImport
    {
        private static List<IHairAssetImporter> _importers = null;
        public static List<IHairAssetImporter> GetImporters()
        {
            if (ReferenceEquals(_importers, null))
            {
                _importers = AppDomain.CurrentDomain.GetAssemblies().SelectMany((asm) => asm.GetTypes()).Where((type) => typeof(IHairAssetImporter).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).Select((type) => Activator.CreateInstance(type)).Cast<IHairAssetImporter>().ToList();
            }
            
            return _importers;
        }
    }
}
