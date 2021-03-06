﻿///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2011 Patchwork Consulting. All rights reserved.                      /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Messaging;
using System.Threading;
using com.espertech.esper.client;

namespace NEsper.Catalyst.Client.Consumers
{
    class MsmqEventConsumer : IDisposable
    {
        /// <summary>
        /// Message queue
        /// </summary>
        private MessageQueue _messageQueue;

        /// <summary>
        /// Event handler for this consumer
        /// </summary>
        private EventHandler<UpdateEventArgs> _eventHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsmqEventConsumer"/> class.
        /// </summary>
        /// <param name="mqPath">The mq path.</param>
        /// <param name="eventHandler"></param>
        public MsmqEventConsumer(string mqPath, EventHandler<UpdateEventArgs> eventHandler)
        {
            if (!MessageQueue.Exists(mqPath)) {
                throw new ArgumentException("invalid msmq path", "mqPath");
            }

            _eventHandler = eventHandler;
            _messageQueue = new MessageQueue(mqPath);
            _messageQueue.Formatter = new BinaryMessageFormatter();
            _messageQueue.ReceiveCompleted += FinishReceive;
            _messageQueue.BeginReceive();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _eventHandler = null;

            var messageQueue = Interlocked.Exchange(ref _messageQueue, null);
            if (messageQueue != null) {
                messageQueue.Close();
                messageQueue.Dispose();
            }
        }

        /// <summary>
        /// Finishes the asynchronous receive.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Messaging.ReceiveCompletedEventArgs"/> instance containing the event data.</param>
        private void FinishReceive(object sender, ReceiveCompletedEventArgs e)
        {
            if (_messageQueue == null) {
                return;
            }

            try
            {
                _messageQueue.EndReceive(e.AsyncResult);
                // receive the current message
                var message = e.Message;
                var eventArgs = ((string) message.Body).ToUpdateEventArgs();
                // send the event(s) along
                _eventHandler.Invoke(null, eventArgs);
            } catch(MessageQueueException) {
            } catch(NullReferenceException) {
            } finally {
                // begin receiving the next message
                _messageQueue.BeginReceive();
            }
        }
    }
}
