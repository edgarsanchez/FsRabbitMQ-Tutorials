open System
open System.Text
open RabbitMQ.Client
open RabbitMQ.Client.Events

/// Assumes only valid positive integer input.
/// Don't expect this one to work for big numbers, and it's probably the slowest recursive implementation possible.
let rec fib n = 
    if n = 1 || n = 0 then
        n
    else
        fib (n - 1) + fib (n - 2)

[<EntryPoint>]
let main _ =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()
    
    channel.QueueDeclare (queue = "rpc_queue", durable = false, exclusive = false, autoDelete = false, arguments = null) |> ignore
    channel.BasicQos (prefetchSize = 0u, prefetchCount = 1us, ``global`` = false)
    let consumer = EventingBasicConsumer (channel)
    channel.BasicConsume (queue = "rpc_queue", autoAck = false, consumer = consumer) |> ignore
    printfn " [*] Awaiting RPC requests"

    consumer.Received.AddHandler (fun _ ea ->
        let body = ea.Body.ToArray ()
        let message = Encoding.UTF8.GetString body
        let response =
            match Int32.TryParse message with
            | true, n ->
                printfn " [.] fib(%s)" message
                sprintf "%d" (fib n)
            | _ ->
                ""

        let responseBytes = ReadOnlyMemory (Encoding.UTF8.GetBytes response)
        let props = ea.BasicProperties
        let replyProps = channel.CreateBasicProperties (CorrelationId = props.CorrelationId)
        channel.BasicPublish (exchange = "", routingKey = props.ReplyTo, basicProperties = replyProps, body = responseBytes)
        channel.BasicAck (deliveryTag = ea.DeliveryTag, multiple = false) )

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0
