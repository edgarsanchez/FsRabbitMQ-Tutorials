open RabbitMQ.Client
open System
open System.Text

[<EntryPoint>]
let main argv =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()

    channel.ExchangeDeclare (exchange = "topic_logs", ``type`` = ExchangeType.Topic)

    let routingKey = if Array.length argv > 0 then Array.head argv else "anonymous.info"
    let message = if Array.length argv > 1 then
                        String.Join (" ", Array.skip 1 argv)
                    else
                        "Hello World!" 
    
    let body = ReadOnlyMemory (Encoding.UTF8.GetBytes message)
    channel.BasicPublish (exchange = "topic_logs", routingKey = routingKey, basicProperties = null, body = body)
    printfn " [x] Sent '%s': '%s'" routingKey message

    0
