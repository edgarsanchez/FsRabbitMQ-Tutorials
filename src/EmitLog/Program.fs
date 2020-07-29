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
    
    channel.ExchangeDeclare (exchange = "logs", ``type`` = ExchangeType.Fanout)

    let message = getMessage argv
    let body = ReadOnlyMemory (Encoding.UTF8.GetBytes message)
    channel.BasicPublish (exchange = "logs", routingKey = "", basicProperties = null, body = body)
    printfn " [x] Sent %s" message

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0 
