﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Mangagers
{
    public abstract class AMngr
    {
        protected AzureConnection AzureConnection = AzureConnection.getInstance();
    }
}