open RabbitMQ.Client
open RabbitMQ.Client.Events
open System
open System.Text

[<EntryPoint>]
let main _ =
    let factory = ConnectionFactory (HostName = "localhost")
    use connection = factory.CreateConnection ()
    use channel = connection.CreateModel ()
    
    channel.ExchangeDeclare (exchange = "logs", ``type`` = ExchangeType.Fanout)

    let queueName = channel.QueueDeclare().QueueName
    channel.QueueBind (queue = queueName, exchange = "logs", routingKey = "")
    printfn " [*] Waiting for logs."

    let consumer = EventingBasicConsumer (channel)
    consumer.Received.AddHandler (fun _ ea ->
        let body = ea.Body.ToArray ()
        let message = Encoding.UTF8.GetString body
        printfn " [x] Received %s" message )
        
    channel.BasicConsume (queue = queueName, autoAck = true, consumer = consumer) |> ignore

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0
