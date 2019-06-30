/*
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
using System.Threading;
using IBM.XMS;
using Newtonsoft.Json;

namespace TryXMS
{
    public class SimpleResponse
    {
        private readonly IConnectionFactory _connectionFactory;

        private readonly CancellationToken _ct;

        private readonly MqEnvironment _env;

        public SimpleResponse(IConnectionFactory connectionFactory, MqEnvironment env, CancellationToken ct)
        {
            _connectionFactory = connectionFactory;
            _env = env;
            _ct = ct;
        }

        public void ReceiveMessages()
        {
            using (var connection = _connectionFactory.CreateConnection(_env.AppUser, _env.AppPassword))
            using (var session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge))
            using (var destination = session.CreateQueue(_env.QueueName))
            using (var consumer = session.CreateConsumer(destination))
            {
                connection.Start();
                while (!_ct.IsCancellationRequested)
                {
                    var request = (ITextMessage) consumer.Receive(100);
                    if (request != null)
                    {
                        Console.WriteLine(request);
                        Sendresponse(request, session);
                        break;
                    }
                }
            }
        }

        private void Sendresponse(ITextMessage request, ISession session)
        {
            var replyDestination = request.JMSReplyTo;
            if (replyDestination != null)
            {
                var replyMessage = session.CreateTextMessage();
                var producer = session.CreateProducer(replyDestination);
                replyMessage.JMSCorrelationID = request.JMSCorrelationID;

                var v = JsonConvert.DeserializeObject<ReplyMessage>(request.Text);
                v.Text = "The squared number is: ";
                v.Value *= v.Value;
                replyMessage.Text = JsonConvert.SerializeObject(v);
                producer.SetIntProperty(XMSC.DELIVERY_MODE, XMSC.DELIVERY_NOT_PERSISTENT);
                producer.Send(replyMessage);
            }
        }

        public class ReplyMessage
        {
            public string Text { get; set; }

            public int Value { get; set; }
        }
    }
}