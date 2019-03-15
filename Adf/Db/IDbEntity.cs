using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Db
{
    /// <summary>
    /// Support Reader Import Flag
    /// </summary>
    public interface IDbEntity
    {
        /// <summary>
        /// import reader
        /// </summary>
        /// <param name="reader"></param>
        void Initialize(IDbReader reader);
    }
}
