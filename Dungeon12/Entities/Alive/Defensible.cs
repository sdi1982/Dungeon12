﻿using Dungeon.Network;

namespace Dungeon12.Entities.Alive
{
    /// <summary>
    /// Умеет защищаться
    /// </summary>
    public class Defensible : Alive
    {

        /// <summary>
        /// 
        /// <para>
        /// [Рассчётное через сеть]
        /// </para>
        /// </summary>
        [Dungeon.Proxied(typeof(NetProxy))]
        public long Defence { get => Get(___Defence, typeof(Defensible).AssemblyQualifiedName); set => Set(value,typeof(Defensible).AssemblyQualifiedName); }
        private long ___Defence = 0;


        /// <summary>
        /// 
        /// <para>
        /// [Рассчётное через сеть]
        /// </para>
        /// </summary>
        [Dungeon.Proxied(typeof(NetProxy))]
        public long Barrier { get => Get(___Barrier, typeof(Defensible).AssemblyQualifiedName); set => Set(value, typeof(Defensible).AssemblyQualifiedName); }
        private long ___Barrier = 0;
    }
}
