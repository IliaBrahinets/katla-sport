using AutoMapper;
using KatlaSport.Services.HiveManagement;

namespace KatlaSport.Services.Tests
{
    internal class MapperInitializer
    {
        private static bool _initizlized = false;
        private static object _lock = new object();

        public static void Initialize()
        {
            lock (_lock)
            {
                if (!_initizlized)
                {
                    Mapper.Initialize(cfg => cfg.AddProfile(new HiveManagementMappingProfile()));
                    _initizlized = true;
                }
            }
        }
    }
}
