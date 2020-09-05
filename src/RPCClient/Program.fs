open System
open System.Collections.Concurrent
open System.Text
open System.Threading
open System.Threading.Tasks
open RabbitMQ.Client
open RabbitMQ.Client.Events

type RpcClient () =
    let queueName = "rpc_queue"

    let factory = ConnectionFactory (HostName = "localhost")
    let connection = factory.CreateConnection ()
    let channel = connection.CreateModel ()
    let replyQueueName = channel.QueueDeclare().QueueName
    let consumer = EventingBasicConsumer (channel)
    let callbackMapper = ConcurrentDictionary<string, TaskCompletionSource<string>> ()

    do
        consumer.Received.AddHandler (fun _ ea ->
            match callbackMapper.TryRemove ea.BasicProperties.CorrelationId with
            | true, tcs -> ea.Body.ToArray () |> Encoding.UTF8.GetString |> tcs.TrySetResult |> ignore
            | _         -> () )

    member __.CallAsync (message: string, ?cancellationToken: CancellationToken) =
        let correlationId = Guid.NewGuid().ToString ()
        let props = channel.CreateBasicProperties (CorrelationId = correlationId, ReplyTo = replyQueueName)
        let messageBytes = ReadOnlyMemory (Encoding.UTF8.GetBytes message)
        let tcs = TaskCompletionSource<string> ()
        callbackMapper.TryAdd (correlationId, tcs) |> ignore

        channel.BasicPublish(exchange = "", routingKey = queueName, basicProperties = props, body = messageBytes)

        channel.BasicConsume(consumer = consumer, queue = replyQueueName, autoAck = true) |> ignore

        let canToken = match cancellationToken with Some token -> token| None -> CancellationToken.None
        canToken.Register (fun _ -> callbackMapper.TryRemove correlationId |> ignore) |> ignore
        tcs.Task

    member __.Close () =
        connection.Close ()

let invokeAsync n =
    async {
        let rpcClient = RpcClient ()

        printfn " [x] Requesting fib(%s)" n
        let! response = rpcClient.CallAsync n |> Async.AwaitTask
        printfn " [.] Got '%s'" response

        rpcClient.Close ()        
    }

[<EntryPoint>]
let main argv =
    printfn "RPC Client"
    let n = if Array.isEmpty argv then "30" else Array.head argv
    async { do! invokeAsync n } |> Async.RunSynchronously

    printfn " Press [Enter] to exit."
    Console.ReadLine () |> ignore
    0 
