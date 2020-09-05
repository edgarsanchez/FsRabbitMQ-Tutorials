# FsRabbitMQ-Tutorials
F# implementation of [RabbitMQ tutorials](https://www.rabbitmq.com/getstarted.html), specifically based on the [C# samples](https://github.com/rabbitmq/rabbitmq-tutorials/tree/master/dotnet) available in github. Now we have all the Getting Started tutorials from the RabbitMQ site implemented:

* The ["Hello World!" tutorial](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html)
  * The src/Send folder contains the Sender console app
  * The src/Receive folder contains the Receive console app
* The [Work queues tutorial](https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html)
  * The src/NewTask folder contains a console app that publishes a message to the task_queue
  * The src/Worker folder contains a console app that subscribes to the task_queue
* The [Publish/Subscribe tutorial](https://www.rabbitmq.com/tutorials/tutorial-three-dotnet.html)
  * The src/EmitLog folder contains a console app that publishes a message to the logs exchange
  * The src/ReceiveLogs folder contains a console app that subscribes to the exchange with a temporary queue (many ReceiveLogs apps can be run simultaneously, each one will have its own temporary queue)
* The [Routing tutorial](https://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html)
  * The src/EmitLogDirect folder contains a console app that publishes a message to the direct_logs exchange using a severity level as routing key, e.g. `dotnet run warning "This is a warning!"`
  * The src/ReceiveLogsDirect folder contains a console app that subscribes to the exchange with a temporary queue and one or more routing keys, e.g. `dotnet run warning` (several ReceiveLogsDirect apps can be run simultaneously, each waiting for specific routing keys in a temporary queue)
* The [Topics tutorial](https://www.rabbitmq.com/tutorials/tutorial-five-dotnet.html)
  * The src/EmitLogTopic folder contains a console app that publishes a message to the topic_logs exchange using a topic as routing key, e.g. `dotnet run kern.critical "A critical kernel error."`
  * The src/ReceiveLogsTopic folder contains a console app that subscribes to the exchange with a temporary queue and one or more topics as routing keys, e.g. `dotnet run kern.*` (several ReceiveLogsTopic apps can be run simultaneously, each waiting for specific topics in a temporary queue)
* The [RPC tutorial](https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html)
  * The src/RPCServer folder contains a console app that waits for requests in the rpc_queue, when a request appears, the server does the job and send the result back using the queue from the `ReplyTo` property. The server must be started before any client, multiple servers can be started.
  * The src/RPCClient folder contains a console app that starts by creating a specific callback queue, each client has got its own callback queue. When the client sends a request, it sets the `ReplyTo` property to its own callback queue and the `CorrelationId` which is unique for every request. You invoke the client with `dotnet run 30` or any positive integer
* The [Publish Confirms tutorial](https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet.html) in the src/PublishConfirms folder. A single console app showing three ways for confirming that a published message was processed by the broker. To run the example all you have to do is type `dotnet run` inside the folder.

All the examples assume a RabbitMQ server installation on the local computer. By default, to run any of the console apps you just have to get into the corresponding folder and type `dotnet run`, some tutorials have more specific instructions.

Comments and feedback welcomed!
