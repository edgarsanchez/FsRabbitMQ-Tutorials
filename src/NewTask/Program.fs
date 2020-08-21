open RabbitMQ.Client
open System
open System.Text

let getMessage (argv: string array) =
    if Array.length argv > 0 then
        String.Join (' ', argv)
    else
        "Hello World!"

[<EntryPoint>]
let main argv =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()
    
    channel.QueueDeclare (queue = "task_queue", durable = true, exclusive = false, autoDelete = false, arguments = null) |> ignore

    let message = getMessage argv
    let body = ReadOnlyMemory (Encoding.UTF8.GetBytes message)

    let properties = channel.CreateBasicProperties ()
    properties.Persistent <- true

    channel.BasicPublish (exchange = "", routingKey = "task_queue", basicProperties = properties, body = body)
    printfn " [x] Sent %s" message

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0 
