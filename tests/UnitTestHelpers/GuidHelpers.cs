using System;

namespace CRDT.UnitTestHelpers
{
    public static class GuidHelpers
    {
        public static Guid GenerateGuid(char startsWith, Guid? guid = null)
        {
            if (guid == null)
            {
                guid = Guid.NewGuid();
            }

            var guidString = guid.ToString();
            var newGuidString = startsWith + guidString.Substring(1);

            return Guid.Parse(newGuidString);
        }
    }
}