using System;
using OB.Api.Core;

namespace OB.DL.Common.Interfaces
{
    internal interface IContextProvider : IDisposable
    {
        /// <summary>Gets the db context.</summary>
        /// <value>The db context.</value>
        IObjectContext Context { get; }
    }
}