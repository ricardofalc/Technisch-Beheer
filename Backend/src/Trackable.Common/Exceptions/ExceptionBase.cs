// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackable.Common.Exceptions
{
    public class ExceptionBase : Exception
    {
        public ExceptionBase(string userFriendlyMessage)
            :base(userFriendlyMessage)
        {

        }
    }
}
