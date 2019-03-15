using System;

namespace Adf
{
    /// <summary>
    /// object helper 
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// dispose a object
        /// </summary>
        /// <param name="disposableObject"></param>
        public static void Dispose(IDisposable disposableObject)
        {
            if (disposableObject != null)
            {
                disposableObject.Dispose();
            }
        }

        /// <summary>
        /// dispose a object and no throw error
        /// </summary>
        /// <param name="disposableObject"></param>
        public static void TryDispose(IDisposable disposableObject)
        {
            if (disposableObject != null)
            {
                try
                {
                    disposableObject.Dispose();
                }
                catch(Exception)
                {}
            }
        }
    }
}
