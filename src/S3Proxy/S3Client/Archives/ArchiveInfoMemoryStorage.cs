using System.Collections.Generic;

namespace ABSA.RD.S4.S3Proxy.S3Client.Archives
{
    class ArchiveInfoMemoryStorage : IArchiveInfoStorage
    {
        private readonly Dictionary<string, ArchiveInfo> _storage = new Dictionary<string, ArchiveInfo>();
        private readonly object _locker = new object();

        public bool TryGet(string reference, out ArchiveInfo info)
        {
            lock (_locker)
                return _storage.TryGetValue(reference, out info);
        }

        public void Save(string reference, ArchiveInfo zipInfo)
        {
            lock (_locker)
            {
                if (_storage.ContainsKey(reference))
                    _storage[reference] = zipInfo;
                else
                    _storage.Add(reference, zipInfo);
            }
        }
    }
}