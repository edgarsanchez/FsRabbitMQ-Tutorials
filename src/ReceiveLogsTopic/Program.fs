open RabbitMQ.Client
open RabbitMQ.Client.Events
open System
open System.Text

[<EntryPoint>]
let main argv =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()

    let exitCode =         
        if Array.length argv < 1 then
            eprintfn "Usage: %s [binding_key...]" (Environment.GetCommandLineArgs().[0])
            1
        else
            channel.ExchangeDeclare (exchange = "topic_logs", ``type`` = ExchangeType.Topic)
            let queueName = channel.QueueDeclare().QueueName

            for bindingKey in argv do
                channel.QueueBind (queue = queueName, exchange = "topic_logs", routingKey = bindingKey)
            
            printfn " [*] Waiting for messages. To exit press Control-C"

            let consumer = EventingBasicConsumer (channel)
            consumer.Received.AddHandler (fun _ ea ->
                let body = ea.Body.ToArray ()
                let message = Encoding.UTF8.GetString body
                let routingKey = ea.RoutingKey
                printfn " [x] Received '%s': '%s'" routingKey message )
                
            channel.BasicConsume (queue = queueName, autoAck = true, consumer = consumer) |> ignore
            0

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    exitCode
