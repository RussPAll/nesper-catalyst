﻿///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2011 Patchwork Consulting. All rights reserved.                      /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Linq;
using com.espertech.esper.client;

namespace NEsper.Catalyst.Common
{
    [DataContract(
        Namespace = "http://www.patchwork-consulting.org")]
    public class StatementDescriptor
    {
        /// <summary>
        /// Gets or sets the unique identifier for the statement.
        /// </summary>
        /// <value>The id.</value>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the URI that defines the location of the event stream.
        /// </summary>
        /// <value>The URI.</value>
        [DataMember]
        public string[] URIs { get; set; }

        /// <summary>
        /// Returns true if statement is a pattern
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pattern; otherwise, <c>false</c>.
        /// </value>
        /// <returns>true if statement is a pattern</returns>
        [DataMember]
        public bool IsPattern { get; set; }

        /// <summary>
        /// Gets the statement's current state
        /// </summary>
        /// <value>The state.</value>
        [DataMember]
        public EPStatementState State { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        [DataMember]
        public XElement EventType { get; set; }
    }

    [CollectionDataContract(
        Name = "StatementDescriptorCollection",
        Namespace = "http://www.patchwork-consulting.org",
        ItemName = "Descriptor")]
    public class StatementDescriptorCollection : Collection<StatementDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementDescriptorCollection"/> class.
        /// </summary>
        public StatementDescriptorCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementDescriptorCollection"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public StatementDescriptorCollection(IList<StatementDescriptor> list) : base(list)
        {
        }
    }
}
