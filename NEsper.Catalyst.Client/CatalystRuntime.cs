﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.util;

namespace NEsper.Catalyst.Client
{
    using Common;

    class CatalystRuntime
        : EPRuntime
    {
        private readonly Catalyst _adapter;
        private readonly WebChannelFactory<IControlManager> _webChannelFactory;
        private readonly string _instanceId;
        private ChannelWrapper<IControlManager> _controlManagerWrapper;

        /// <summary>
        /// Gets or sets an event handler to receive events that are unmatched by any statement.
        /// </summary>
        public event EventHandler<UnmatchedEventArgs> UnmatchedEvent
        {
            add { throw new NotSupportedException(); }
            remove { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalystRuntime"/> class.
        /// </summary>
        /// <param name="adapter">The catalyst adapter.</param>
        /// <param name="instanceId">The instance id.</param>
        public CatalystRuntime(Catalyst adapter, string instanceId)
        {
            _adapter = adapter;
            _webChannelFactory = adapter.WebChannelFactory;
            _instanceId = instanceId;
        }

        /// <summary>
        /// Gets a control manager.  The control manager is disposed with the runtime, so it should not
        /// be wrapped elsewhere.
        /// </summary>
        /// <returns></returns>
        private IControlManager GetControlManager()
        {
            if (_controlManagerWrapper == null) {
                _controlManagerWrapper = new ChannelWrapper<IControlManager>(_webChannelFactory.CreateChannel());
            }

            return _controlManagerWrapper.Channel;
        }

        private readonly IDictionary<Type, DataContractJsonSerializer> _serializerTable =
            new Dictionary<Type, DataContractJsonSerializer>();

        /// <summary>
        /// Gets the serializer associated with the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private DataContractJsonSerializer GetSerializer(Type type)
        {
            lock (_serializerTable) {
                var serializer = _serializerTable.Get(type);
                if (serializer != null) {
                    return serializer;
                }

                if (type.IsSerializable) {
                    serializer = new DataContractJsonSerializer(type);
                    _serializerTable[type] = serializer;
                    return serializer;
                }

                // look for a data contract in the hierarchy of types
                var dataContractType = type.FindAttributeInTypeTree(typeof (DataContractAttribute));
                if (dataContractType != null) {
                    serializer = new DataContractJsonSerializer(dataContractType);
                    _serializerTable[type] = serializer;
                    return serializer;
                }

                throw new EPException("type is not annotated as DataContract and is not Serializable");
            }
        }

        /// <summary>
        /// Serializes an object ... at least to the degree that it can.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        private string SerializeJson(object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var dictionaryWriter = JsonReaderWriterFactory.CreateJsonWriter(memoryStream, Encoding.UTF8, false)) {
                    var serializer = GetSerializer(obj.GetType());
                    serializer.WriteObject(dictionaryWriter, obj);
                    dictionaryWriter.Close();
                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
        }

        /// <summary>
        /// Send an event represented by a plain object to the event stream processing
        /// runtime.
        /// </summary>
        /// <param name="obj">is the event to sent to the runtime</param>
        public void SendEvent(object obj)
        {
            var controlManager = GetControlManager();
            var serialized = SerializeJson(obj);
            var eventArgs = new JsonEventArgs(obj.GetType().FullName, serialized);
            controlManager.SendJsonEvent(_instanceId, eventArgs);
        }

        /// <summary>
        /// Send a map containing event property values to the event stream processing
        /// runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// </summary>
        /// <param name="map">map that contains event property values. Keys are expected to be of type string while value scan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName. </param>
        /// <param name="eventTypeName">the name for the Map event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        public void SendEvent(IDictionary<string, object> map, string eventTypeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send an event represented by a LINQ element to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// </summary>
        /// <param name="element">The element.</param>
        public void SendEvent(XElement element)
        {
            var controlManager = GetControlManager();
            controlManager.SendXmlEvent(_instanceId, element);
        }

        /// <summary>
        /// Send an event represented by a DOM node to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        public void SendEvent(XmlNode node)
        {
            var controlManager = GetControlManager();
            var xelement = XElement.Load(new XmlNodeReader(node));
            controlManager.SendXmlEvent(_instanceId, xelement);
        }

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// </summary>
        /// <param name="evnt">to route internally for processing by the event stream processing runtime</param>
        public void Route(object evnt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// </summary>
        /// <param name="map">map that contains event property values. Keys are expected to be of type string while valuescan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName. </param>
        /// <param name="eventTypeName">the name for Map event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        public void Route(IDictionary<string, object> map, string eventTypeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// </summary>
        /// <param name="element">The LINQ element as an event.</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        public void Route(XElement element)
        {
            var controlManager = GetControlManager();
            controlManager.SendXmlEvent(_instanceId, element);
        }

        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event has
        /// priority over other events sent to the runtime. In a single-threaded application
        /// the routed event is processed before the next event is sent to the runtime through
        /// the EPRuntime.SendEvent method.
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        public void Route(XmlNode node)
        {
            var controlManager = GetControlManager();
            var xelement = XElement.Load(new XmlNodeReader(node));
            controlManager.SendXmlEvent(_instanceId, xelement);
        }

        /// <summary>
        /// Returns the current variable value. A null value is a valid value for a
        /// variable.
        /// </summary>
        /// <param name="variableName">is the name of the variable to return the value for</param>
        /// <returns>
        /// current variable value
        /// </returns>
        /// <throws>VariableNotFoundException if a variable by that name has not been declared</throws>
        public object GetVariableValue(string variableName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns current variable values for each of the variable names passed in,
        /// guaranteeing consistency in the face of concurrent updates to the variables.
        /// </summary>
        /// <param name="variableNames">is a set of variable names for which to return values</param>
        /// <returns>
        /// map of variable name and variable value
        /// </returns>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        public IDictionary<string, object> GetVariableValue(ICollection<string> variableNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns current variable values for all variables, guaranteeing consistency in
        /// the face of concurrent updates to the variables.
        /// </summary>
        /// <returns>
        /// map of variable name and variable value
        /// </returns>
        public IDictionary<string, object> VariableValueAll
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Sets the value of a single variable.
        /// </summary>
        /// <param name="variableName">is the name of the variable to change the value of</param>
        /// <param name="variableValue">is the new value of the variable, with null an allowed value</param>
        /// <throws>VariableValueException if the value does not match variable type or cannot be safely coercedto the variable type </throws>
        /// <throws>VariableNotFoundException if the variable name has not been declared</throws>
        public void SetVariableValue(string variableName, object variableValue)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the value of multiple variables in one update, applying all or none of the
        /// changes to variable values in one atomic transaction.
        /// </summary>
        /// <param name="variableValues">is the map of variable name and variable value, with null an allowed value</param>
        /// <throws>VariableValueException if any value does not match variable type or cannot be safely coercedto the variable type </throws>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        public void SetVariableValue(IDictionary<string, object> variableValues)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a facility to process event objects that are of a known type.
        /// </summary>
        /// <param name="eventTypeName">is the name of the event type</param>
        public IEventSender GetEventSender(string eventTypeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// For use with plug-in event representations, returns a facility to process event
        /// objects that are of one of a number of types that one or more of the registered
        /// plug-in event representation extensions can reflect upon and provide an event for.
        /// </summary>
        /// <param name="uris">
        /// is the URIs that specify which plug-in event representations may process an event object.
        /// <para/>URIs do not need to match event representation URIs exactly, a child (hierarchical) match is enough for an event representation to participate.
        /// <para/>The order of URIs is relevant as each event representation's factory is asked in turn to process the event, until the first factory processes the event.
        /// </param>
        public IEventSender GetEventSender(Uri[] uris)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute an on-demand query.
        /// <para/>
        /// On-demand queries are EPL queries that execute non-continuous fire-and-forget
        /// queries against named windows.
        /// </summary>
        /// <param name="epl">is the EPL to execute</param>
        /// <returns>
        /// query result
        /// </returns>
        public EPOnDemandQueryResult ExecuteQuery(string epl)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepare an on-demand query before execution and for repeated execution.
        /// </summary>
        /// <param name="epl">to prepare</param>
        /// <returns>
        /// proxy to execute upon, that also provides the event type of the returned results
        /// </returns>
        public EPOnDemandPreparedQuery PrepareQuery(string epl)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Number of events evaluated over the lifetime of the event stream processing
        /// runtime, or since the last ResetStats() call.
        /// </summary>
        /// <returns>
        /// number of events received
        /// </returns>
        public long NumEventsEvaluated
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Reset number of events received and emitted
        /// </summary>
        public void ResetStats()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the event renderer for events generated by this runtime.
        /// </summary>
        /// <returns>
        /// event renderer
        /// </returns>
        public IEventRenderer EventRenderer
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns current engine time.
        /// <para/>
        /// If time is provided externally via timer events, the function returns current
        /// time as externally provided.
        /// </summary>
        /// <returns>
        /// current engine time
        /// </returns>
        public long CurrentTime
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the time at which the next schedule execution is expected, returns null if no schedule execution is
        /// outstanding.
        /// </summary>
        public long? NextScheduledTime
        {
            get { throw new NotImplementedException(); }
        }
    }
}