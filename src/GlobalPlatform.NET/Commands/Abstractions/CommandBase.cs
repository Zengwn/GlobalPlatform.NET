﻿using System.Linq;
using GlobalPlatform.NET.Commands.Interfaces;
using Iso7816;

namespace GlobalPlatform.NET.Commands.Abstractions
{
    public abstract class CommandBase<TCommand, TBuilder> : IApduBuilder
        where TCommand : TBuilder, new()
    {
        protected byte P1;

        protected byte P2;

        /// <summary>
        /// Starts building the command. 
        /// </summary>
        public static TBuilder Build => new TCommand();

        public abstract CommandApdu AsApdu();

        public byte[] AsBytes() => this.AsApdu().Buffer.ToArray();
    }
}
