open RabbitMQ.Client
open RabbitMQ.Client.Events
open System
open System.Text

[<EntryPoint>]
let main _ =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()
    
    channel.QueueDeclare (queue = "hello", durable = false, exclusive = false, autoDelete = false, arguments = null) |> ignore
    printfn " [*] Waiting for messages."

    let consumer = EventingBasicConsumer (channel)
    consumer.Received.AddHandler (fun _ ea ->
        let body = ea.Body.ToArray ()
        let message = Encoding.UTF8.GetString body
        printfn " [x] Received %s" message )
    channel.BasicConsume (queue = "hello", autoAck = true, consumer = consumer) |> ignore

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0 
