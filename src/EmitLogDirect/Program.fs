open RabbitMQ.Client
open System
open System.Text

[<EntryPoint>]
let main argv =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()

    channel.ExchangeDeclare (exchange = "direct_logs", ``type`` = ExchangeType.Direct)

    let severity = if Array.length argv > 0 then Array.head argv else "info"
    let message = if Array.length argv > 1 then
                        String.Join (" ", Array.skip 1 argv)
                    else
                        "Hello World!" 
    
    let body = ReadOnlyMemory (Encoding.UTF8.GetBytes message)
    channel.BasicPublish (exchange = "direct_logs", routingKey = severity, basicProperties = null, body = body)
    printfn " [x] Sent '%s': '%s'" severity message

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0
