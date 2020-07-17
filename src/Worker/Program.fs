open RabbitMQ.Client
open RabbitMQ.Client.Events
open System
open System.Text
open System.Threading

[<EntryPoint>]
let main _ =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()
    
    channel.QueueDeclare (queue = "task_queue", durable = true, exclusive = false, autoDelete = false, arguments = null) |> ignore
    channel.BasicQos (prefetchSize = 0u, prefetchCount = 1us, ``global`` = false)
    printfn " [*] Waiting for messages."

    let consumer = EventingBasicConsumer (channel)
    consumer.Received.AddHandler (fun _ ea ->
        let body = ea.Body.ToArray ()
        let message = Encoding.UTF8.GetString body
        printfn " [x] Received %s" message 
        
        let dots = message.Split('.').Length - 1
        Thread.Sleep (dots * 1000)
        printfn " [x] Done"

        // here channel could also be accessed as ((EventingBasicConsumer)sender).Model
        channel.BasicAck (deliveryTag = ea.DeliveryTag, multiple = false) )

    channel.BasicConsume (queue = "task_queue", autoAck = false, consumer = consumer) |> ignore

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0
