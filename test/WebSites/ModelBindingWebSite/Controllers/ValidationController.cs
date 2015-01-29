// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;

namespace ModelBindingWebSite.Controllers
{
    public class ValidationController : Controller
    {
        public bool SkipValidation(Resident resident)
        {
            return ModelState.IsValid;
        }

        public bool Something(Company1 f)
        {
            return ModelState.IsValid;
        }

    }

    public class Company1
    {
        public int Bar { get; set; }

        public Department1 Department { get; set; }
    }

    public class  Department1
    {
        public string Name { get; set; }

        public string Location { get; set; }
    }
}