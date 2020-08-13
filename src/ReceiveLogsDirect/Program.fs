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
            eprintfn "Usage: %s [info] [warning] [error]" (Environment.GetCommandLineArgs().[0])
            1
        else
            channel.ExchangeDeclare (exchange = "direct_logs", ``type`` = ExchangeType.Direct)
            let queueName = channel.QueueDeclare().QueueName

            for severity in argv do
                channel.QueueBind (queue = queueName, exchange = "direct_logs", routingKey = severity)
            
            printfn " [*] Waiting for messages."

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
