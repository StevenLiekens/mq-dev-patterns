﻿/*
* (c) Copyright IBM Corporation 2018
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Text;
using System.Threading;
using IBM.XMS;

namespace TryXMS
{
    public class SimpleRequest
    {
        private readonly IConnectionFactory _connectionFactory;

        private readonly CancellationToken _ct;

        private readonly MqEnvironment _env;

        public SimpleRequest(IConnectionFactory connectionFactory, MqEnvironment env, CancellationToken ct)
        {
            _connectionFactory = connectionFactory;
            _env = env;
            _ct = ct;
        }

        public void SendRequest()
        {
            using (var connection = _connectionFactory.CreateConnection(_env.AppUser, _env.AppPassword))
            using (var session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge))
            using (var destination = session.CreateQueue(_env.QueueName))
            {
                destination.SetIntProperty(XMSC.WMQ_TARGET_CLIENT, XMSC.WMQ_TARGET_DEST_MQ);
                using (var producer = session.CreateProducer(destination))
                using (var temporaryDestination = session.CreateTemporaryQueue())
                {
                    connection.Start();
                    var message = new JsonMessage("This is a simple put and your lucky number is ", 7);
                    var textMessage = session.CreateTextMessage(message.ToJsonString());

                    // Disable for correl id by using message id (why?)
                    textMessage.JMSCorrelationID = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    textMessage.JMSExpiration = 900000;
                    textMessage.JMSReplyTo = temporaryDestination;

                    var selector = $"JMSCorrelationID='ID:{FromAsciiToHex(textMessage.JMSCorrelationID)}'";

                    producer.Send(textMessage);


                    using (var consumer = session.CreateConsumer(temporaryDestination, selector))
                    {
                        while (!_ct.IsCancellationRequested)
                        {
                            var responseMessage = (ITextMessage) consumer.Receive(100);
                            if (responseMessage != null)
                            {
                                Console.WriteLine(responseMessage);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private string FromAsciiToHex(string ascii)
        {
            var bytes = Encoding.ASCII.GetBytes(ascii);
            return string.Concat(bytes.Select(num => num.ToString("X2")));
        }
    }
}