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

namespace TryXMS
{
    public class SimpleConsumer
    {
        private readonly IConnectionFactory _connectionFactory;

        private readonly CancellationToken _ct;

        private readonly MqEnvironment _env;

        public SimpleConsumer(IConnectionFactory connectionFactory, MqEnvironment env, CancellationToken ct)
        {
            _connectionFactory = connectionFactory;
            _env = env;
            _ct = ct;
        }

        public void GetMessages()
        {
            using (var connection = _connectionFactory.CreateConnection(_env.AppUser, _env.AppPassword))
            using (var session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge))
            using (var destination = session.CreateQueue(_env.QueueName))
            using (var consumer = session.CreateConsumer(destination))
            {
                connection.Start();
                while (!_ct.IsCancellationRequested)
                {
                    var textMessage = (ITextMessage) consumer.Receive(100);
                    if (textMessage != null)
                        Console.WriteLine(textMessage);
                }
            }
        }
    }
}