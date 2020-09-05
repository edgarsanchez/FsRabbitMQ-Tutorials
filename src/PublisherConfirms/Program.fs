open System
open System.Collections.Concurrent
open System.Diagnostics
open System.Linq
open System.Text
open System.Threading
open RabbitMQ.Client

let messageCount = 50_000

let createConnection () =
    let factory = ConnectionFactory (HostName = "localhost")
    factory.CreateConnection ()

let publishMessagesIndividually () =
    use connection = createConnection ()
    use channel = connection.CreateModel ()

    let queueName = channel.QueueDeclare().QueueName
    channel.ConfirmSelect ()

    let timer = Stopwatch ()
    timer.Start ()
    for i in 0 .. messageCount - 1 do
        let body = ReadOnlyMemory (Encoding.UTF8.GetBytes (string i))
        channel.BasicPublish (exchange = "", routingKey = queueName, basicProperties = null, body = body)
        channel.WaitForConfirmsOrDie (TimeSpan (0, 0, 5))
    
    timer.Stop ()
    printfn "Published %d messages individually in %d ms" messageCount timer.ElapsedMilliseconds

let publishMessagesInBatch () =
    use connection = createConnection ()
    use channel = connection.CreateModel ()

    let queueName = channel.QueueDeclare().QueueName
    channel.ConfirmSelect ()

    let batchSize = 100

    let rec processBatches countInBatch count =
        if count < messageCount then
            let body = ReadOnlyMemory (Encoding.UTF8.GetBytes (string count))
            channel.BasicPublish (exchange = "", routingKey = queueName, basicProperties = null, body = body)
            if countInBatch < batchSize then
                processBatches (countInBatch + 1) (count + 1)
            else
                channel.WaitForConfirmsOrDie (TimeSpan (0, 0, 5))
                processBatches 0 (count + 1)
        else
            countInBatch

    let timer = Stopwatch ()
    timer.Start ()
    if processBatches 0 0 > 0 then    
        channel.WaitForConfirmsOrDie (TimeSpan (0, 0, 5))
    timer.Stop ()
    printfn "Published %d messages in batch in %d ms" messageCount timer.ElapsedMilliseconds

let rec waitUntil waited numberOfMilliSeconds condition =
    if condition () || waited >= numberOfMilliSeconds then
        condition ()
    else
        Thread.Sleep 100
        waitUntil (waited + 100) numberOfMilliSeconds condition

let handlePublishConfirmsAsynchronously () =
    use connection = createConnection ()
    use channel = connection.CreateModel ()

    let queueName = channel.QueueDeclare().QueueName
    channel.ConfirmSelect ()

    let outstandingConfirms = ConcurrentDictionary ()

    let cleanOutstandingConfirms sequenceNumber multiple =
        if multiple then
            outstandingConfirms.Where (fun k -> k.Key <= sequenceNumber)
            |> Seq.iter (fun entry -> outstandingConfirms.TryRemove entry.Key |> ignore)
        else
            outstandingConfirms.TryRemove sequenceNumber |> ignore

    channel.BasicAcks.AddHandler (fun _ ea -> cleanOutstandingConfirms ea.DeliveryTag ea.Multiple)
    channel.BasicNacks.AddHandler (fun _ ea ->
        let _, body = outstandingConfirms.TryGetValue ea.DeliveryTag
        printfn "Message with body %s has been nack-ed. Sequence number: %d, multiple: %b" body ea.DeliveryTag ea.Multiple
        cleanOutstandingConfirms ea.DeliveryTag ea.Multiple )

    let timer = Stopwatch ()
    timer.Start ()
    for i in 0 .. messageCount - 1 do
        let body = string i
        outstandingConfirms.TryAdd (channel.NextPublishSeqNo, string i) |> ignore
        channel.BasicPublish (exchange = "", routingKey = queueName, basicProperties = null, body = ReadOnlyMemory (Encoding.UTF8.GetBytes body))

    if not (waitUntil 0 (60*1000) (fun () -> outstandingConfirms.IsEmpty)) then
        raise (Exception ("All messages could not be confirmed in 60 seconds"))

    timer.Stop ()
    printfn "Published %d messages and handled confirm asynchronously %d ms" messageCount timer.ElapsedMilliseconds

[< EntryPoint >]
let main _ =
    publishMessagesIndividually ()
    publishMessagesInBatch ()
    handlePublishConfirmsAsynchronously ()

    0