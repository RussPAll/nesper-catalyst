﻿///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2011 Patchwork Consulting. All rights reserved.                      /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace NEsper.Catalyst
{
    public interface IEventConsumer : IDisposable
    {
        /// <summary>
        /// Gets the URI.
        /// </summary>
        /// <value>The URI.</value>
        Uri Uri { get; }

        /// <summary>
        /// Event handler for xml events.
        /// </summary>
        event Action<XElement> XmlEvent;

        /// <summary>
        /// Event handler for dictionary events.
        /// </summary>
        event Action<string, IDictionary<string, object>> DictionaryEvent;
    }
}
