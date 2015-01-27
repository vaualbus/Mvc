// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ControllersFromServicesWebSite
{
    public class ControllerWithAutoWireupProperties : Controller
    {
        public QueryValueService Service { get; set; }

        [HttpGet("/autowireup/service")]
        public IActionResult ActionConsumingService()
        {
            return Content("Value from service: " + Service.GetValue());
        }

        [HttpGet("/autowireup/activated")]
        public IActionResult ActionUsingActivatedProperties(string value)
        {
            // ViewDataDictionary is activated.
            ViewData["Value"] = value;
            return View("~/ViewData");
        }
    }
}