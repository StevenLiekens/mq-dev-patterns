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

using IBM.XMS;

namespace TryXMS
{
    public class SimplePublisher
    {
        private readonly IConnectionFactory _connectionFactory;

        private readonly MqEnvironment _env;

        public SimplePublisher(IConnectionFactory connectionFactory, MqEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public void PublishMessage()
        {
            using (var connection = _connectionFactory.CreateConnection(_env.AppUser, _env.AppPassword))
            using (var session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge))
            using (var destination = session.CreateTopic(_env.TopicName))
            using (var producer = session.CreateProducer(destination))
            {
                connection.Start();
                var textMessage = session.CreateTextMessage(new JsonMessage("This is a simple publisher and your lucky number is ").ToJsonString());
                producer.Send(textMessage);
            }
        }
    }
}