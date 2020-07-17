open RabbitMQ.Client
open System
open System.Text

[<EntryPoint>]
let main _ =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()
    
    channel.QueueDeclare (queue = "hello", durable = false, exclusive = false, autoDelete = false, arguments = null) |> ignore

    let message = "Hello World!"
    let body = ReadOnlyMemory(Encoding.UTF8.GetBytes message)

    channel.BasicPublish (exchange = "", routingKey = "hello", basicProperties = null, body = body)
    printfn " [x] Sent %s" message

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0 
