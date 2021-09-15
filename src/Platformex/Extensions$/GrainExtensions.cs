using Orleans;
using System;

namespace Platformex
{
    public static class GrainExtensions
    {
        public static TIdentity GetId<TIdentity>(this IGrain grain)
        {
            var strId = grain.GetGrainIdentity().PrimaryKeyString;
            return (TIdentity)Activator.CreateInstance(typeof(TIdentity), strId);
        }
    }
}