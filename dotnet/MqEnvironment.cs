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

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TryXMS
{
    public class MqEnvironment
    {
        public string AppUser { get; set; }

        public string Channel { get; set; }

        public string CipherSuite { get; set; }

        public string Host { get; set; }

        public string KeyRepository { get; set; }

        public string ModelQueueName { get; set; }

        public int Port { get; set; }

        public string Qmgr { get; set; }

        public string QueueName { get; set; }

        public string TopicName { get; set; }

        public string AppPassword { get; set; }


        public static MqEnvironment Load(string filePath)
        {
            return JsonConvert.DeserializeObject<MqEnvironment>(File.ReadAllText(filePath), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
        }
    }
}