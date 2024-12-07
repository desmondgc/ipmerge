// For more information see https://aka.ms/fsharp-console-apps

open System
open System.IO
open System.Linq
open System.Net

printfn "Hello from F#"

type IPOrCidr =
    | IP of IPAddress
    | Cidr of IPNetwork
    
    override this.ToString() =
        match this with
        | IP ip -> ip.ToString()
        | Cidr cidr -> cidr.ToString()
        
let readIPAddresses (path: string) =
    seq {
        use reader = new StreamReader(path)
        while not reader.EndOfStream do
            yield IPAddress.Parse(reader.ReadLine())
    }
    
let subnetSize (prefixLength: int) =
    if prefixLength < 0 || prefixLength > 31 then
        invalidArg (nameof prefixLength) $"Value passed in was %d{prefixLength}."
    else
        1 <<< (32 - prefixLength)

let ipToInt (x: IPAddress) =
    let bytes = x.GetAddressBytes()
    (uint32 bytes[0] <<< 24) ||| (uint32 bytes[1] <<< 16) ||| (uint32 bytes[2] <<< 8) ||| (uint32 bytes[3])
    
let intToIP (x: uint32) =
    IPAddress(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(int x)))
    
let masks = Array.init 31 (fun index -> 0xFFFFFFFFu <<< (31 - index))
    
let getMask prefixLength = masks[prefixLength - 1]
    
let getNetAddress ip prefixLength =
    ipToInt ip &&& getMask prefixLength

/// <remarks>
/// | prefix len | subnet size | threshold |
/// | ---------- | ----------- | --------- |
/// |     29     |    8  (6)   |      5    |
/// |     28     |   16 (14)   |     12    |
/// |     27     |   32 (30)   |     27    |
/// |     26     |   64 (62)   |     58    |
/// </remarks>
let findMostlyCompleteSubnets prefixLength (ips: IPAddress seq) =    
    let completenessThreshold =
        (subnetSize prefixLength) + prefixLength - 32
    
    query {
        for ip in ips do
        groupBy (getNetAddress ip prefixLength) into net
        where (net.Count() >= completenessThreshold)
        select (IPNetwork(intToIP net.Key, prefixLength))
    }
    
/// <example>
/// <code>
/// tryCombine (IPNetwork.Parse("192.168.0.0/24")) (IPNetwork.Parse("192.168.1.0/24"))
/// </code>
/// yields Some 192.168.0.0/23
/// </example>
let tryCombine (net1: IPNetwork) (net2: IPNetwork) =
    if net1.PrefixLength = net2.PrefixLength then
        let supPrefixLength = net1.PrefixLength - 1
        let sup1 = getNetAddress net1.BaseAddress supPrefixLength
        let sup2 = getNetAddress net2.BaseAddress supPrefixLength
        if sup1 = sup2 then Some(IPNetwork(intToIP sup1, supPrefixLength)) else None
    else
        None
    
let collapse (nets: IPNetwork seq) = 
    let rec foldNet acc net =
        match acc with
        | [] -> [ net ]
        | x :: xs ->
            match tryCombine net x with
            | Some supernet -> foldNet xs supernet                      // backtrack
            | None when x.Contains(net.BaseAddress) -> x :: xs          // skip
            | _ -> net :: x :: xs

    nets
    |> Seq.sortBy (fun net -> (ipToInt net.BaseAddress, net.PrefixLength))
    |> Seq.fold foldNet []
    |> Seq.rev
    
let getFirstByte (ip: IPAddress) =
    ip.GetAddressBytes()[0]
    
let toInt = function
    | IP ip -> ipToInt ip
    | Cidr cidr -> ipToInt cidr.BaseAddress
    
let goForIt path prefixLength =
    let ips = Seq.cache (readIPAddresses path)
    
    let netsByFB =
        ips
        |> findMostlyCompleteSubnets prefixLength
        |> collapse
        |> Seq.groupBy (fun net -> getFirstByte net.BaseAddress)
        |> Map.ofSeq
        
    let resolveToSubnet ip =
        Map.tryFind (getFirstByte ip) netsByFB
        |> Option.bind (Seq.tryFind _.Contains(ip))
        |> function
            | Some net -> Cidr net
            | None -> IP ip
    
    ips
    |> Seq.map resolveToSubnet
    |> Seq.distinct
    |> Seq.sortBy toInt
    |> Seq.iter (printfn "%O")
    
[<EntryPoint>]
let main argv =
    match argv with
    | [| path |] ->
        goForIt path 29
        0
    | [| path; prefixLength |] ->
        goForIt path (Int32.Parse(prefixLength))
        0
    | _ ->
        printfn "Usage: ipmerge <path> [length]"
        1