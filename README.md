# FsRabbitMQ-Tutorials
F# implementation of [RabbitMQ tutorials](https://www.rabbitmq.com/getstarted.html), specifically based on the [C# samples](https://github.com/rabbitmq/rabbitmq-tutorials/tree/master/dotnet) available in github. So far I've got implemented:

* The "Hello World!" tutorial
  * The src/Send folder contains the Sender console app
  * The src/Receive folder contains the Receive console app
* The Work queues tutorial
  * The src/NewTask folder contains the console app that publishes a message to the task_queue
  * The src/Worker folder contains the console app that subscribes to the task_queue

All the examples assume a RabbitMQ server installation on the local computer. To run any of the console apps you just have to get into the corresponding folder and type:

    dotnet run

Comments and feedback welcomed!
