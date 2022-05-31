using OB.BL.Operations.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Extensions
{
    public static class BusinessLayerExceptionExtensions
    {
        public static void ValidateToThrowException(this BusinessLayerException exception)
        {
            if (exception != null)
                throw exception;
        }
    }
}
