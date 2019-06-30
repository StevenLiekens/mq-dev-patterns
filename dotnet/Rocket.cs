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
    public class Rocket
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        static Rocket()
        {
            // Ensure that CTRL+C or CTRL+Break won't terminate the application, allowing graceful shutdown using a CancellationToken
            Console.CancelKeyPress += (sender, e) =>
            {
                switch (e.SpecialKey)
                {
                    case ConsoleSpecialKey.ControlC:
                        Console.WriteLine(
                            "CTRL + C pressed, terminating application when all operations have completed.");
                        break;
                    case ConsoleSpecialKey.ControlBreak:
                        Console.WriteLine(
                            "CTRL + Break pressed, terminating application when all operations have completed.");
                        break;
                }

                CancellationTokenSource.Cancel();
                e.Cancel = true;
            };
        }

        public static void Main(string[] args)
        {
            var env = MqEnvironment.Load("env.json");

            if (args.Length == 0)
            {
                Console.WriteLine("Say which program you want to run; put, get, pub, sub, request, response");
                Environment.Exit(0);
            }

            var xmsFactoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
            var connectionFactory = xmsFactoryFactory.CreateConnectionFactory();
            connectionFactory.SetStringProperty(XMSC.WMQ_HOST_NAME, env.Host);
            connectionFactory.SetIntProperty(XMSC.WMQ_PORT, env.Port);
            connectionFactory.SetStringProperty(XMSC.WMQ_CHANNEL, env.Channel);
            connectionFactory.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
            connectionFactory.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, env.Qmgr);
            if (env.KeyRepository != null && env.CipherSuite != null)
                connectionFactory.SetStringProperty(XMSC.WMQ_SSL_KEY_REPOSITORY, env.KeyRepository);
            if (env.CipherSuite != null)
                connectionFactory.SetStringProperty(XMSC.WMQ_SSL_CIPHER_SPEC, env.CipherSuite);

            //connectionFactory.SetStringProperty(XMSC.USERID, env.AppUser);
            //connectionFactory.SetStringProperty(XMSC.PASSWORD, env.AppPassword);

            try
            {
                switch (args[0])
                {
                    case "put":
                        new SimpleProducer(connectionFactory, env).PutMessage();
                        break;
                    case "get":
                        new SimpleConsumer(connectionFactory, env, CancellationTokenSource.Token).GetMessages();
                        break;
                    case "sub":
                        new SimpleSubscriber(connectionFactory, env, CancellationTokenSource.Token).SubscribeMessages();
                        break;
                    case "pub":
                        new SimplePublisher(connectionFactory, env).PublishMessage();
                        break;
                    case "request":
                        connectionFactory.SetStringProperty(XMSC.WMQ_TEMPORARY_MODEL, env.ModelQueueName);
                        new SimpleRequest(connectionFactory, env, CancellationTokenSource.Token).SendRequest();
                        break;
                    case "response":
                        new SimpleResponse(connectionFactory, env, CancellationTokenSource.Token).ReceiveMessages();
                        break;
                    default:
                        Console.WriteLine("Say which program you want to run; put, get, pub, sub, request, response");
                        break;
                }
            }
            catch (XMSException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}